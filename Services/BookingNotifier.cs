using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using TravelHub.Models;

namespace TravelHub.Services
{
    public static class BookingNotifier
    {
        static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");
        static string Money(decimal v) { return v.ToString("N0", Vi) + "&nbsp;₫"; }
        static string E(string s) { return HttpUtility.HtmlEncode(s ?? ""); }

        public static string SummaryHtml(BookingDetail d)
        {
            var sb = new StringBuilder();
            sb.Append("<table role='presentation' width='100%' cellpadding='0' cellspacing='0' style='border-collapse:collapse;font-size:14px;color:#243247'>");
            Action<string, string> row = (k, v) => sb.AppendFormat("<tr><td style='padding:6px 0;color:#64748b;width:38%'>{0}</td><td style='padding:6px 0;font-weight:600'>{1}</td></tr>", k, v);
            row("Mã đặt phòng", E(d.Code));
            row("Chỗ nghỉ", E(d.HotelName));
            row("Loại phòng", E(d.RoomName) + " × " + d.Quantity);
            row("Nhận phòng", d.CheckIn.ToString("dd/MM/yyyy", Vi) + " (từ " + E(d.CheckInTime) + ")");
            row("Trả phòng", d.CheckOut.ToString("dd/MM/yyyy", Vi) + " (trước " + E(d.CheckOutTime) + ")");
            row("Số đêm / khách", d.Nights + " đêm · " + d.Adults + " người lớn" + (d.Children > 0 ? ", " + d.Children + " trẻ em" : ""));
            if (d.Quote != null)
            {
                row("Tiền phòng", Money(d.Quote.Subtotal));
                if (d.Quote.Discount > 0) row("Giảm giá", "−" + Money(d.Quote.Discount));
                if (d.Quote.ServiceFee > 0) row("Phụ phí dịch vụ", Money(d.Quote.ServiceFee));
                if (d.Quote.Tax > 0) row("Thuế VAT", Money(d.Quote.Tax));
            }
            row("Tổng thanh toán", "<span style='font-size:16px;color:#2563eb'>" + Money(d.Total) + "</span>");
            row("Trạng thái", E(d.Status));
            sb.Append("</table>");
            sb.Append("<p style='font-size:13px;color:#64748b;margin:14px 0 0'><strong>Chính sách hủy:</strong> " + E(d.CancelPolicyText) + "</p>");
            return sb.ToString();
        }

        static string ViewUrl(BookingDetail d) { return AppConfig.BaseUrl + "/Booking/Details/" + d.Id; }

        static Dictionary<string, string> Vals(BookingDetail d, string title, string intro)
        {
            return new Dictionary<string, string> {
                { "Title", title }, { "Name", d.GuestName }, { "Intro", intro }, { "SummaryHtml", SummaryHtml(d) }, { "ButtonUrl", ViewUrl(d) } };
        }

        public static void PaymentConfirmed(int bookingId, int? paymentId)
        {
            var d = BookingService.Get(bookingId, null, true);
            if (d == null) return;
            var receiptNo = ReceiptService.Create(bookingId, paymentId);
            EmailService.Enqueue("BOOKING_CONFIRMED", "BOOKING_CONFIRMED:" + bookingId, d.GuestEmail, "Xác nhận đặt phòng " + d.Code + " – TravelHub",
                EmailTemplates.Render("booking-confirmed", Vals(d, "Đặt phòng đã được xác nhận", "Cảm ơn bạn đã đặt phòng. Thanh toán đã được ghi nhận và đặt phòng của bạn đã được xác nhận.")));
            EmailService.Enqueue("PAYMENT_RECEIPT", "PAYMENT_RECEIPT:" + (paymentId ?? bookingId), d.GuestEmail, "Biên nhận thanh toán " + receiptNo + " – TravelHub",
                EmailTemplates.Render("payment-receipt", Vals(d, "Biên nhận thanh toán", "Chúng tôi đã nhận khoản thanh toán cho đặt phòng của bạn. Biên nhận PDF được đính kèm email này.")), receiptNo);
            var admin = AppConfig.Setting("EmailQuanTri", "");
            if (!string.IsNullOrWhiteSpace(admin))
                EmailService.Enqueue("ADMIN_NEW_BOOKING", "ADMIN_NEW_BOOKING:" + bookingId, admin, "[TravelHub] Đặt phòng mới " + d.Code,
                    EmailTemplates.Render("admin-new-booking", Vals(d, "Có đặt phòng mới", "Khách " + d.GuestName + " (" + d.GuestEmail + ", " + d.GuestPhone + ") vừa thanh toán thành công.")));
        }

        public static void PaymentFailed(int bookingId, string txnRef)
        {
            var d = BookingService.Get(bookingId, null, true);
            if (d == null) return;
            EmailService.Enqueue("PAYMENT_FAILED", "PAYMENT_FAILED:" + txnRef, d.GuestEmail, "Thanh toán chưa thành công – " + d.Code,
                EmailTemplates.Render("payment-failed", Vals(d, "Thanh toán chưa thành công",
                    d.IsHoldActive ? "Giao dịch chưa hoàn tất. Phòng vẫn được giữ cho bạn đến " + d.ExpiresAt.Value.ToString("HH:mm dd/MM", Vi) + ", bạn có thể thanh toán lại." : "Giao dịch chưa hoàn tất và thời gian giữ phòng đã kết thúc.")));
        }

        public static void Cancelled(BookingDetail d, decimal refund)
        {
            if (d == null) return;
            var intro = "Đặt phòng của bạn đã được hủy." + (refund > 0 ? " Yêu cầu hoàn tiền " + refund.ToString("N0", Vi) + " ₫ đã được ghi nhận và đang chờ xử lý." : "");
            EmailService.Enqueue("BOOKING_CANCELLED", "BOOKING_CANCELLED:" + d.Id, d.GuestEmail, "Đã hủy đặt phòng " + d.Code, EmailTemplates.Render("booking-cancelled", Vals(d, "Đặt phòng đã hủy", intro)));
        }

        public static void Expired(BookingDetail d)
        {
            EmailService.Enqueue("BOOKING_EXPIRED", "BOOKING_EXPIRED:" + d.Id, d.GuestEmail, "Đặt phòng " + d.Code + " đã hết hạn giữ chỗ",
                EmailTemplates.Render("booking-cancelled", Vals(d, "Hết thời gian giữ phòng", "Đơn đặt phòng chưa được thanh toán trong thời gian giữ chỗ nên đã tự động hết hạn.")));
        }

        public static void RefundCompleted(BookingDetail d, decimal amount, string reference)
        {
            EmailService.Enqueue("REFUND_DONE", "REFUND_DONE:" + d.Id, d.GuestEmail, "Hoàn tiền đặt phòng " + d.Code,
                EmailTemplates.Render("booking-cancelled", Vals(d, "Đã hoàn tiền", "TravelHub đã hoàn " + amount.ToString("N0", Vi) + " ₫ cho đặt phòng này" + (string.IsNullOrEmpty(reference) ? "." : " (mã tham chiếu: " + reference + ").") + " Thời gian tiền về tài khoản phụ thuộc ngân hàng.")));
        }

        public static void RefundRejected(BookingDetail d, string reason)
        {
            EmailService.Enqueue("REFUND_REJECTED", "REFUND_REJECTED:" + d.Id, d.GuestEmail, "Yêu cầu hoàn tiền " + d.Code + " không được chấp thuận",
                EmailTemplates.Render("booking-cancelled", Vals(d, "Yêu cầu hoàn tiền bị từ chối", "Rất tiếc, yêu cầu hoàn tiền không được chấp thuận. Lý do: " + reason)));
        }
    }
}
