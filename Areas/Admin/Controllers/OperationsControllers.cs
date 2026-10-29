using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Dapper;
using TravelHub.Data;
using TravelHub.Infrastructure;
using TravelHub.Models;
using TravelHub.Services;
using TravelHub.Services.Payment;

namespace TravelHub.Areas.Admin.Controllers
{
    [RoleAuthorize(Roles = "Admin,Staff,ContentManager")]
    public abstract class AdminController : AppController
    {
        protected static string Like(string s) { return string.IsNullOrWhiteSpace(s) ? null : "%" + s.Trim().Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%"; }
        protected void Audit(string action, string entity, string id, string desc) { AppLog.Audit(Me.AccountId, action, entity, id, desc); }
    }

    [RoleAuthorize(Roles = "Admin,Staff,ContentManager")]
    public class DashboardController : AdminController
    {
        public ActionResult Index(int days = 30)
        {
            if (days != 7 && days != 30 && days != 90) days = 30;
            ViewBag.Title = "Tổng quan";
            return View(DashboardService.Load(days));
        }
    }

    public class BookingFilter { public string code { get; set; } public string customer { get; set; } public string status { get; set; } public DateTime? from { get; set; } public DateTime? to { get; set; } }

    [RoleAuthorize(Roles = "Admin,Staff")]
    public class BookingsController : AdminController
    {
        const string BaseSql = @"FROM DonDatPhong o JOIN KhachSan ks ON ks.MaKS=o.MaKS JOIN KhachHang kh ON kh.MaKH=o.MaKH
            LEFT JOIN ChiTietDatPhong ct ON ct.MaDon=o.MaDon LEFT JOIN Phong p ON p.MaPhong=ct.MaPhong
            WHERE (@code IS NULL OR o.MaDonCode LIKE @code) AND (@cust IS NULL OR kh.HoTen LIKE @cust COLLATE Vietnamese_100_CI_AI OR kh.Email LIKE @cust OR o.EmailKhach LIKE @cust)
              AND (@status IS NULL OR o.TrangThai=@status) AND (@from IS NULL OR o.NgayNhanPhong>=@from) AND (@to IS NULL OR o.NgayNhanPhong<=@to)";

        object Args(string code, string customer, string status, DateTime? from, DateTime? to) { return new { code = Like(code), cust = Like(customer), status = string.IsNullOrEmpty(status) ? null : status, from, to }; }

        public ActionResult Index(string code, string customer, string status, DateTime? from, DateTime? to, int page = 1)
        {
            if (page < 1) page = 1; const int size = 20;
            using (var c = TravelDb.Open())
            {
                var a = new DynamicParameters(Args(code, customer, status, from, to)); a.Add("skip", (page - 1) * size); a.Add("take", size);
                ViewBag.Total = c.ExecuteScalar<int>("SELECT COUNT(*) " + BaseSql, a);
                var rows = c.Query<BookingListRow>(@"SELECT o.MaDon Id, o.MaDonCode Code, ks.TenKS HotelName, p.TenPhong RoomName, o.NgayNhanPhong CheckIn, o.NgayTraPhong CheckOut, o.ThanhTien Total, o.TrangThai Status,
                    ISNULL(o.HoTenKhach, kh.HoTen) CustomerName, ISNULL(o.EmailKhach, kh.Email) CustomerEmail, o.NgayDat CreatedAt, o.NgayHetHan ExpiresAt " + BaseSql + " ORDER BY o.NgayDat DESC OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY", a).ToList();
                ViewBag.Page = page; ViewBag.Size = size; ViewBag.Q = new BookingFilter { code = code, customer = customer, status = status, from = from, to = to };
                ViewBag.Title = "Đặt phòng";
                return View(rows);
            }
        }

        public ActionResult Export(string code, string customer, string status, DateTime? from, DateTime? to)
        {
            using (var c = TravelDb.Open())
            {
                var rows = c.Query<BookingListRow>(@"SELECT TOP 5000 o.MaDon Id, o.MaDonCode Code, ks.TenKS HotelName, p.TenPhong RoomName, o.NgayNhanPhong CheckIn, o.NgayTraPhong CheckOut, o.ThanhTien Total, o.TrangThai Status,
                    ISNULL(o.HoTenKhach, kh.HoTen) CustomerName, ISNULL(o.EmailKhach, kh.Email) CustomerEmail, o.NgayDat CreatedAt " + BaseSql + " ORDER BY o.NgayDat DESC", Args(code, customer, status, from, to)).ToList();
                var sb = new StringBuilder("﻿Mã đặt phòng,Khách,Email,Chỗ nghỉ,Phòng,Nhận phòng,Trả phòng,Tổng tiền,Trạng thái,Ngày đặt\r\n");
                Func<string, string> q = s => { s = s ?? ""; if (s.Length > 0 && "=+-@".IndexOf(s[0]) >= 0) s = "'" + s; return "\"" + s.Replace("\"", "\"\"") + "\""; };   // chống CSV/formula injection
                foreach (var r in rows) sb.AppendLine(string.Join(",", q(r.Code), q(r.CustomerName), q(r.CustomerEmail), q(r.HotelName), q(r.RoomName), r.CheckIn.ToString("yyyy-MM-dd"), r.CheckOut.ToString("yyyy-MM-dd"), r.Total.ToString("0"), q(r.Status), r.CreatedAt.ToString("yyyy-MM-dd HH:mm")));
                Audit("EXPORT_BOOKINGS", "DonDatPhong", null, rows.Count + " dòng");
                return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "bookings-" + DateTime.Now.ToString("yyyyMMdd-HHmm") + ".csv");
            }
        }

        public ActionResult Details(int id)
        {
            var d = BookingService.Get(id, null, true);
            if (d == null) return HttpNotFound();
            ViewBag.Title = "Đặt phòng " + d.Code; ViewBag.ReceiptNo = ReceiptService.LatestForBooking(id);
            return View(d);
        }

        [HttpPost]
        public ActionResult Cancel(int id, string reason, bool fullRefund = false)
        {
            if (string.IsNullOrWhiteSpace(reason)) { Err("Cần nhập lý do hủy."); return RedirectToAction("Details", new { id }); }
            try { var r = BookingService.Cancel(id, null, true, Me.AccountId, "[Nhân viên] " + reason, fullRefund && Me.Role == AppRoles.Admin); Ok(r.Message); }
            catch (BookingException ex) { Err(ex.Message); }
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        public ActionResult Transition(int id, string to)
        {
            string target = to == "checkin" ? BookingStatus.CheckedIn : to == "complete" ? BookingStatus.Completed : null;
            if (target == null) return new HttpStatusCodeResult(400);
            using (var c = TravelDb.Open())
            using (var tx = c.BeginTransaction())
            {
                var cur = c.ExecuteScalar<string>("SELECT TrangThai FROM DonDatPhong WITH (UPDLOCK) WHERE MaDon=@id", new { id }, tx);
                if (cur == null) return HttpNotFound();
                if (!BookingStatus.CanTransition(cur, target)) { Err("Không thể chuyển từ “" + cur + "” sang “" + target + "”."); return RedirectToAction("Details", new { id }); }
                c.Execute("UPDATE DonDatPhong SET TrangThai=@t, TrangThaiCode=@code, NgayCapNhat=GETDATE() WHERE MaDon=@id", new { t = target, code = BookingStatus.Code(target), id }, tx);
                BookingService.AddHistory(c, tx, id, cur, target, Me.AccountId, "Cập nhật bởi " + Me.UserName);
                tx.Commit();
            }
            Audit("BOOKING_STATUS", "DonDatPhong", id.ToString(), target);
            Ok("Đã cập nhật trạng thái: " + target);
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        public ActionResult Note(int id, string note)
        {
            using (var c = TravelDb.Open()) c.Execute("UPDATE DonDatPhong SET GhiChuNoiBo=@n WHERE MaDon=@id", new { n = note, id });
            Audit("BOOKING_NOTE", "DonDatPhong", id.ToString(), note);
            Ok("Đã lưu ghi chú nội bộ.");
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        public ActionResult ResendEmail(int id)
        {
            var d = BookingService.Get(id, null, true);
            if (d == null) return HttpNotFound();
            if (d.StatusCode != "Paid" && d.StatusCode != "CheckedIn" && d.StatusCode != "Completed") { Err("Chỉ gửi lại cho đơn đã thanh toán."); return RedirectToAction("Details", new { id }); }
            EmailService.ResetForRetry("BOOKING_CONFIRMED:" + id);
            EmailService.ResetForRetry("PAYMENT_RECEIPT:" + (d.Payments.FirstOrDefault(p => p.TrangThai == PaymentStatus.Succeeded)?.MaThanhToan ?? id));
            Audit("EMAIL_RESEND", "DonDatPhong", id.ToString(), null);
            Ok("Đã đưa email xác nhận vào hàng đợi gửi lại.");
            return RedirectToAction("Details", new { id });
        }

        public ActionResult Receipt(string no)
        {
            int bid; var s = ReceiptService.Load(no ?? "", out bid);
            if (s == null) return HttpNotFound();
            return File(ReceiptPdf.Render(s), "application/pdf", "TravelHub-" + s.ReceiptNo + ".pdf");
        }
    }

    [RoleAuthorize(Roles = "Admin,Staff")]
    public class PaymentsController : AdminController
    {
        public ActionResult Index(string q, string gateway, string status, int page = 1)
        {
            if (page < 1) page = 1; const int size = 25;
            using (var c = TravelDb.Open())
            {
                var p = new DynamicParameters(new { q = Like(q), gw = string.IsNullOrEmpty(gateway) ? null : gateway, st = string.IsNullOrEmpty(status) ? null : status, skip = (page - 1) * size, take = size });
                const string from = @"FROM ThanhToan t JOIN DonDatPhong o ON o.MaDon=t.MaDon WHERE (@q IS NULL OR o.MaDonCode LIKE @q OR t.MaThamChieuVNPAY LIKE @q OR t.MaGiaoDichCong LIKE @q) AND (@gw IS NULL OR t.Cong=@gw OR t.PhuongThuc=@gw) AND (@st IS NULL OR t.TrangThai=@st)";
                ViewBag.Total = c.ExecuteScalar<int>("SELECT COUNT(*) " + from, p);
                var rows = c.Query(@"SELECT t.MaThanhToan, o.MaDon, o.MaDonCode, t.SoTien, t.PhuongThuc, t.MaThamChieuVNPAY, t.MaGiaoDichCong, t.TrangThai, t.TrangThaiCode, t.LoaiGiaoDich, t.NgayThanhToan, t.NgayTao " + from + " ORDER BY t.MaThanhToan DESC OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY", p).ToList();
                ViewBag.Page = page; ViewBag.Size = size; ViewBag.Q = q; ViewBag.Gateway = gateway; ViewBag.Status = status; ViewBag.Title = "Thanh toán";
                return View(rows);
            }
        }

        public ActionResult Callbacks(int id)
        {
            using (var c = TravelDb.Open())
            {
                ViewBag.Payment = c.QueryFirstOrDefault("SELECT t.*, o.MaDonCode FROM ThanhToan t JOIN DonDatPhong o ON o.MaDon=t.MaDon WHERE t.MaThanhToan=@id", new { id });
                if (ViewBag.Payment == null) return HttpNotFound();
                ViewBag.Title = "Nhật ký giao dịch #" + id;
                return View(c.Query("SELECT * FROM NhatKyThanhToan WHERE MaThanhToan=@id ORDER BY MaNhatKy DESC", new { id }).ToList());
            }
        }
    }

    [RoleAuthorize(Roles = "Admin,Staff")]
    public class RefundsController : AdminController
    {
        public ActionResult Index(string status)
        {
            using (var c = TravelDb.Open())
            {
                var rows = c.Query<RefundRow>(@"SELECT r.MaYeuCau, r.MaDon, o.MaDonCode, ISNULL(o.HoTenKhach, kh.HoTen) TenKhach, r.SoTien, r.LyDo, r.TrangThai, r.MaGiaoDichHoan, r.LyDoTuChoi, CAST(r.NgayTao AS datetime) NgayTao, CAST(r.NgayXuLy AS datetime) NgayXuLy
                    FROM YeuCauHoanTien r JOIN DonDatPhong o ON o.MaDon=r.MaDon JOIN KhachHang kh ON kh.MaKH=o.MaKH WHERE (@s IS NULL OR r.TrangThai=@s) ORDER BY r.MaYeuCau DESC", new { s = string.IsNullOrEmpty(status) ? null : status }).ToList();
                ViewBag.Status = status; ViewBag.Title = "Yêu cầu hoàn tiền"; ViewBag.Reconcile = c.Query("SELECT t.MaThanhToan, o.MaDonCode, t.SoTien, t.TrangThaiCode, t.MaGiaoDichCong FROM ThanhToan t JOIN DonDatPhong o ON o.MaDon=t.MaDon WHERE t.TrangThai=N'Cần đối soát' AND t.TrangThaiCode<>'SUPERSEDED'").ToList();
                return View(rows);
            }
        }

        /// <summary>Duyệt: chưa phải "đã hoàn". Cổng chưa hỗ trợ API hoàn tự động → MANUAL_REQUIRED (chờ nhập mã giao dịch hoàn thực tế).</summary>
        [HttpPost]
        public ActionResult Approve(int id)
        {
            using (var c = TravelDb.Open())
            {
                var n = c.Execute("UPDATE YeuCauHoanTien SET TrangThai='MANUAL_REQUIRED', NguoiXuLy=@u, NgayXuLy=SYSUTCDATETIME() WHERE MaYeuCau=@id AND TrangThai='PENDING'", new { u = Me.AccountId, id });
                if (n == 0) Err("Yêu cầu không còn ở trạng thái chờ duyệt."); else { Audit("REFUND_APPROVE", "YeuCauHoanTien", id.ToString(), null); Ok("Đã duyệt. Hãy hoàn tiền qua cổng/ngân hàng rồi xác nhận kèm mã giao dịch hoàn."); }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Reject(int id, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) { Err("Cần nhập lý do từ chối."); return RedirectToAction("Index"); }
            using (var c = TravelDb.Open())
            {
                var row = c.QueryFirstOrDefault("SELECT MaDon FROM YeuCauHoanTien WHERE MaYeuCau=@id AND TrangThai IN ('PENDING','MANUAL_REQUIRED')", new { id });
                if (row == null) { Err("Yêu cầu không thể từ chối ở trạng thái hiện tại."); return RedirectToAction("Index"); }
                c.Execute("UPDATE YeuCauHoanTien SET TrangThai='REJECTED', LyDoTuChoi=@r, NguoiXuLy=@u, NgayXuLy=SYSUTCDATETIME() WHERE MaYeuCau=@id", new { r = reason, u = Me.AccountId, id });
                Audit("REFUND_REJECT", "YeuCauHoanTien", id.ToString(), reason);
                var d = BookingService.Get((int)row.MaDon, null, true); if (d != null) BookingNotifier.RefundRejected(d, reason);
            }
            Ok("Đã từ chối yêu cầu hoàn tiền.");
            return RedirectToAction("Index");
        }

        /// <summary>Chỉ đánh dấu REFUNDED khi nhập mã giao dịch hoàn thực tế (xác nhận từ cổng/ngân hàng) — có ghi nhật ký người thực hiện.</summary>
        [HttpPost]
        public ActionResult Complete(int id, string reference)
        {
            if (string.IsNullOrWhiteSpace(reference) || reference.Trim().Length < 4) { Err("Cần nhập mã giao dịch hoàn tiền thực tế từ cổng/ngân hàng."); return RedirectToAction("Index"); }
            using (var c = TravelDb.Open())
            {
                var row = c.QueryFirstOrDefault("SELECT MaDon, SoTien FROM YeuCauHoanTien WHERE MaYeuCau=@id AND TrangThai='MANUAL_REQUIRED'", new { id });
                if (row == null) { Err("Chỉ xác nhận được yêu cầu đã duyệt và đang chờ hoàn thủ công."); return RedirectToAction("Index"); }
                var n = c.Execute("UPDATE YeuCauHoanTien SET TrangThai='REFUNDED', MaGiaoDichHoan=@r, NguoiXuLy=@u, NgayXuLy=SYSUTCDATETIME() WHERE MaYeuCau=@id AND TrangThai='MANUAL_REQUIRED'", new { r = reference.Trim(), u = Me.AccountId, id });
                if (n == 1)
                {
                    Audit("REFUND_COMPLETE", "YeuCauHoanTien", id.ToString(), "ref=" + reference.Trim());
                    var d = BookingService.Get((int)row.MaDon, null, true); if (d != null) BookingNotifier.RefundCompleted(d, (decimal)row.SoTien, reference.Trim());
                }
            }
            Ok("Đã ghi nhận hoàn tiền hoàn tất.");
            return RedirectToAction("Index");
        }
    }
}
