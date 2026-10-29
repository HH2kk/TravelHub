using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using Dapper;
using Newtonsoft.Json;
using TravelHub.Data;
using TravelHub.Models;

namespace TravelHub.Services
{
    public static class BookingService
    {
        const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        static string NewCode()
        {
            var b = new byte[6];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(b);
            return "TH" + DateTime.Now.ToString("yyMMdd") + new string(b.Select(x => CodeAlphabet[x % CodeAlphabet.Length]).ToArray());
        }

        // ------------------------------------------------------------ báo giá
        public static void ValidateStay(DateTime checkIn, DateTime checkOut)
        {
            if (checkIn.Date >= checkOut.Date) throw new BookingException("Ngày nhận phòng phải trước ngày trả phòng.");
            if (checkIn.Date < DateTime.Today) throw new BookingException("Không thể đặt phòng cho ngày trong quá khứ.");
            if ((checkOut.Date - checkIn.Date).TotalDays > 30) throw new BookingException("Thời gian lưu trú tối đa 30 đêm.");
            if (checkIn.Date > DateTime.Today.AddYears(1)) throw new BookingException("Chỉ nhận đặt phòng trong vòng 1 năm.");
        }

        public class CouponInfo { public int MaKM; public decimal? Pct; public decimal? Amt; public decimal? Min; public decimal? Max; public int? Qty; public int Used; public int? PerCustomer; public int? HotelId; }

        public static PriceQuote Quote(IDbConnection c, IDbTransaction tx, int roomId, DateTime checkIn, DateTime checkOut, int rooms, string couponCode, int? customerId, out CouponInfo coupon, out string couponMessage)
        {
            coupon = null; couponMessage = null;
            var basePrice = c.ExecuteScalar<decimal>("SELECT GiaPhong FROM Phong WHERE MaPhong=@r", new { r = roomId }, tx);
            var overrides = c.Query<NightPrice>("SELECT Ngay AS Date, Gia AS Price FROM GiaPhongTheoNgay WHERE MaPhong=@r AND Ngay >= @a AND Ngay < @b",
                new { r = roomId, a = checkIn.Date, b = checkOut.Date }, tx).ToDictionary(x => x.Date.Date, x => x.Price);
            var nights = new List<NightPrice>();
            for (var d = checkIn.Date; d < checkOut.Date; d = d.AddDays(1))
                nights.Add(new NightPrice { Date = d, Price = overrides.ContainsKey(d) ? overrides[d] : basePrice });

            decimal discount = 0;
            var subtotal = nights.Sum(n => n.Price) * rooms;
            if (!string.IsNullOrWhiteSpace(couponCode))
            {
                var code = couponCode.Trim();
                coupon = c.QueryFirstOrDefault<CouponInfo>(@"SELECT MaKM, PhanTramGiam Pct, SoTienGiam Amt, GiaTriToiThieu [Min], GiamToiDa [Max], SoLuong Qty, SoLuongDaDung Used, SoLanToiDaMoiKH PerCustomer, MaKS HotelId
                    FROM KhuyenMai WHERE MaCode=@c AND TrangThai=1 AND (NgayBatDau IS NULL OR NgayBatDau<=GETDATE()) AND (NgayKetThuc IS NULL OR NgayKetThuc>=GETDATE())", new { c = code }, tx);
                if (coupon == null) couponMessage = "Mã khuyến mãi không hợp lệ hoặc đã hết hạn.";
                else if (coupon.Qty.HasValue && coupon.Used >= coupon.Qty.Value) { couponMessage = "Mã khuyến mãi đã hết lượt sử dụng."; coupon = null; }
                else if (coupon.PerCustomer.HasValue && customerId.HasValue &&
                         c.ExecuteScalar<int>("SELECT COUNT(*) FROM KhuyenMai_SuDung WHERE MaKM=@k AND MaKH=@h", new { k = coupon.MaKM, h = customerId }, tx) >= coupon.PerCustomer.Value)
                { couponMessage = "Bạn đã dùng hết số lần cho mã này."; coupon = null; }
                else
                {
                    discount = PricingCalculator.CouponDiscount(subtotal, coupon.Pct, coupon.Amt, coupon.Min, coupon.Max);
                    if (discount <= 0) { couponMessage = "Đơn chưa đủ điều kiện áp dụng mã này."; coupon = null; }
                    else couponMessage = "Đã áp dụng mã " + code + ".";
                }
            }
            var q = PricingCalculator.Calculate(nights, rooms, discount, AppConfig.SettingDecimal("ThueVAT", 0.08m), AppConfig.SettingDecimal("PhuPhiDichVu", 0m));
            q.CouponCode = coupon != null ? couponCode.Trim() : null;
            return q;
        }

        // ------------------------------------------------------------ tạo đơn
        public static BookingDetail Create(int customerId, BookingRequest r)
        {
            ValidateStay(r.CheckIn, r.CheckOut);
            if (r.Rooms < 1 || r.Rooms > 10) throw new BookingException("Số phòng không hợp lệ.");
            if (r.Adults < 1) throw new BookingException("Cần ít nhất 1 người lớn.");
            if (!r.AcceptTerms) throw new BookingException("Vui lòng đồng ý với điều kiện đặt phòng và chính sách hủy.");

            using (var c = TravelDb.Open())
            using (var tx = c.BeginTransaction())
            {
                // Khóa dòng Phong → các đặt phòng đồng thời cho cùng loại phòng được xếp hàng tuần tự.
                var room = c.QueryFirstOrDefault(@"SELECT p.MaPhong, p.SoNguoiToiDa, ks.ChinhSachHuyLoai, ks.GioNhanPhong
                    FROM Phong p WITH (UPDLOCK, ROWLOCK) INNER JOIN KhachSan ks ON ks.MaKS = p.MaKS
                    WHERE p.MaPhong=@r AND p.MaKS=@h AND p.TrangThai=1 AND ks.TrangThai=1", new { r = r.RoomId, h = r.HotelId }, tx);
                if (room == null) throw new BookingException("Phòng không còn mở bán.");
                if (r.Adults + r.Children > (int)room.SoNguoiToiDa * r.Rooms) throw new BookingException("Số khách vượt quá sức chứa của " + r.Rooms + " phòng.");

                var avail = c.ExecuteScalar<int>("SELECT dbo.fn_PhongConLai(@r,@a,@b)", new { r = r.RoomId, a = r.CheckIn.Date, b = r.CheckOut.Date }, tx);
                if (avail < r.Rooms) throw new BookingException(avail == 0 ? "Rất tiếc, loại phòng này đã hết trong khoảng ngày bạn chọn." : "Chỉ còn " + avail + " phòng trong khoảng ngày bạn chọn.");

                CouponInfo coupon; string cmsg;
                var q = Quote(c, tx, r.RoomId, r.CheckIn, r.CheckOut, r.Rooms, r.CouponCode, customerId, out coupon, out cmsg);
                string policy = room.ChinhSachHuyLoai;
                var freeUntil = CancellationPolicy.FreeCancelDeadline(r.CheckIn, (string)room.GioNhanPhong);
                var expires = DateTime.Now.AddMinutes(AppConfig.HoldMinutes);

                int id = 0; string code = null;
                for (int attempt = 0; attempt < 5 && id == 0; attempt++)
                {
                    code = NewCode();
                    try
                    {
                        id = c.ExecuteScalar<int>(@"
INSERT INTO DonDatPhong (MaDonCode, MaKH, MaKS, MaKM, NgayNhanPhong, NgayTraPhong, SoNguoiLon, SoTreEm, TongTien, GiamGia, ThanhTien,
                         TrangThai, TrangThaiCode, NgayHetHan, GhiChu, HoTenKhach, EmailKhach, SdtKhach, SnapshotGia, ChinhSachHuy, HuyMienPhiDen, NgayDat)
VALUES (@code,@kh,@ks,@km,@in,@out,@adt,@chd,@sub,@disc,@total,@st,'Pending',@exp,@note,@gn,@ge,@gp,@snap,@pol,@free,GETDATE());
SELECT CAST(SCOPE_IDENTITY() AS int);",
                            new { code, kh = customerId, ks = r.HotelId, km = coupon == null ? (int?)null : coupon.MaKM, @in = r.CheckIn.Date, @out = r.CheckOut.Date,
                                  adt = r.Adults, chd = r.Children, sub = q.Subtotal, disc = q.Discount, total = q.Total, st = BookingStatus.Pending, exp = expires,
                                  note = r.Note, gn = r.GuestName.Trim(), ge = r.GuestEmail.Trim(), gp = r.GuestPhone.Trim(),
                                  snap = JsonConvert.SerializeObject(q), pol = policy, free = freeUntil }, tx);
                    }
                    catch (System.Data.SqlClient.SqlException ex) when (ex.Number == 2601 || ex.Number == 2627) { id = 0; }
                }
                if (id == 0) throw new BookingException("Không thể tạo mã đặt phòng, vui lòng thử lại.");

                c.Execute("INSERT INTO ChiTietDatPhong (MaDon, MaPhong, SoLuong, DonGia, SoDem) VALUES (@d,@p,@q,@g,@n)",
                    new { d = id, p = r.RoomId, q = r.Rooms, g = q.AverageNight, n = q.NightCount }, tx);
                if (coupon != null)
                {
                    c.Execute("UPDATE KhuyenMai SET SoLuongDaDung = SoLuongDaDung + 1 WHERE MaKM=@k", new { k = coupon.MaKM }, tx);
                    c.Execute("INSERT INTO KhuyenMai_SuDung (MaKM, MaKH, MaDon, NgaySuDung, SoTienGiam) VALUES (@k,@h,@d,GETDATE(),@s)", new { k = coupon.MaKM, h = customerId, d = id, s = q.Discount }, tx);
                }
                AddHistory(c, tx, id, null, BookingStatus.Pending, null, "Tạo đơn, giữ phòng " + AppConfig.HoldMinutes + " phút");
                tx.Commit();
                return Get(id, customerId, false);
            }
        }

        public static void AddHistory(IDbConnection c, IDbTransaction tx, int bookingId, string from, string to, int? accountId, string reason)
        {
            c.Execute("INSERT INTO LichSuTrangThaiDon (MaDon, TuTrangThai, DenTrangThai, MaTaiKhoan, LyDo) VALUES (@d,@f,@t,@a,@r)",
                new { d = bookingId, f = from, t = to, a = accountId, r = reason }, tx);
        }

        // ------------------------------------------------------------ đọc đơn
        /// <param name="customerId">null = không giới hạn theo chủ đơn (chỉ dùng cho nhân viên đã kiểm quyền).</param>
        public static BookingDetail Get(int id, int? customerId, bool staff)
        {
            using (var c = TravelDb.Open())
            {
                var d = c.QueryFirstOrDefault<BookingDetail>(@"
SELECT o.MaDon Id, o.MaKH CustomerId, o.MaDonCode Code, o.TrangThai Status, o.MaKS HotelId, ks.TenKS HotelName, ks.DiaChi HotelAddress, ks.SoDienThoai HotelPhone,
       ct.MaPhong RoomId, p.TenPhong RoomName, p.LoaiPhong RoomType, ISNULL(ct.SoLuong,1) Quantity, o.NgayNhanPhong CheckIn, o.NgayTraPhong CheckOut,
       o.SoNguoiLon Adults, o.SoTreEm Children, ISNULL(o.HoTenKhach, kh.HoTen) GuestName, ISNULL(o.EmailKhach, kh.Email) GuestEmail, ISNULL(o.SdtKhach, kh.SoDienThoai) GuestPhone,
       o.GhiChu Note, o.GhiChuNoiBo InternalNote, o.TongTien Subtotal, o.GiamGia Discount, o.ThanhTien Total, o.ChinhSachHuy CancelPolicyText, ISNULL(ks.ChinhSachHuyLoai,'FLEX1') CancelPolicyCode,
       ks.GioNhanPhong CheckInTime, ks.GioTraPhong CheckOutTime, o.NgayHetHan ExpiresAt, o.NgayDat CreatedAt, o.LyDoHuy CancelReason, o.SnapshotGia SnapshotJson, ks.HinhAnhDaiDien CoverImage,
       CAST(CASE WHEN EXISTS(SELECT 1 FROM DanhGia g WHERE g.MaDon=o.MaDon) THEN 1 ELSE 0 END AS bit) HasReview
FROM DonDatPhong o
INNER JOIN KhachSan ks ON ks.MaKS=o.MaKS
INNER JOIN KhachHang kh ON kh.MaKH=o.MaKH
LEFT JOIN ChiTietDatPhong ct ON ct.MaDon=o.MaDon
LEFT JOIN Phong p ON p.MaPhong=ct.MaPhong
WHERE o.MaDon=@id AND (@staff=1 OR o.MaKH=@kh)", new { id, staff, kh = customerId ?? 0 });
                if (d == null) return null;
                if (!string.IsNullOrEmpty(d.SnapshotJson))
                    try { d.Quote = JsonConvert.DeserializeObject<PriceQuote>(d.SnapshotJson); } catch { }
                d.CancelPolicyText = CancellationPolicy.Describe(d.CancelPolicyCode);
                d.Payments = c.Query<PaymentRow>(@"SELECT MaThanhToan, SoTien, PhuongThuc, MaGiaoDich, MaThamChieuVNPAY, TrangThai, TrangThaiCode, LoaiGiaoDich, NgayThanhToan, NgayTao
                                                   FROM ThanhToan WHERE MaDon=@id ORDER BY MaThanhToan DESC", new { id }).ToList();
                d.PaidAmount = d.Payments.Where(p => p.TrangThai == PaymentStatus.Succeeded || p.TrangThai == "Đã thanh toán").Sum(p => p.SoTien);
                d.History = c.Query<HistoryRow>(@"SELECT h.TuTrangThai, h.DenTrangThai, h.LyDo, tk.TenDangNhap NguoiThucHien, CAST(h.NgayTao AS datetime) NgayTao
                                                  FROM LichSuTrangThaiDon h LEFT JOIN TaiKhoan tk ON tk.MaTaiKhoan=h.MaTaiKhoan WHERE h.MaDon=@id ORDER BY h.MaLichSu", new { id }).ToList();
                d.Refund = c.QueryFirstOrDefault<RefundRow>(@"SELECT TOP 1 MaYeuCau, MaDon, SoTien, LyDo, TrangThai, MaGiaoDichHoan, LyDoTuChoi, CAST(NgayTao AS datetime) NgayTao, CAST(NgayXuLy AS datetime) NgayXuLy
                                                              FROM YeuCauHoanTien WHERE MaDon=@id ORDER BY MaYeuCau DESC", new { id });
                return d;
            }
        }

        public static List<BookingListRow> ListForCustomer(int customerId)
        {
            using (var c = TravelDb.Open())
                return c.Query<BookingListRow>(@"
SELECT o.MaDon Id, o.MaDonCode Code, ks.TenKS HotelName, p.TenPhong RoomName, o.NgayNhanPhong CheckIn, o.NgayTraPhong CheckOut, o.ThanhTien Total,
       o.TrangThai Status, o.NgayDat CreatedAt, o.NgayHetHan ExpiresAt, ks.HinhAnhDaiDien CoverImage
FROM DonDatPhong o INNER JOIN KhachSan ks ON ks.MaKS=o.MaKS
LEFT JOIN ChiTietDatPhong ct ON ct.MaDon=o.MaDon LEFT JOIN Phong p ON p.MaPhong=ct.MaPhong
WHERE o.MaKH=@k ORDER BY o.NgayDat DESC", new { k = customerId }).ToList();
        }

        // ------------------------------------------------------------ hủy đơn
        public class CancelResult { public decimal RefundAmount; public string Message; public bool RefundRequested; }

        public static CancelResult Cancel(int bookingId, int? customerId, bool staff, int accountId, string reason, bool forceFullRefund = false)
        {
            BookingDetail snapshot;
            var res = new CancelResult();
            using (var c = TravelDb.Open())
            using (var tx = c.BeginTransaction())
            {
                var o = c.QueryFirstOrDefault(@"SELECT o.MaDon, o.MaKH, o.TrangThai, o.MaKM, o.NgayNhanPhong, ks.GioNhanPhong, ks.ChinhSachHuyLoai, o.SnapshotGia
                    FROM DonDatPhong o WITH (UPDLOCK, ROWLOCK) INNER JOIN KhachSan ks ON ks.MaKS=o.MaKS
                    WHERE o.MaDon=@id AND (@staff=1 OR o.MaKH=@kh)", new { id = bookingId, staff, kh = customerId ?? 0 }, tx);
                if (o == null) throw new BookingException("Không tìm thấy đơn đặt phòng.");
                string status = o.TrangThai;
                if (!BookingStatus.CanTransition(status, BookingStatus.Cancelled))
                    throw new BookingException("Đơn ở trạng thái “" + status + "” không thể hủy.");

                decimal paid = c.ExecuteScalar<decimal>(@"SELECT ISNULL(SUM(SoTien),0) FROM ThanhToan WHERE MaDon=@id AND TrangThai IN (N'Thành công', N'Đã thanh toán')", new { id = bookingId }, tx);
                RefundDecision dec = null;
                if (paid > 0)
                {
                    decimal firstNight = 0;
                    try
                    {
                        var q = JsonConvert.DeserializeObject<PriceQuote>((string)o.SnapshotGia);
                        if (q != null && q.Nights.Count > 0)
                        {
                            var n = q.Nights[0].Price * q.Quantity;
                            firstNight = Math.Round(n * (1 + q.TaxRate) * (1 + q.ServiceFeeRate), 0);
                        }
                    }
                    catch { }
                    dec = CancellationPolicy.Evaluate((string)o.ChinhSachHuyLoai, (DateTime)o.NgayNhanPhong, (string)o.GioNhanPhong, DateTime.Now, paid, firstNight);
                    if (forceFullRefund && staff) { dec.Amount = paid; dec.Description = "Nhân viên duyệt hoàn 100%."; }
                }

                c.Execute(@"UPDATE DonDatPhong SET TrangThai=@st, TrangThaiCode='Cancelled', LyDoHuy=@r, NgayHetHan=NULL, NgayCapNhat=GETDATE() WHERE MaDon=@id",
                    new { st = BookingStatus.Cancelled, r = reason, id = bookingId }, tx);
                AddHistory(c, tx, bookingId, status, BookingStatus.Cancelled, accountId, string.IsNullOrWhiteSpace(reason) ? "Hủy đơn" : reason);

                if (o.MaKM != null)
                {
                    c.Execute("UPDATE KhuyenMai SET SoLuongDaDung = CASE WHEN SoLuongDaDung>0 THEN SoLuongDaDung-1 ELSE 0 END WHERE MaKM=@k", new { k = (int)o.MaKM }, tx);
                    c.Execute("DELETE FROM KhuyenMai_SuDung WHERE MaDon=@d", new { d = bookingId }, tx);
                }
                // Vô hiệu các giao dịch đang chờ để không thể thanh toán tiếp.
                c.Execute(@"UPDATE ThanhToan SET TrangThai=N'Cần đối soát', TrangThaiCode='SUPERSEDED' WHERE MaDon=@d AND TrangThai=N'Đang xử lý'", new { d = bookingId }, tx);

                if (dec != null && dec.Amount > 0)
                {
                    var payId = c.ExecuteScalar<int?>("SELECT TOP 1 MaThanhToan FROM ThanhToan WHERE MaDon=@id AND TrangThai IN (N'Thành công', N'Đã thanh toán') ORDER BY MaThanhToan DESC", new { id = bookingId }, tx);
                    c.Execute("INSERT INTO YeuCauHoanTien (MaDon, MaThanhToan, SoTien, LyDo, TrangThai) VALUES (@d,@p,@s,@l,'PENDING')",
                        new { d = bookingId, p = payId, s = dec.Amount, l = dec.Description + (string.IsNullOrWhiteSpace(reason) ? "" : " Lý do khách: " + reason) }, tx);
                    res.RefundAmount = dec.Amount; res.RefundRequested = true;
                    res.Message = "Đã hủy đơn. Yêu cầu hoàn " + dec.Amount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")) + "₫ đang chờ xử lý.";
                }
                else if (paid > 0) res.Message = "Đã hủy đơn. " + (dec != null ? dec.Description : "");
                else res.Message = "Đã hủy đơn.";
                tx.Commit();
            }
            AppLog.Audit(accountId, "BOOKING_CANCEL", "DonDatPhong", bookingId.ToString(), reason);
            snapshot = Get(bookingId, null, true);
            BookingNotifier.Cancelled(snapshot, res.RefundAmount);
            return res;
        }

        /// <summary>Chuyển đơn "Chờ thanh toán" quá hạn sang "Hết hạn" (giải phóng mã giảm giá). Tồn phòng đã tự giải phóng theo NgayHetHan.</summary>
        public static int ExpireHolds()
        {
            using (var c = TravelDb.Open())
            {
                // Giữ thêm 2 phút ân hạn cho giao dịch đang ở cổng thanh toán trước khi đổi trạng thái hiển thị.
                var ids = c.Query<int>(@"SELECT MaDon FROM DonDatPhong WHERE TrangThai IN (N'Chờ thanh toán') AND NgayHetHan IS NOT NULL AND NgayHetHan < DATEADD(minute,-2,GETDATE())").ToList();
                int n = 0;
                foreach (var id in ids)
                {
                    using (var tx = c.BeginTransaction())
                    {
                        var o = c.QueryFirstOrDefault("SELECT MaKM FROM DonDatPhong WITH (UPDLOCK) WHERE MaDon=@id AND TrangThai=N'Chờ thanh toán' AND NgayHetHan < DATEADD(minute,-2,GETDATE())", new { id }, tx);
                        if (o == null) { tx.Rollback(); continue; }
                        c.Execute("UPDATE DonDatPhong SET TrangThai=@st, TrangThaiCode='Expired', NgayCapNhat=GETDATE() WHERE MaDon=@id", new { st = BookingStatus.Expired, id }, tx);
                        AddHistory(c, tx, id, BookingStatus.Pending, BookingStatus.Expired, null, "Hết thời gian giữ phòng");
                        if (o.MaKM != null)
                        {
                            c.Execute("UPDATE KhuyenMai SET SoLuongDaDung = CASE WHEN SoLuongDaDung>0 THEN SoLuongDaDung-1 ELSE 0 END WHERE MaKM=@k", new { k = (int)o.MaKM }, tx);
                            c.Execute("DELETE FROM KhuyenMai_SuDung WHERE MaDon=@d", new { d = id }, tx);
                        }
                        tx.Commit(); n++;
                    }
                    var d = Get(id, null, true);
                    if (d != null) BookingNotifier.Expired(d);
                }
                return n;
            }
        }
    }
}
