using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Dapper;
using Newtonsoft.Json;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using TravelHub.Data;
using TravelHub.Models;

namespace TravelHub.Services
{
    /// <summary>Dữ liệu bất biến của biên nhận: lưu JSON tại thời điểm thanh toán nên không đổi khi giá/khách sạn thay đổi sau này.</summary>
    public class ReceiptSnapshot
    {
        public string ReceiptNo { get; set; }
        public DateTime IssuedAt { get; set; }
        public string BookingCode { get; set; }
        public string PaymentRef { get; set; }
        public string GatewayTxnNo { get; set; }
        public string Method { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime? PaidAt { get; set; }
        public string GuestName { get; set; }
        public string GuestEmail { get; set; }
        public string GuestPhone { get; set; }
        public string HotelName { get; set; }
        public string HotelAddress { get; set; }
        public string RoomName { get; set; }
        public int Rooms { get; set; }
        public int Adults { get; set; }
        public int Children { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int Nights { get; set; }
        public PriceQuote Quote { get; set; }
        public decimal Total { get; set; }
        public decimal Paid { get; set; }
        public string CancelPolicy { get; set; }
    }

    public static class ReceiptService
    {
        public static string Create(int bookingId, int? paymentId)
        {
            using (var c = TravelDb.Open())
            {
                if (paymentId.HasValue)
                {
                    var existing = c.ExecuteScalar<string>("SELECT SoBienNhan FROM BienNhan WHERE MaThanhToan=@p", new { p = paymentId });
                    if (existing != null) return existing;
                }
                var d = BookingService.Get(bookingId, null, true);
                if (d == null) throw new InvalidOperationException("Không tìm thấy đơn.");
                var pay = d.Payments.FirstOrDefault(p => p.MaThanhToan == paymentId) ?? d.Payments.FirstOrDefault(p => p.TrangThai == PaymentStatus.Succeeded);
                var snap = new ReceiptSnapshot
                {
                    IssuedAt = DateTime.Now, BookingCode = d.Code, PaymentRef = pay != null ? pay.MaThamChieuVNPAY : null, GatewayTxnNo = null,
                    Method = pay != null ? pay.PhuongThuc : null, PaymentStatus = pay != null ? pay.TrangThai : "—",
                    PaidAt = pay != null ? pay.NgayThanhToan : null,
                    GuestName = d.GuestName, GuestEmail = d.GuestEmail, GuestPhone = d.GuestPhone,
                    HotelName = d.HotelName, HotelAddress = d.HotelAddress, RoomName = d.RoomName, Rooms = d.Quantity,
                    Adults = d.Adults, Children = d.Children, CheckIn = d.CheckIn, CheckOut = d.CheckOut, Nights = d.Nights,
                    Quote = d.Quote, Total = d.Total, Paid = pay != null && pay.TrangThai == PaymentStatus.Succeeded ? pay.SoTien : 0,
                    CancelPolicy = d.CancelPolicyText
                };
                if (pay != null) snap.GatewayTxnNo = c.ExecuteScalar<string>("SELECT MaGiaoDichCong FROM ThanhToan WHERE MaThanhToan=@p", new { p = pay.MaThanhToan });
                var tmp = "TMP" + Guid.NewGuid().ToString("N").Substring(0, 20);
                int id;
                try
                {
                    id = c.ExecuteScalar<int>(@"INSERT INTO BienNhan (SoBienNhan, MaDon, MaThanhToan, DuLieuSnapshot) VALUES (@s,@d,@p,@j); SELECT CAST(SCOPE_IDENTITY() AS int);",
                        new { s = tmp, d = bookingId, p = paymentId, j = JsonConvert.SerializeObject(snap) });
                }
                catch (System.Data.SqlClient.SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
                {
                    return c.ExecuteScalar<string>("SELECT SoBienNhan FROM BienNhan WHERE MaThanhToan=@p", new { p = paymentId });
                }
                var no = "BN" + DateTime.Now.ToString("yyMMdd") + "-" + id.ToString("D5");
                snap.ReceiptNo = no;
                c.Execute("UPDATE BienNhan SET SoBienNhan=@s, DuLieuSnapshot=@j WHERE MaBienNhan=@id", new { s = no, j = JsonConvert.SerializeObject(snap), id });
                return no;
            }
        }

        public static ReceiptSnapshot Load(string receiptNo, out int bookingId)
        {
            bookingId = 0;
            using (var c = TravelDb.Open())
            {
                var row = c.QueryFirstOrDefault("SELECT MaDon, DuLieuSnapshot FROM BienNhan WHERE SoBienNhan=@s", new { s = receiptNo });
                if (row == null) return null;
                bookingId = row.MaDon;
                var snap = JsonConvert.DeserializeObject<ReceiptSnapshot>((string)row.DuLieuSnapshot);
                snap.ReceiptNo = receiptNo;
                return snap;
            }
        }

        public static string LatestForBooking(int bookingId)
        {
            using (var c = TravelDb.Open())
                return c.ExecuteScalar<string>("SELECT TOP 1 SoBienNhan FROM BienNhan WHERE MaDon=@d ORDER BY MaBienNhan DESC", new { d = bookingId });
        }

        public static byte[] RenderPdfBySoBienNhan(string receiptNo)
        {
            int bid; var s = Load(receiptNo, out bid);
            return s == null ? null : ReceiptPdf.Render(s);
        }
    }

    public static class ReceiptPdf
    {
        static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");
        static string Money(decimal v) { return v.ToString("N0", Vi) + " ₫"; }

        static XFont F(double size, bool bold = false)
        {
            // Arial hỗ trợ đủ dấu tiếng Việt; nhúng Unicode để PDF hiển thị đúng trên mọi máy.
            return new XFont("Arial", size, bold ? XFontStyle.Bold : XFontStyle.Regular, new XPdfFontOptions(PdfFontEncoding.Unicode));
        }

        public static byte[] Render(ReceiptSnapshot s)
        {
            var doc = new PdfDocument();
            doc.Info.Title = "Biên nhận " + s.ReceiptNo;
            doc.Info.Author = "TravelHub";
            var page = doc.AddPage(); page.Size = PdfSharp.PageSize.A4;
            var g = XGraphics.FromPdfPage(page);
            double L = 40, R = page.Width - 40, y = 40;
            var navy = XColor.FromArgb(16, 42, 67); var blue = XColor.FromArgb(37, 99, 235); var gray = XColor.FromArgb(100, 116, 139);

            g.DrawRectangle(new XSolidBrush(navy), 0, 0, page.Width, 74);
            g.DrawString("TravelHub", F(24, true), XBrushes.White, L, 46);
            g.DrawString("BIÊN NHẬN THANH TOÁN", F(11, true), XBrushes.White, new XRect(L, 30, R - L, 20), XStringFormats.TopRight);
            g.DrawString(s.ReceiptNo ?? "", F(10), XBrushes.White, new XRect(L, 48, R - L, 20), XStringFormats.TopRight);
            y = 96;

            Action<string, string> kv = (k, v) =>
            {
                g.DrawString(k, F(9), new XSolidBrush(gray), L, y);
                g.DrawString(v ?? "—", F(10), XBrushes.Black, L + 130, y);
                y += 16;
            };
            Action<string> head = t =>
            {
                y += 6; g.DrawString(t, F(11, true), new XSolidBrush(blue), L, y); y += 4;
                g.DrawLine(new XPen(XColor.FromArgb(226, 232, 240), 1), L, y, R, y); y += 14;
            };

            head("Thông tin giao dịch");
            kv("Mã đặt phòng", s.BookingCode);
            kv("Mã giao dịch", s.PaymentRef);
            kv("Mã giao dịch của cổng", s.GatewayTxnNo);
            kv("Phương thức", s.Method);
            kv("Trạng thái", s.PaymentStatus);
            kv("Thời điểm thanh toán", s.PaidAt.HasValue ? s.PaidAt.Value.ToString("dd/MM/yyyy HH:mm", Vi) : "—");
            kv("Ngày lập biên nhận", s.IssuedAt.ToString("dd/MM/yyyy HH:mm", Vi));

            head("Người đặt");
            kv("Họ tên", s.GuestName); kv("Email", s.GuestEmail); kv("Điện thoại", s.GuestPhone);

            head("Chỗ nghỉ");
            kv("Tên", s.HotelName); kv("Địa chỉ", s.HotelAddress);
            kv("Loại phòng", s.RoomName + " × " + s.Rooms);
            kv("Nhận phòng", s.CheckIn.ToString("dd/MM/yyyy", Vi));
            kv("Trả phòng", s.CheckOut.ToString("dd/MM/yyyy", Vi) + "  (" + s.Nights + " đêm)");
            kv("Khách", s.Adults + " người lớn" + (s.Children > 0 ? ", " + s.Children + " trẻ em" : ""));

            head("Chi tiết giá");
            var q = s.Quote;
            if (q != null)
            {
                foreach (var n in q.Nights.Take(14))
                {
                    g.DrawString(n.Date.ToString("ddd dd/MM/yyyy", Vi), F(9.5), XBrushes.Black, L, y);
                    g.DrawString(Money(n.Price * q.Quantity), F(9.5), XBrushes.Black, new XRect(L, y - 10, R - L, 14), XStringFormats.TopRight);
                    y += 14;
                }
                if (q.Nights.Count > 14) { g.DrawString("… và " + (q.Nights.Count - 14) + " đêm khác", F(9), new XSolidBrush(gray), L, y); y += 14; }
                y += 4;
                Action<string, decimal, bool> line = (k, v, b) =>
                {
                    g.DrawString(k, F(10, b), XBrushes.Black, L, y);
                    g.DrawString(Money(v), F(10, b), XBrushes.Black, new XRect(L, y - 10, R - L, 14), XStringFormats.TopRight);
                    y += 16;
                };
                line("Tiền phòng", q.Subtotal, false);
                if (q.Discount > 0) line("Giảm giá" + (string.IsNullOrEmpty(q.CouponCode) ? "" : " (" + q.CouponCode + ")"), -q.Discount, false);
                if (q.ServiceFee > 0) line("Phụ phí dịch vụ", q.ServiceFee, false);
                if (q.Tax > 0) line("Thuế VAT (" + (q.TaxRate * 100).ToString("0.##", Vi) + "%)", q.Tax, false);
                g.DrawLine(new XPen(XColor.FromArgb(226, 232, 240), 1), L, y - 4, R, y - 4);
            }
            y += 4;
            g.DrawString("Tổng cộng", F(11, true), XBrushes.Black, L, y);
            g.DrawString(Money(s.Total), F(11, true), XBrushes.Black, new XRect(L, y - 10, R - L, 14), XStringFormats.TopRight); y += 18;
            g.DrawString("Đã thanh toán", F(11, true), new XSolidBrush(XColor.FromArgb(15, 157, 138)), L, y);
            g.DrawString(Money(s.Paid), F(11, true), new XSolidBrush(XColor.FromArgb(15, 157, 138)), new XRect(L, y - 10, R - L, 14), XStringFormats.TopRight); y += 18;
            g.DrawString("Còn phải trả", F(10), XBrushes.Black, L, y);
            g.DrawString(Money(Math.Max(0, s.Total - s.Paid)), F(10), XBrushes.Black, new XRect(L, y - 10, R - L, 14), XStringFormats.TopRight); y += 24;

            var tf = new PdfSharp.Drawing.Layout.XTextFormatter(g);
            tf.DrawString("Chính sách hủy: " + s.CancelPolicy, F(9), new XSolidBrush(gray), new XRect(L, y, R - L, 36), XStringFormats.TopLeft); y += 40;
            tf.DrawString("Đây là biên nhận ghi nhận khoản thanh toán trên hệ thống TravelHub, không phải hóa đơn giá trị gia tăng. " +
                          "Hỗ trợ: " + AppConfig.Setting("EmailHoTro", "support@travelhub.vn") + ".", F(8.5), new XSolidBrush(gray), new XRect(L, page.Height - 70, R - L, 40), XStringFormats.TopLeft);
            using (var ms = new MemoryStream()) { doc.Save(ms, false); return ms.ToArray(); }
        }
    }
}
