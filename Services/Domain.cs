using System;
using System.Collections.Generic;
using System.Linq;

namespace TravelHub.Services
{
    /// <summary>Trạng thái đơn đặt phòng. Giá trị text khớp dữ liệu hiện có trong DonDatPhong.TrangThai.</summary>
    public static class BookingStatus
    {
        public const string Pending = "Chờ thanh toán";
        public const string LegacyPending = "Chờ xác nhận";
        public const string Paid = "Đã thanh toán";      // đã xác nhận + đã thu tiền
        public const string CheckedIn = "Đã nhận phòng";
        public const string Completed = "Hoàn thành";
        public const string Cancelled = "Đã hủy";
        public const string Expired = "Hết hạn";

        public static readonly string[] InactiveStatuses = { Cancelled, "Hủy", Expired, "Cancelled", "Canceled" };

        public static string Code(string text)
        {
            switch (text)
            {
                case Pending: case LegacyPending: return "Pending";
                case Paid: return "Paid";
                case CheckedIn: return "CheckedIn";
                case Completed: return "Completed";
                case Cancelled: case "Hủy": return "Cancelled";
                case Expired: return "Expired";
                default: return "Pending";
            }
        }

        static readonly Dictionary<string, string[]> Transitions = new Dictionary<string, string[]>
        {
            { Pending,       new[] { Paid, Cancelled, Expired } },
            { LegacyPending, new[] { Paid, Cancelled, Expired } },
            { Paid,          new[] { CheckedIn, Cancelled, Completed } },
            { CheckedIn,     new[] { Completed } },
            { Expired,       new[] { Paid } },   // chỉ hợp lệ khi thanh toán đến muộn và vẫn còn phòng
        };

        public static bool CanTransition(string from, string to)
        {
            string[] next;
            return from != null && Transitions.TryGetValue(from, out next) && next.Contains(to);
        }
    }

    public static class PaymentStatus
    {
        public const string Processing = "Đang xử lý";
        public const string Succeeded = "Thành công";
        public const string Failed = "Thất bại";
        public const string Reconcile = "Cần đối soát";
    }

    public static class AppRoles
    {
        public const string Customer = "Traveler";
        public const string Partner = "Partner";
        public const string Admin = "Admin";
        public const string Staff = "Staff";
        public const string ContentManager = "ContentManager";
        public const string AdminArea = "Admin,Staff,ContentManager";
    }

    public class NightPrice
    {
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
    }

    /// <summary>Bảng giá chi tiết. Mọi số tiền tính ở server bằng decimal.</summary>
    public class PriceQuote
    {
        public int Quantity { get; set; }
        public List<NightPrice> Nights { get; set; } = new List<NightPrice>();
        public decimal Subtotal { get; set; }      // tổng tiền phòng trước giảm giá/thuế
        public decimal Discount { get; set; }
        public string CouponCode { get; set; }
        public decimal TaxRate { get; set; }
        public decimal Tax { get; set; }
        public decimal ServiceFeeRate { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal Total { get; set; }
        public int NightCount { get { return Nights.Count; } }
        public decimal AverageNight { get { return Nights.Count == 0 ? 0 : Math.Round(Nights.Average(n => n.Price), 0); } }
    }

    public static class PricingCalculator
    {
        /// <summary>Tính giá từ giá từng đêm. Làm tròn về VND nguyên.</summary>
        public static PriceQuote Calculate(IList<NightPrice> nights, int quantity, decimal discount, decimal taxRate, decimal serviceFeeRate)
        {
            if (nights == null || nights.Count == 0) throw new ArgumentException("Không có đêm nào để tính giá.");
            if (quantity < 1) throw new ArgumentException("Số phòng phải ≥ 1.");
            var q = new PriceQuote { Quantity = quantity, TaxRate = taxRate, ServiceFeeRate = serviceFeeRate };
            q.Nights.AddRange(nights);
            q.Subtotal = nights.Sum(n => n.Price) * quantity;
            q.Discount = Math.Min(Math.Max(0, discount), q.Subtotal);
            var taxable = q.Subtotal - q.Discount;
            q.ServiceFee = Math.Round(taxable * serviceFeeRate, 0, MidpointRounding.AwayFromZero);
            q.Tax = Math.Round((taxable + q.ServiceFee) * taxRate, 0, MidpointRounding.AwayFromZero);
            q.Total = taxable + q.ServiceFee + q.Tax;
            return q;
        }

        /// <summary>Số tiền giảm của mã khuyến mãi (đã áp trần và giá trị tối thiểu).</summary>
        public static decimal CouponDiscount(decimal subtotal, decimal? percent, decimal? amount, decimal? minOrder, decimal? maxDiscount)
        {
            if (minOrder.HasValue && subtotal < minOrder.Value) return 0;
            decimal d = 0;
            if (percent.HasValue && percent.Value > 0) d = Math.Round(subtotal * percent.Value / 100m, 0, MidpointRounding.AwayFromZero);
            else if (amount.HasValue) d = amount.Value;
            if (maxDiscount.HasValue && maxDiscount.Value > 0) d = Math.Min(d, maxDiscount.Value);
            return Math.Min(d, subtotal);
        }
    }

    public class RefundDecision
    {
        public decimal Amount { get; set; }
        public bool Eligible { get { return Amount > 0; } }
        public string Description { get; set; }
        public bool FreeCancellation { get; set; }
    }

    /// <summary>Chính sách hủy: FREE24 = hoàn toàn bộ nếu hủy trước 24h so với giờ nhận phòng;
    /// FLEX1 = như FREE24 nhưng hủy muộn bị giữ lại 1 đêm đầu; NONREFUND = không hoàn.</summary>
    public static class CancellationPolicy
    {
        public static string Describe(string code)
        {
            switch (code)
            {
                case "FREE24": return "Miễn phí hủy trước 24 giờ so với giờ nhận phòng; hủy muộn hơn không được hoàn tiền.";
                case "NONREFUND": return "Không hoàn tiền khi hủy hoặc thay đổi.";
                default: return "Miễn phí hủy trước 24 giờ so với giờ nhận phòng; hủy muộn hơn bị giữ lại giá trị 1 đêm đầu tiên.";
            }
        }

        public static string ShortLabel(string code)
        {
            return code == "NONREFUND" ? "Không hoàn hủy" : "Miễn phí hủy";
        }

        public static DateTime FreeCancelDeadline(DateTime checkInDate, string checkInTime)
        {
            TimeSpan t;
            if (!TimeSpan.TryParse(checkInTime ?? "14:00", out t)) t = new TimeSpan(14, 0, 0);
            return checkInDate.Date.Add(t).AddHours(-24);
        }

        /// <param name="paidAmount">Số tiền thực thu cho đơn.</param>
        /// <param name="firstNightWithTax">Giá trị 1 đêm đầu (đã gồm thuế/phí) — tối đa bị giữ lại với FLEX1.</param>
        public static RefundDecision Evaluate(string code, DateTime checkInDate, string checkInTime, DateTime nowLocal, decimal paidAmount, decimal firstNightWithTax)
        {
            var r = new RefundDecision();
            if (paidAmount <= 0) { r.Description = "Đơn chưa có khoản thanh toán nào."; return r; }
            var deadline = FreeCancelDeadline(checkInDate, checkInTime);
            bool early = nowLocal <= deadline;
            if (code == "NONREFUND")
            {
                r.Description = "Chính sách không hoàn tiền.";
            }
            else if (early)
            {
                r.Amount = paidAmount; r.FreeCancellation = true;
                r.Description = "Hủy trước hạn miễn phí — hoàn 100%.";
            }
            else if (code == "FREE24")
            {
                r.Description = "Đã quá hạn hủy miễn phí — không được hoàn tiền.";
            }
            else
            {
                r.Amount = Math.Max(0, paidAmount - Math.Min(paidAmount, firstNightWithTax));
                r.Description = "Đã quá hạn hủy miễn phí — giữ lại giá trị 1 đêm đầu tiên.";
            }
            return r;
        }
    }

    /// <summary>Tính tồn phòng theo từng đêm (dùng cho unit test; SQL dùng dbo.fn_PhongConLai).</summary>
    public static class InventoryMath
    {
        public class Hold { public DateTime CheckIn; public DateTime CheckOut; public int Qty; }

        /// <summary>Số phòng bị chiếm nhiều nhất trong một đêm bất kỳ của khoảng [from, to).</summary>
        public static int PeakUsed(IEnumerable<Hold> holds, DateTime from, DateTime to)
        {
            var list = holds.ToList(); int peak = 0;
            for (var d = from.Date; d < to.Date; d = d.AddDays(1))
                peak = Math.Max(peak, list.Where(h => h.CheckIn.Date <= d && h.CheckOut.Date > d).Sum(h => h.Qty));
            return peak;
        }

        public static int MinAvailable(int totalRooms, IEnumerable<Hold> holds, DateTime checkIn, DateTime checkOut)
        {
            int min = int.MaxValue;
            var list = holds.ToList();
            for (var d = checkIn.Date; d < checkOut.Date; d = d.AddDays(1))
            {
                int used = list.Where(h => h.CheckIn.Date <= d && h.CheckOut.Date > d).Sum(h => h.Qty);
                min = Math.Min(min, totalRooms - used);
            }
            return min == int.MaxValue ? 0 : Math.Max(0, min);
        }
    }
}
