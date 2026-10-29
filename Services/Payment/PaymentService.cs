using System;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using Dapper;
using TravelHub.Data;
using TravelHub.Models;

namespace TravelHub.Services.Payment
{
    public class CallbackOutcome
    {
        public string RspCode { get; set; } = "99";      // mã trả cho VNPay IPN
        public string Message { get; set; }
        public int? BookingId { get; set; }
        public bool SignatureValid { get; set; }
        public bool NewlyConfirmed { get; set; }
    }

    public static class PaymentService
    {
        const int MaxHoldMinutes = 60;

        // ------------------------------------------------------------ khởi tạo thanh toán
        public static string StartPayment(int bookingId, int customerId, string gatewayName, string ip, string returnUrl)
        {
            var gw = Gateways.Get(gatewayName);
            if (gw == null) throw new BookingException("Phương thức thanh toán không hợp lệ.");
            if (!gw.IsConfigured) throw new BookingException("Cổng thanh toán " + gw.DisplayName + " chưa được cấu hình trên hệ thống.");

            string txnRef; decimal amount; DateTime expire;
            using (var c = TravelDb.Open())
            using (var tx = c.BeginTransaction())
            {
                var o = c.QueryFirstOrDefault(@"SELECT TrangThai, ThanhTien, NgayHetHan, NgayDat, MaDonCode FROM DonDatPhong WITH (UPDLOCK, ROWLOCK)
                                                WHERE MaDon=@id AND MaKH=@kh", new { id = bookingId, kh = customerId }, tx);
                if (o == null) throw new BookingException("Không tìm thấy đơn đặt phòng.");
                if ((string)o.TrangThai != BookingStatus.Pending && (string)o.TrangThai != BookingStatus.LegacyPending)
                    throw new BookingException("Đơn không ở trạng thái chờ thanh toán.");
                if (o.NgayHetHan != null && (DateTime)o.NgayHetHan < DateTime.Now)
                    throw new BookingException("Đã hết thời gian giữ phòng. Vui lòng đặt lại.");
                amount = (decimal)o.ThanhTien;
                if (amount <= 0) throw new BookingException("Số tiền thanh toán không hợp lệ.");

                // Gia hạn giữ phòng đủ thời gian ra cổng thanh toán (tối đa 60 phút kể từ lúc tạo đơn).
                var cap = ((DateTime)o.NgayDat).AddMinutes(MaxHoldMinutes);
                expire = DateTime.Now.AddMinutes(15); if (expire > cap) expire = cap;
                if (expire <= DateTime.Now) throw new BookingException("Đã quá thời gian tối đa giữ phòng. Vui lòng đặt lại.");
                c.Execute("UPDATE DonDatPhong SET NgayHetHan = CASE WHEN NgayHetHan < @e THEN @e ELSE NgayHetHan END WHERE MaDon=@id", new { e = expire, id = bookingId }, tx);

                // Các giao dịch đang chờ cũ bị vô hiệu; nếu cổng báo thành công muộn sẽ vào luồng đối soát.
                c.Execute(@"UPDATE ThanhToan SET TrangThai=@s, TrangThaiCode='SUPERSEDED', NgayCapNhat=GETDATE()
                            WHERE MaDon=@id AND TrangThai=@p", new { s = PaymentStatus.Reconcile, p = PaymentStatus.Processing, id = bookingId }, tx);

                txnRef = ("TH" + bookingId + "-" + DateTime.Now.ToString("yyMMddHHmmss") + Guid.NewGuid().ToString("N").Substring(0, 6)).ToUpperInvariant();
                c.Execute(@"INSERT INTO ThanhToan (MaDon, SoTien, PhuongThuc, MaGiaoDich, MaThamChieuVNPAY, Cong, TrangThai, TrangThaiCode, LoaiGiaoDich, TienTe, NgayTao)
                            VALUES (@id,@amt,@pm,@ref,@ref,@gw,@st,'PENDING','PAYMENT','VND',GETDATE())",
                    new { id = bookingId, amt = amount, pm = gw.Name, @ref = txnRef, gw = gw.Name, st = PaymentStatus.Processing }, tx);
                tx.Commit();
            }
            return gw.BuildPaymentUrl(new PaymentRequest
            {
                TxnRef = txnRef, Amount = amount, IpAddress = ip, ReturnUrl = returnUrl, ExpireAt = expire,
                OrderInfo = "Thanh toan dat phong " + txnRef
            });
        }

        // ------------------------------------------------------------ callback (IPN và return URL dùng chung — idempotent)
        public static CallbackOutcome HandleCallback(IPaymentGateway gw, NameValueCollection query, string source)
        {
            var cb = gw.ParseCallback(query);
            var outcome = new CallbackOutcome { SignatureValid = cb.SignatureValid };
            int? paymentId = null;
            try
            {
                if (!cb.SignatureValid) { outcome.RspCode = "97"; outcome.Message = "Invalid signature"; return outcome; }
                if (string.IsNullOrWhiteSpace(cb.TxnRef)) { outcome.RspCode = "01"; outcome.Message = "Order not found"; return outcome; }

                using (var c = TravelDb.Open())
                using (var tx = c.BeginTransaction(IsolationLevel.Serializable))
                {
                    var pay = c.QueryFirstOrDefault(@"SELECT MaThanhToan, MaDon, SoTien, TrangThai, TrangThaiCode FROM ThanhToan WITH (UPDLOCK, HOLDLOCK) WHERE MaThamChieuVNPAY=@r", new { r = cb.TxnRef }, tx);
                    if (pay == null) { outcome.RspCode = "01"; outcome.Message = "Order not found"; tx.Commit(); return outcome; }
                    paymentId = pay.MaThanhToan; int bookingId = pay.MaDon; outcome.BookingId = bookingId;

                    if ((decimal)pay.SoTien * 100m != cb.AmountMinor) { outcome.RspCode = "04"; outcome.Message = "Invalid amount"; tx.Commit(); return outcome; }

                    string pStatus = pay.TrangThai, pCode = pay.TrangThaiCode;
                    if (pStatus == PaymentStatus.Succeeded) { outcome.RspCode = "02"; outcome.Message = "Order already confirmed"; outcome.BookingId = bookingId; tx.Commit(); return outcome; }
                    if (pStatus == PaymentStatus.Failed) { outcome.RspCode = "02"; outcome.Message = "Order already confirmed"; tx.Commit(); return outcome; }

                    var failCode = cb.ResponseCode != "00" ? cb.ResponseCode : cb.TransactionStatus;
                    bool superseded = pCode == "SUPERSEDED";

                    if (!cb.Success)
                    {
                        c.Execute(@"UPDATE ThanhToan SET TrangThai=@s, TrangThaiCode=@code, MaPhanHoiCong=@rc, MaGiaoDichCong=@no, NgayCapNhat=GETDATE() WHERE MaThanhToan=@id",
                            new { s = PaymentStatus.Failed, code = string.IsNullOrWhiteSpace(failCode) ? "FAILED" : failCode, rc = cb.ResponseCode, no = cb.GatewayTxnNo, id = (int)pay.MaThanhToan }, tx);
                        outcome.RspCode = "00"; outcome.Message = "Confirm Success";
                        tx.Commit(); return outcome;
                    }

                    // ---- Cổng báo THÀNH CÔNG, đã xác minh chữ ký + số tiền ----
                    var o = c.QueryFirstOrDefault(@"SELECT o.TrangThai, o.NgayHetHan, o.NgayNhanPhong, o.NgayTraPhong, o.SoNguoiLon, ct.MaPhong, ISNULL(ct.SoLuong,1) Sl
                        FROM DonDatPhong o WITH (UPDLOCK, ROWLOCK) LEFT JOIN ChiTietDatPhong ct ON ct.MaDon=o.MaDon WHERE o.MaDon=@id", new { id = bookingId }, tx);
                    string bStatus = o.TrangThai;
                    bool stillHeld = (bStatus == BookingStatus.Pending || bStatus == BookingStatus.LegacyPending);

                    if (bStatus == BookingStatus.Paid || bStatus == BookingStatus.CheckedIn || bStatus == BookingStatus.Completed)
                    {
                        // Đơn đã được thanh toán bởi giao dịch khác → giao dịch này trùng, cần đối soát/hoàn.
                        MarkReconcile(c, tx, (int)pay.MaThanhToan, cb, "DUPLICATE");
                        QueueRefund(c, tx, bookingId, (int)pay.MaThanhToan, (decimal)pay.SoTien, "Thanh toán trùng — đơn đã được thanh toán trước đó.");
                        outcome.RspCode = "00"; outcome.Message = "Confirm Success"; tx.Commit(); return outcome;
                    }

                    bool holdValid = stillHeld && (o.NgayHetHan == null || (DateTime)o.NgayHetHan >= DateTime.Now);
                    bool canConfirm = holdValid;
                    if (!canConfirm)
                    {
                        // Hold đã quá hạn (phòng có thể đã bán cho người khác) hoặc đơn đã Hết hạn nhưng tiền đã về:
                        // chỉ xác nhận khi còn phòng (khóa dòng Phong để chống tranh chấp).
                        if ((stillHeld || bStatus == BookingStatus.Expired) && o.MaPhong != null)
                        {
                            c.Execute("SELECT 1 FROM Phong WITH (UPDLOCK, ROWLOCK) WHERE MaPhong=@p", new { p = (int)o.MaPhong }, tx);
                            var avail = c.ExecuteScalar<int>("SELECT dbo.fn_PhongConLai(@p,@a,@b)", new { p = (int)o.MaPhong, a = (DateTime)o.NgayNhanPhong, b = (DateTime)o.NgayTraPhong }, tx);
                            canConfirm = avail >= (int)o.Sl;
                        }
                    }

                    if (!canConfirm)
                    {
                        MarkReconcile(c, tx, (int)pay.MaThanhToan, cb, "LATE_NO_STOCK");
                        QueueRefund(c, tx, bookingId, (int)pay.MaThanhToan, (decimal)pay.SoTien, "Thanh toán đến sau khi đơn " + bStatus.ToLowerInvariant() + " và phòng không còn trống — hoàn toàn bộ.");
                        BookingService.AddHistory(c, tx, bookingId, bStatus, bStatus, null, "Nhận thanh toán muộn, không còn phòng → chờ hoàn tiền");
                        outcome.RspCode = "00"; outcome.Message = "Confirm Success"; tx.Commit(); return outcome;
                    }

                    c.Execute(@"UPDATE ThanhToan SET TrangThai=@s, TrangThaiCode='00', MaPhanHoiCong=@rc, MaGiaoDichCong=@no, NgayThanhToan=GETDATE(), NgayCapNhat=GETDATE() WHERE MaThanhToan=@id",
                        new { s = PaymentStatus.Succeeded, rc = cb.ResponseCode, no = cb.GatewayTxnNo, id = (int)pay.MaThanhToan }, tx);
                    c.Execute("UPDATE DonDatPhong SET TrangThai=@s, TrangThaiCode='Paid', NgayHetHan=NULL, NgayCapNhat=GETDATE(), PhuongThucThanhToan=@pm WHERE MaDon=@id",
                        new { s = BookingStatus.Paid, pm = gw.Name, id = bookingId }, tx);
                    BookingService.AddHistory(c, tx, bookingId, bStatus, BookingStatus.Paid, null,
                        superseded ? "Thanh toán thành công (giao dịch đến muộn, vẫn còn phòng)" : "Thanh toán thành công qua " + gw.Name);
                    // Các giao dịch chờ khác của đơn không còn hiệu lực.
                    c.Execute(@"UPDATE ThanhToan SET TrangThai=@r, TrangThaiCode='SUPERSEDED' WHERE MaDon=@id AND MaThanhToan<>@pid AND TrangThai=@p",
                        new { r = PaymentStatus.Reconcile, id = bookingId, pid = (int)pay.MaThanhToan, p = PaymentStatus.Processing }, tx);
                    outcome.NewlyConfirmed = true; outcome.RspCode = "00"; outcome.Message = "Confirm Success";
                    tx.Commit();
                }
            }
            catch (Exception ex)
            {
                AppLog.Error("PaymentCallback", ex);
                outcome.RspCode = "99"; outcome.Message = "Unknown error";
            }
            finally
            {
                LogCallback(paymentId, gw.Name, source, cb, outcome, query);
            }

            if (outcome.NewlyConfirmed && outcome.BookingId.HasValue)
            {
                try { BookingNotifier.PaymentConfirmed(outcome.BookingId.Value, paymentId); }
                catch (Exception ex) { AppLog.Error("PostPayment", ex); }  // lỗi email/PDF không được làm mất giao dịch đã xác nhận
            }
            else if (!outcome.NewlyConfirmed && outcome.BookingId.HasValue && cb.SignatureValid && !cb.Success)
            {
                try { BookingNotifier.PaymentFailed(outcome.BookingId.Value, cb.TxnRef); } catch (Exception ex) { AppLog.Error("PaymentFailedMail", ex); }
            }
            return outcome;
        }

        static void MarkReconcile(IDbConnection c, IDbTransaction tx, int paymentId, CallbackResult cb, string code)
        {
            c.Execute(@"UPDATE ThanhToan SET TrangThai=@s, TrangThaiCode=@code, MaPhanHoiCong=@rc, MaGiaoDichCong=@no, NgayThanhToan=GETDATE(), NgayCapNhat=GETDATE() WHERE MaThanhToan=@id",
                new { s = PaymentStatus.Reconcile, code, rc = cb.ResponseCode, no = cb.GatewayTxnNo, id = paymentId }, tx);
        }

        static void QueueRefund(IDbConnection c, IDbTransaction tx, int bookingId, int paymentId, decimal amount, string reason)
        {
            // Chỉ một yêu cầu hoàn đang mở cho mỗi đơn (unique index) → bỏ qua nếu đã có.
            c.Execute(@"IF NOT EXISTS (SELECT 1 FROM YeuCauHoanTien WHERE MaDon=@d AND TrangThai IN ('PENDING','APPROVED','REFUNDED','MANUAL_REQUIRED'))
                        INSERT INTO YeuCauHoanTien (MaDon, MaThanhToan, SoTien, LyDo, TrangThai) VALUES (@d,@p,@s,@l,'PENDING')",
                new { d = bookingId, p = paymentId, s = amount, l = reason }, tx);
        }

        static void LogCallback(int? paymentId, string gw, string source, CallbackResult cb, CallbackOutcome o, NameValueCollection q)
        {
            try
            {
                // Không lưu chữ ký/secret; chỉ giữ các tham số vnp_* phục vụ đối soát.
                var data = string.Join("&", q.AllKeys.Where(k => k != null && k.StartsWith("vnp_") && !k.Contains("SecureHash")).Select(k => k + "=" + q[k]));
                if (data.Length > 1900) data = data.Substring(0, 1900);
                using (var c = TravelDb.Open())
                    c.Execute(@"INSERT INTO NhatKyThanhToan (MaThanhToan, Cong, Nguon, ThamChieu, ChuKyHopLe, MaKetQua, DuLieu) VALUES (@p,@g,@s,@r,@v,@k,@d)",
                        new { p = paymentId, g = gw, s = source, r = cb.TxnRef, v = cb.SignatureValid, k = o.RspCode, d = data });
            }
            catch (Exception ex) { AppLog.Error("LogCallback", ex); }
        }

        public static string StatusFor(int bookingId, int? customerId, bool staff)
        {
            var d = BookingService.Get(bookingId, customerId, staff);
            return d == null ? null : d.Status;
        }
    }
}
