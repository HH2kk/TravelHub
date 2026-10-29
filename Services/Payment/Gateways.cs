using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace TravelHub.Services.Payment
{
    public class PaymentRequest
    {
        public string TxnRef { get; set; }
        public decimal Amount { get; set; }          // VND
        public string OrderInfo { get; set; }
        public string IpAddress { get; set; }
        public string ReturnUrl { get; set; }
        public DateTime ExpireAt { get; set; }
    }

    public class CallbackResult
    {
        public bool SignatureValid { get; set; }
        public string TxnRef { get; set; }
        public long AmountMinor { get; set; }        // VNPay: VND × 100
        public bool Success { get; set; }
        public string ResponseCode { get; set; }
        public string TransactionStatus { get; set; }
        public string GatewayTxnNo { get; set; }
    }

    public interface IPaymentGateway
    {
        string Name { get; }
        string DisplayName { get; }
        /// <summary>Đã có đủ credential (TmnCode/HashSecret...) hay chưa.</summary>
        bool IsConfigured { get; }
        string BuildPaymentUrl(PaymentRequest r);
        CallbackResult ParseCallback(NameValueCollection query);
    }

    public static class Gateways
    {
        public static IPaymentGateway Get(string name)
        {
            switch ((name ?? "").ToUpperInvariant())
            {
                case "VNPAY": return new VnPayGateway();
                case "MOMO": return new MoMoGateway();
                default: return null;
            }
        }

        public static IEnumerable<IPaymentGateway> All { get { return new IPaymentGateway[] { new VnPayGateway(), new MoMoGateway() }; } }
    }

    /// <summary>VNPay 2.1.0 (HMAC-SHA512). Cần Merchant sandbox: Vnpay:TmnCode, Vnpay:HashSecret, Vnpay:Url.</summary>
    public class VnPayGateway : IPaymentGateway
    {
        readonly string _tmn, _secret, _url;

        public VnPayGateway()
        {
            _tmn = AppConfig.Get("Vnpay:TmnCode");
            _secret = AppConfig.Get("Vnpay:HashSecret");
            _url = AppConfig.Get("Vnpay:Url", "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html");
        }

        /// <summary>Dùng cho unit test: truyền secret trực tiếp.</summary>
        public VnPayGateway(string tmnCode, string hashSecret, string url) { _tmn = tmnCode; _secret = hashSecret; _url = url; }

        public string Name { get { return "VNPAY"; } }
        public string DisplayName { get { return "VNPay (ATM / thẻ / QR)"; } }
        public bool IsConfigured
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_tmn) && !string.IsNullOrWhiteSpace(_secret)
                    && AppConfig.Setting("BatVNPAY", "1") == "1";
            }
        }

        public static string Sign(string secret, string data)
        {
            using (var h = new HMACSHA512(Encoding.UTF8.GetBytes(secret)))
                return string.Concat(h.ComputeHash(Encoding.UTF8.GetBytes(data)).Select(b => b.ToString("x2")));
        }

        static string BuildQuery(SortedDictionary<string, string> p)
        {
            return string.Join("&", p.Where(kv => !string.IsNullOrEmpty(kv.Value))
                .Select(kv => WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value)));
        }

        public string BuildPaymentUrl(PaymentRequest r)
        {
            if (!IsConfigured) throw new InvalidOperationException("VNPay chưa được cấu hình (thiếu Vnpay:TmnCode / Vnpay:HashSecret).");
            var vn = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, vn);
            var expire = TimeZoneInfo.ConvertTimeFromUtc(r.ExpireAt.ToUniversalTime(), vn);
            var p = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                { "vnp_Version", "2.1.0" }, { "vnp_Command", "pay" }, { "vnp_TmnCode", _tmn },
                { "vnp_Amount", (r.Amount * 100m).ToString("0", CultureInfo.InvariantCulture) },
                { "vnp_CreateDate", now.ToString("yyyyMMddHHmmss") }, { "vnp_CurrCode", "VND" },
                { "vnp_IpAddr", r.IpAddress ?? "127.0.0.1" }, { "vnp_Locale", "vn" },
                { "vnp_OrderInfo", r.OrderInfo }, { "vnp_OrderType", "other" },
                { "vnp_ReturnUrl", r.ReturnUrl }, { "vnp_TxnRef", r.TxnRef },
                { "vnp_ExpireDate", expire.ToString("yyyyMMddHHmmss") }
            };
            var q = BuildQuery(p);
            return _url + "?" + q + "&vnp_SecureHash=" + Sign(_secret, q);
        }

        public CallbackResult ParseCallback(NameValueCollection query)
        {
            var res = new CallbackResult();
            if (query == null || string.IsNullOrEmpty(_secret)) return res;
            var p = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (string k in query.AllKeys)
                if (k != null && k.StartsWith("vnp_", StringComparison.Ordinal) && k != "vnp_SecureHash" && k != "vnp_SecureHashType")
                    p[k] = query[k];
            var given = query["vnp_SecureHash"] ?? "";
            var expected = Sign(_secret, BuildQuery(p));
            res.SignatureValid = ConstantTimeEquals(given.ToLowerInvariant(), expected);
            res.TxnRef = query["vnp_TxnRef"];
            long amt; long.TryParse(query["vnp_Amount"], NumberStyles.None, CultureInfo.InvariantCulture, out amt);
            res.AmountMinor = amt;
            res.ResponseCode = query["vnp_ResponseCode"];
            res.TransactionStatus = query["vnp_TransactionStatus"];
            res.GatewayTxnNo = query["vnp_TransactionNo"];
            res.Success = res.ResponseCode == "00" && res.TransactionStatus == "00";
            return res;
        }

        static bool ConstantTimeEquals(string a, string b)
        {
            if (a.Length != b.Length) return false;
            int d = 0;
            for (int i = 0; i < a.Length; i++) d |= a[i] ^ b[i];
            return d == 0;
        }
    }

    /// <summary>MoMo: CHƯA tích hợp (cần tài khoản merchant + tài liệu API). Luôn báo chưa cấu hình, không giả lập thanh toán.</summary>
    public class MoMoGateway : IPaymentGateway
    {
        public string Name { get { return "MOMO"; } }
        public string DisplayName { get { return "Ví MoMo"; } }
        public bool IsConfigured { get { return false; } }
        public string BuildPaymentUrl(PaymentRequest r) { throw new NotSupportedException("Cổng MoMo chưa được tích hợp (cần credential merchant)."); }
        public CallbackResult ParseCallback(NameValueCollection query) { return new CallbackResult(); }
    }
}
