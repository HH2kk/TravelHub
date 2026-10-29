using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using TravelHub.Services;
using TravelHub.Services.Payment;
using Xunit;

namespace TravelHub.Tests
{
    public class FakeUpload : System.Web.HttpPostedFileBase
    {
        readonly byte[] _b; readonly string _n;
        public FakeUpload(string name, byte[] bytes) { _n = name; _b = bytes; }
        public override string FileName { get { return _n; } }
        public override int ContentLength { get { return _b.Length; } }
        public override System.IO.Stream InputStream { get { return new System.IO.MemoryStream(_b); } }
    }

    public class UploadSecurityTests
    {
        static byte[] Pad(params byte[] head) { var b = new byte[64]; Array.Copy(head, b, head.Length); return b; }
        string Try(string name, byte[] bytes) { string err; var p = TravelHub.Controllers.UploadHelper.SaveImage(new FakeUpload(name, bytes), "~/x", "t", out err); Assert.Null(p); return err; }

        [Fact] public void Executable_extension_is_rejected() { Assert.NotNull(Try("shell.aspx", Pad(0xFF, 0xD8, 0xFF))); Assert.NotNull(Try("a.exe", Pad(0x4D, 0x5A))); Assert.NotNull(Try("a.jpg.cshtml", Pad(0xFF, 0xD8, 0xFF))); }
        [Fact] public void Image_extension_with_non_image_content_is_rejected() { Assert.Contains("không phải ảnh", Try("fake.jpg", Pad(0x4D, 0x5A, 0x90))); }
        [Fact] public void Oversized_file_is_rejected() { Assert.Contains("4 MB", Try("big.jpg", new byte[5 * 1024 * 1024])); }
        [Fact] public void Empty_file_is_rejected() { Assert.NotNull(Try("a.jpg", new byte[0])); }
    }

    public class PricingTests
    {
        static List<NightPrice> Nights(params decimal[] p) { var d = DateTime.Today.AddDays(30); return p.Select((x, i) => new NightPrice { Date = d.AddDays(i), Price = x }).ToList(); }

        [Fact]
        public void Total_is_sum_of_each_night_times_rooms_plus_tax()
        {
            var q = PricingCalculator.Calculate(Nights(1000000, 1200000), 2, 0, 0.08m, 0);
            Assert.Equal(4400000m, q.Subtotal);
            Assert.Equal(352000m, q.Tax);
            Assert.Equal(4752000m, q.Total);
        }

        [Fact]
        public void Discount_never_exceeds_subtotal_and_reduces_taxable_amount()
        {
            var q = PricingCalculator.Calculate(Nights(1000000), 1, 5000000, 0.1m, 0);
            Assert.Equal(1000000m, q.Discount);
            Assert.Equal(0m, q.Total);
        }

        [Fact] public void Coupon_percent_respects_cap_and_minimum()
        {
            Assert.Equal(200000m, PricingCalculator.CouponDiscount(2000000, 20, null, null, 200000));
            Assert.Equal(0m, PricingCalculator.CouponDiscount(500000, 10, null, 1000000, null));
            Assert.Equal(300000m, PricingCalculator.CouponDiscount(2000000, null, 300000, null, null));
        }

        [Fact] public void Empty_nights_rejected() { Assert.Throws<ArgumentException>(() => PricingCalculator.Calculate(new List<NightPrice>(), 1, 0, 0, 0)); }
    }

    public class CancellationTests
    {
        static readonly DateTime CheckIn = new DateTime(2030, 6, 10);

        [Fact] public void Free_cancel_before_deadline_refunds_everything()
        {
            var r = CancellationPolicy.Evaluate("FLEX1", CheckIn, "14:00", new DateTime(2030, 6, 8), 2000000, 500000);
            Assert.Equal(2000000m, r.Amount); Assert.True(r.FreeCancellation);
        }

        [Fact] public void Flex_late_cancel_keeps_first_night()
        {
            var r = CancellationPolicy.Evaluate("FLEX1", CheckIn, "14:00", new DateTime(2030, 6, 10, 9, 0, 0), 2000000, 500000);
            Assert.Equal(1500000m, r.Amount);
        }

        [Fact] public void Free24_late_cancel_refunds_nothing() { Assert.Equal(0m, CancellationPolicy.Evaluate("FREE24", CheckIn, "14:00", new DateTime(2030, 6, 10, 9, 0, 0), 2000000, 500000).Amount); }
        [Fact] public void Nonrefundable_never_refunds() { Assert.Equal(0m, CancellationPolicy.Evaluate("NONREFUND", CheckIn, "14:00", new DateTime(2030, 1, 1), 2000000, 500000).Amount); }
        [Fact] public void Nothing_paid_nothing_to_refund() { Assert.False(CancellationPolicy.Evaluate("FLEX1", CheckIn, "14:00", new DateTime(2030, 1, 1), 0, 0).Eligible); }
        [Fact] public void Deadline_is_24h_before_checkin_time() { Assert.Equal(new DateTime(2030, 6, 9, 14, 0, 0), CancellationPolicy.FreeCancelDeadline(CheckIn, "14:00")); }
    }

    public class InventoryMathTests
    {
        static InventoryMath.Hold H(int a, int b, int q) { var d = new DateTime(2030, 1, 1); return new InventoryMath.Hold { CheckIn = d.AddDays(a), CheckOut = d.AddDays(b), Qty = q }; }

        [Fact] public void Sold_out_on_a_middle_night_blocks_the_whole_stay()
        {
            var holds = new[] { H(1, 2, 2) };           // phòng hết đúng đêm giữa kỳ
            Assert.Equal(0, InventoryMath.MinAvailable(2, holds, new DateTime(2030, 1, 1), new DateTime(2030, 1, 4)));
        }

        [Fact] public void Checkout_day_is_free_for_next_guest()
        {
            var holds = new[] { H(0, 2, 1) };
            Assert.Equal(1, InventoryMath.MinAvailable(1, new InventoryMath.Hold[0], new DateTime(2030, 1, 3), new DateTime(2030, 1, 5)));
            Assert.Equal(1, InventoryMath.MinAvailable(1, holds, new DateTime(2030, 1, 3), new DateTime(2030, 1, 5)));
        }

        [Fact] public void Non_overlapping_partial_bookings_do_not_add_up()
        {
            var holds = new[] { H(0, 1, 1), H(1, 2, 1) };    // mỗi đêm chỉ 1 phòng bận
            Assert.Equal(1, InventoryMath.MinAvailable(2, holds, new DateTime(2030, 1, 1), new DateTime(2030, 1, 3)));
        }
    }

    public class StatusTests
    {
        [Theory]
        [InlineData("Chờ thanh toán", "Đã thanh toán", true)]
        [InlineData("Chờ thanh toán", "Hoàn thành", false)]
        [InlineData("Đã hủy", "Đã thanh toán", false)]
        [InlineData("Đã thanh toán", "Đã nhận phòng", true)]
        [InlineData("Hoàn thành", "Đã hủy", false)]
        [InlineData("Đã nhận phòng", "Đã hủy", false)]
        public void Transitions(string from, string to, bool ok) { Assert.Equal(ok, BookingStatus.CanTransition(from, to)); }
    }

    public class VnPaySignatureTests
    {
        const string Secret = "unit-test-secret";
        static VnPayGateway Gw() { return new VnPayGateway("TMN", Secret, "https://example.test/pay"); }

        public static NameValueCollection Signed(string txn, long amountMinor, string rc = "00", string ts = "00", string secret = Secret)
        {
            var p = new SortedDictionary<string, string>(StringComparer.Ordinal) { { "vnp_Amount", amountMinor.ToString() }, { "vnp_ResponseCode", rc }, { "vnp_TransactionNo", "14000001" }, { "vnp_TransactionStatus", ts }, { "vnp_TxnRef", txn } };
            var q = string.Join("&", p.Select(kv => System.Net.WebUtility.UrlEncode(kv.Key) + "=" + System.Net.WebUtility.UrlEncode(kv.Value)));
            var c = new NameValueCollection(); foreach (var kv in p) c[kv.Key] = kv.Value; c["vnp_SecureHash"] = VnPayGateway.Sign(secret, q); return c;
        }

        [Fact] public void Valid_signature_is_accepted() { var r = Gw().ParseCallback(Signed("T1", 5000000)); Assert.True(r.SignatureValid); Assert.True(r.Success); Assert.Equal(5000000, r.AmountMinor); }
        [Fact] public void Tampered_amount_is_rejected() { var c = Signed("T1", 5000000); c["vnp_Amount"] = "100"; Assert.False(Gw().ParseCallback(c).SignatureValid); }
        [Fact] public void Wrong_secret_is_rejected() { Assert.False(Gw().ParseCallback(Signed("T1", 5000000, secret: "other")).SignatureValid); }
        [Fact] public void Missing_hash_is_rejected() { var c = Signed("T1", 5000000); c.Remove("vnp_SecureHash"); Assert.False(Gw().ParseCallback(c).SignatureValid); }
        [Fact] public void Failed_code_is_not_success() { Assert.False(Gw().ParseCallback(Signed("T1", 5000000, "24", "02")).Success); }

        [Fact] public void Payment_url_contains_signature_and_amount_times_100()
        {
            var url = Gw().BuildPaymentUrl(new PaymentRequest { TxnRef = "TX1", Amount = 1500000, ReturnUrl = "http://x/return", OrderInfo = "test", ExpireAt = DateTime.Now.AddMinutes(15), IpAddress = "127.0.0.1" });
            Assert.Contains("vnp_Amount=150000000", url); Assert.Contains("vnp_SecureHash=", url); Assert.Contains("vnp_TxnRef=TX1", url);
        }
    }
}
