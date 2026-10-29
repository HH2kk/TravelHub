using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Dapper;
using TravelHub.Data;
using TravelHub.Infrastructure;
using TravelHub.Models;
using TravelHub.Services;
using TravelHub.Services.Payment;

namespace TravelHub.Controllers
{
    [RoleAuthorize]
    public class BookingController : AppController
    {
        // ------------------------------------------------------------ Bước 2–6: chọn phòng, nhập thông tin, xem giá
        [HttpGet, RequireVerifiedEmail]
        public ActionResult Checkout(BookingRequest r)
        {
            ModelState.Clear();
            if (r.Adults < 1) r.Adults = 2; if (r.Rooms < 1) r.Rooms = 1;
            var vm = Build(r, true);
            if (vm == null) return HttpNotFound();
            return View(vm);
        }

        // Cập nhật giá (mã giảm giá, số phòng…) mà không tạo đơn
        [HttpPost, RequireVerifiedEmail]
        public ActionResult Quote([Bind(Prefix = "Request")] BookingRequest r)
        {
            ModelState.Clear();
            var vm = Build(r, false);
            if (vm == null) return HttpNotFound();
            return View("Checkout", vm);
        }

        CheckoutViewModel Build(BookingRequest r, bool prefill)
        {
            var hotel = CatalogService.GetHotel(r.HotelId);
            var room = CatalogService.GetRoom(r.RoomId, r.CheckIn, r.CheckOut);
            if (hotel == null || room == null || room.HotelId != hotel.Id) return null;
            if (prefill)
            {
                var p = new AccountService().GetProfile(Me.AccountId);
                if (string.IsNullOrEmpty(r.GuestName)) r.GuestName = p.FullName;
                if (string.IsNullOrEmpty(r.GuestEmail)) r.GuestEmail = p.Email;
                if (string.IsNullOrEmpty(r.GuestPhone)) r.GuestPhone = p.Phone;
            }
            var vm = new CheckoutViewModel { Request = r, Hotel = hotel, Room = room, CancelPolicyCode = hotel.CancelPolicyCode, CancelPolicyText = hotel.CancelPolicyText,
                CheckInTime = hotel.CheckInTime, CheckOutTime = hotel.CheckOutTime, EmailConfirmed = true };
            try
            {
                BookingService.ValidateStay(r.CheckIn, r.CheckOut);
                using (var c = TravelDb.Open())
                {
                    BookingService.CouponInfo cp; string msg;
                    vm.Quote = BookingService.Quote(c, null, r.RoomId, r.CheckIn, r.CheckOut, Math.Max(1, r.Rooms), r.CouponCode, Me.CustomerId, out cp, out msg);
                    vm.CouponMessage = msg;
                    var avail = c.ExecuteScalar<int>("SELECT dbo.fn_PhongConLai(@r,@a,@b)", new { r = r.RoomId, a = r.CheckIn.Date, b = r.CheckOut.Date });
                    if (avail < r.Rooms) ViewBag.AvailError = avail == 0 ? "Loại phòng này đã hết trong khoảng ngày bạn chọn." : "Chỉ còn " + avail + " phòng trong khoảng ngày bạn chọn.";
                }
            }
            catch (BookingException ex) { ViewBag.StayError = ex.Message; }
            ViewBag.Title = "Đặt phòng – " + hotel.Name;
            return vm;
        }

        // ------------------------------------------------------------ Bước 7: tạo đơn giữ chỗ
        [HttpPost, RequireVerifiedEmail, RateLimit(10, 60)]
        public ActionResult Create([Bind(Prefix = "Request")] BookingRequest r)
        {
            if (!ModelState.IsValid)
            {
                var vm = Build(r, false);
                return vm == null ? (ActionResult)HttpNotFound() : View("Checkout", vm);
            }
            try
            {
                var b = BookingService.Create(Me.CustomerId.Value, r);
                AppLog.Audit(Me.AccountId, "BOOKING_CREATE", "DonDatPhong", b.Id.ToString(), b.Code);
                return RedirectToAction("Details", new { id = b.Id });
            }
            catch (BookingException ex)
            {
                ModelState.AddModelError("", ex.Message);
                var vm = Build(r, false);
                return vm == null ? (ActionResult)HttpNotFound() : View("Checkout", vm);
            }
        }

        // ------------------------------------------------------------ Bước 8: chuyển sang cổng thanh toán
        [HttpPost, RequireVerifiedEmail, RateLimit(10, 60)]
        public ActionResult Pay(int id, string gateway)
        {
            try
            {
                var returnUrl = AppConfig.BaseUrl + Url.Action("VnPayReturn", "Payment");
                var url = PaymentService.StartPayment(id, Me.CustomerId.Value, gateway ?? "VNPAY", (Request.UserHostAddress == "::1" ? "127.0.0.1" : Request.UserHostAddress), returnUrl);
                return Redirect(url);
            }
            catch (BookingException ex) { Err(ex.Message); return RedirectToAction("Details", new { id }); }
            catch (Exception ex) { AppLog.Error("Pay", ex); Err("Không thể khởi tạo thanh toán lúc này."); return RedirectToAction("Details", new { id }); }
        }

        // ------------------------------------------------------------ Chi tiết / danh sách (chống IDOR: luôn lọc theo chủ đơn)
        public ActionResult Details(int id)
        {
            var d = BookingService.Get(id, Me.CustomerId ?? 0, false);
            if (d == null) return HttpNotFound();
            ViewBag.Title = "Đặt phòng " + d.Code;
            ViewBag.Gateways = Gateways.All.ToList();
            ViewBag.ReceiptNo = ReceiptService.LatestForBooking(id);
            if (d.PaidAmount > 0 && d.StatusCode == "Paid")
            {
                decimal first = d.Quote != null && d.Quote.Nights.Count > 0 ? Math.Round(d.Quote.Nights[0].Price * d.Quote.Quantity * (1 + d.Quote.TaxRate) * (1 + d.Quote.ServiceFeeRate), 0) : 0;
                ViewBag.RefundPreview = CancellationPolicy.Evaluate(d.CancelPolicyCode, d.CheckIn, d.CheckInTime, DateTime.Now, d.PaidAmount, first);
            }
            return View(d);
        }

        public ActionResult My()
        {
            ViewBag.Title = "Đặt phòng của tôi";
            return View(BookingService.ListForCustomer(Me.CustomerId ?? 0));
        }

        [HttpPost, RateLimit(10, 60)]
        public ActionResult Cancel(int id, string reason)
        {
            try
            {
                var res = BookingService.Cancel(id, Me.CustomerId ?? 0, false, Me.AccountId, reason);
                Ok(res.Message);
            }
            catch (BookingException ex) { Err(ex.Message); }
            return RedirectToAction("Details", new { id });
        }

        [HttpPost, RateLimit(5, 300)]
        public ActionResult ResendEmail(int id)
        {
            var d = BookingService.Get(id, Me.CustomerId ?? 0, false);
            if (d == null) return HttpNotFound();
            if (d.StatusCode != "Paid" && d.StatusCode != "CheckedIn" && d.StatusCode != "Completed") { Err("Chỉ gửi lại email cho đặt phòng đã thanh toán."); return RedirectToAction("Details", new { id }); }
            EmailService.ResetForRetry("BOOKING_CONFIRMED:" + id);
            var key = "BOOKING_CONFIRMED:" + id;
            // Nếu email cũ chưa từng được tạo (hiếm), tạo lại.
            using (var c = TravelDb.Open())
                if (c.ExecuteScalar<int>("SELECT COUNT(*) FROM EmailOutbox WHERE KhoaTrung=@k", new { k = key }) == 0) BookingNotifier.PaymentConfirmed(id, d.Payments.FirstOrDefault(p => p.TrangThai == PaymentStatus.Succeeded)?.MaThanhToan);
            Ok("Đã đưa email xác nhận vào hàng đợi gửi lại.");
            return RedirectToAction("Details", new { id });
        }

        // ------------------------------------------------------------ Biên nhận PDF (chỉ chủ đơn hoặc nhân viên)
        public ActionResult Receipt(string no)
        {
            int bookingId; var snap = ReceiptService.Load(no ?? "", out bookingId);
            if (snap == null) return HttpNotFound();
            bool staff = IsStaff;
            var d = BookingService.Get(bookingId, Me.CustomerId ?? 0, staff);
            if (d == null) return HttpNotFound();     // không phải chủ đơn → 404, không lộ sự tồn tại
            var pdf = ReceiptPdf.Render(snap);
            return File(pdf, "application/pdf", "TravelHub-" + snap.ReceiptNo + ".pdf");
        }

        // ------------------------------------------------------------ Đánh giá
        [HttpPost, RateLimit(5, 300)]
        public ActionResult Review(int id, int rating, string content)
        {
            var d = BookingService.Get(id, Me.CustomerId ?? 0, false);
            if (d == null) return HttpNotFound();
            if (d.StatusCode != "Completed" && !(d.StatusCode == "Paid" && d.CheckOut.Date <= DateTime.Today)) { Err("Chỉ đánh giá sau khi đã hoàn thành kỳ lưu trú."); return RedirectToAction("Details", new { id }); }
            if (rating < 1 || rating > 5) { Err("Điểm đánh giá từ 1 đến 5 sao."); return RedirectToAction("Details", new { id }); }
            if (content != null && content.Length > 1000) content = content.Substring(0, 1000);
            try
            {
                using (var c = TravelDb.Open())
                    c.Execute("INSERT INTO DanhGia (MaKH, MaKS, MaDon, SoSao, NoiDung, TrangThai) VALUES (@k,@h,@d,@s,@n,1)", new { k = Me.CustomerId, h = d.HotelId, d = id, s = rating, n = content });
                Ok("Cảm ơn bạn đã đánh giá!");
            }
            catch (System.Data.SqlClient.SqlException ex) when (ex.Number == 2601 || ex.Number == 2627) { Err("Bạn đã đánh giá kỳ lưu trú này rồi."); }
            return RedirectToAction("Details", new { id });
        }
    }

    public class PaymentController : AppController
    {
        // URL trả về sau khi thanh toán: chữ ký được xác minh phía server; trạng thái hiển thị lấy từ DB, không từ trình duyệt.
        [HttpGet]
        public ActionResult VnPayReturn()
        {
            var gw = Gateways.Get("VNPAY");
            var o = PaymentService.HandleCallback(gw, Request.QueryString, "RETURN");
            ViewBag.Title = "Kết quả thanh toán";
            ViewBag.SignatureValid = o.SignatureValid;
            if (o.BookingId.HasValue && Me != null && Me.CustomerId.HasValue)
            {
                var d = BookingService.Get(o.BookingId.Value, Me.CustomerId.Value, false);
                if (d != null) return View("Result", d);
            }
            return View("Result", (BookingDetail)null);
        }

        // IPN server-to-server từ VNPay. Không cần đăng nhập/antiforgery; bảo vệ bằng chữ ký HMAC.
        [HttpGet, IgnoreAntiForgery]
        public ContentResult VnPayIpn()
        {
            var o = PaymentService.HandleCallback(Gateways.Get("VNPAY"), Request.QueryString, "IPN");
            return Content("{\"RspCode\":\"" + o.RspCode + "\",\"Message\":\"" + (o.Message ?? "") + "\"}", "application/json");
        }
    }
}
