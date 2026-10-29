using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using TravelHub.Services;

namespace TravelHub.Infrastructure
{
    public static class Ui
    {
        static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

        // Bộ icon dạng Lucide (stroke 2, 24x24) nhúng trực tiếp để không phụ thuộc CDN.
        static readonly Dictionary<string, string> Paths = new Dictionary<string, string>
        {
            { "search", "<circle cx='11' cy='11' r='8'/><path d='m21 21-4.3-4.3'/>" },
            { "heart", "<path d='M19 14c1.49-1.46 3-3.21 3-5.5A5.5 5.5 0 0 0 16.5 3c-1.76 0-3 .5-4.5 2-1.5-1.5-2.74-2-4.5-2A5.5 5.5 0 0 0 2 8.5c0 2.3 1.5 4.05 3 5.5l7 7Z'/>" },
            { "map-pin", "<path d='M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0Z'/><circle cx='12' cy='10' r='3'/>" },
            { "calendar", "<rect width='18' height='18' x='3' y='4' rx='2'/><path d='M16 2v4M8 2v4M3 10h18'/>" },
            { "user", "<path d='M19 21v-2a4 4 0 0 0-4-4H9a4 4 0 0 0-4 4v2'/><circle cx='12' cy='7' r='4'/>" },
            { "users", "<path d='M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2'/><circle cx='9' cy='7' r='4'/><path d='M22 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75'/>" },
            { "bed", "<path d='M2 4v16M2 8h18a2 2 0 0 1 2 2v10M2 17h20M6 8v9'/>" },
            { "star", "<path d='m12 2 3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z'/>" },
            { "wifi", "<path d='M5 13a10 10 0 0 1 14 0M8.5 16.5a5 5 0 0 1 7 0M2 8.82a15 15 0 0 1 20 0M12 20h.01'/>" },
            { "check", "<path d='M20 6 9 17l-5-5'/>" },
            { "shield", "<path d='M20 13c0 5-3.5 7.5-7.66 8.95a1 1 0 0 1-.67-.01C7.5 20.5 4 18 4 13V6a1 1 0 0 1 1-1c2 0 4.5-1.2 6.24-2.72a1.17 1.17 0 0 1 1.52 0C14.51 3.81 17 5 19 5a1 1 0 0 1 1 1z'/><path d='m9 12 2 2 4-4'/>" },
            { "tag", "<path d='M12.586 2.586A2 2 0 0 0 11.172 2H4a2 2 0 0 0-2 2v7.172a2 2 0 0 0 .586 1.414l8.704 8.704a2.426 2.426 0 0 0 3.42 0l6.58-6.58a2.426 2.426 0 0 0 0-3.42z'/><circle cx='7.5' cy='7.5' r='.5'/>" },
            { "headset", "<path d='M3 14h3a2 2 0 0 1 2 2v3a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-7a9 9 0 0 1 18 0v7a2 2 0 0 1-2 2h-1a2 2 0 0 1-2-2v-3a2 2 0 0 1 2-2h3'/>" },
            { "plane", "<path d='M17.8 19.2 16 11l3.5-3.5C21 6 21.5 4 21 3c-1-.5-3 0-4.5 1.5L13 8 4.8 6.2c-.5-.1-.9.1-1.1.5l-.3.5c-.2.5-.1 1 .3 1.3L9 12l-2 3H4l-1 1 3 2 2 3 1-1v-3l3-2 3.5 5.3c.3.4.8.5 1.3.3l.5-.2c.4-.3.6-.7.5-1.2z'/>" },
            { "menu", "<path d='M4 6h16M4 12h16M4 18h16'/>" },
            { "x", "<path d='M18 6 6 18M6 6l12 12'/>" },
            { "log-out", "<path d='M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4M16 17l5-5-5-5M21 12H9'/>" },
            { "clock", "<circle cx='12' cy='12' r='10'/><path d='M12 6v6l4 2'/>" },
            { "download", "<path d='M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4M7 10l5 5 5-5M12 15V3'/>" },
            { "mail", "<rect width='20' height='16' x='2' y='4' rx='2'/><path d='m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7'/>" },
            { "chevron-right", "<path d='m9 18 6-6-6-6'/>" },
            { "chevron-left", "<path d='m15 18-6-6 6-6'/>" },
            { "layout", "<rect width='7' height='9' x='3' y='3' rx='1'/><rect width='7' height='5' x='14' y='3' rx='1'/><rect width='7' height='9' x='14' y='12' rx='1'/><rect width='7' height='5' x='3' y='16' rx='1'/>" },
            { "credit-card", "<rect width='20' height='14' x='2' y='5' rx='2'/><path d='M2 10h20'/>" },
            { "file-text", "<path d='M15 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7z'/><path d='M14 2v4a2 2 0 0 0 2 2h4M10 9H8M16 13H8M16 17H8'/>" },
            { "settings", "<path d='M12.22 2h-.44a2 2 0 0 0-2 2v.18a2 2 0 0 1-1 1.73l-.43.25a2 2 0 0 1-2 0l-.15-.08a2 2 0 0 0-2.73.73l-.22.38a2 2 0 0 0 .73 2.73l.15.1a2 2 0 0 1 1 1.72v.51a2 2 0 0 1-1 1.74l-.15.09a2 2 0 0 0-.73 2.73l.22.38a2 2 0 0 0 2.73.73l.15-.08a2 2 0 0 1 2 0l.43.25a2 2 0 0 1 1 1.73V20a2 2 0 0 0 2 2h.44a2 2 0 0 0 2-2v-.18a2 2 0 0 1 1-1.73l.43-.25a2 2 0 0 1 2 0l.15.08a2 2 0 0 0 2.73-.73l.22-.39a2 2 0 0 0-.73-2.73l-.15-.08a2 2 0 0 1-1-1.74v-.5a2 2 0 0 1 1-1.74l.15-.09a2 2 0 0 0 .73-2.73l-.22-.38a2 2 0 0 0-2.73-.73l-.15.08a2 2 0 0 1-2 0l-.43-.25a2 2 0 0 1-1-1.73V4a2 2 0 0 0-2-2z'/><circle cx='12' cy='12' r='3'/>" },
            { "building", "<rect width='16' height='20' x='4' y='2' rx='2'/><path d='M9 22v-4h6v4M8 6h.01M16 6h.01M12 6h.01M12 10h.01M12 14h.01M16 10h.01M16 14h.01M8 10h.01M8 14h.01'/>" },
            { "message", "<path d='M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z'/>" },
            { "newspaper", "<path d='M4 22h16a2 2 0 0 0 2-2V4a2 2 0 0 0-2-2H8a2 2 0 0 0-2 2v16a2 2 0 0 1-2 2Zm0 0a2 2 0 0 1-2-2v-9c0-1.1.9-2 2-2h2M18 14h-8M15 18h-5M10 6h8v4h-8z'/>" },
            { "alert", "<path d='m21.73 18-8-14a2 2 0 0 0-3.48 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.73-3'/><path d='M12 9v4M12 17h.01'/>" },
            { "trash", "<path d='M3 6h18M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2'/>" },
            { "edit", "<path d='M12 3H5a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7'/><path d='M18.375 2.625a1 1 0 0 1 3 3l-9.013 9.014a2 2 0 0 1-.853.505l-2.873.84a.5.5 0 0 1-.62-.62l.84-2.873a2 2 0 0 1 .506-.852z'/>" },
            { "plus", "<path d='M5 12h14M12 5v14'/>" },
            { "image", "<rect width='18' height='18' x='3' y='3' rx='2'/><circle cx='9' cy='9' r='2'/><path d='m21 15-3.09-3.09a2 2 0 0 0-2.82 0L6 21'/>" },
            { "coffee", "<path d='M10 2v2M14 2v2M16 8a1 1 0 0 1 1 1v8a4 4 0 0 1-4 4H7a4 4 0 0 1-4-4V9a1 1 0 0 1 1-1h14a4 4 0 1 1 0 8h-1M6 2v2'/>" },
            { "car", "<path d='M19 17h2c.6 0 1-.4 1-1v-3c0-.9-.7-1.7-1.5-1.9C18.7 10.6 16 10 16 10s-1.3-1.4-2.2-2.3c-.5-.4-1.1-.7-1.8-.7H5c-.6 0-1.1.4-1.4.9l-1.4 2.9A3.7 3.7 0 0 0 2 12v4c0 .6.4 1 1 1h2'/><circle cx='7' cy='17' r='2'/><path d='M9 17h6'/><circle cx='17' cy='17' r='2'/>" },
            { "waves", "<path d='M2 6c.6.5 1.2 1 2.5 1C7 7 7 5 9.5 5c2.6 0 2.4 2 5 2 2.5 0 2.5-2 5-2 1.3 0 1.9.5 2.5 1M2 12c.6.5 1.2 1 2.5 1 2.5 0 2.5-2 5-2 2.6 0 2.4 2 5 2 2.5 0 2.5-2 5-2 1.3 0 1.9.5 2.5 1M2 18c.6.5 1.2 1 2.5 1 2.5 0 2.5-2 5-2 2.6 0 2.4 2 5 2 2.5 0 2.5-2 5-2 1.3 0 1.9.5 2.5 1'/>" },
            { "wind", "<path d='M17.7 7.7a2.5 2.5 0 1 1 1.8 4.3H2M9.6 4.6A2 2 0 1 1 11 8H2M12.6 19.4A2 2 0 1 0 14 16H2'/>" },
            { "ticket", "<path d='M2 9a3 3 0 0 1 0 6v2a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2v-2a3 3 0 0 1 0-6V7a2 2 0 0 0-2-2H4a2 2 0 0 0-2 2Z'/><path d='M13 5v2M13 17v2M13 11v2'/>" },
            { "trending-up", "<path d='m22 7-8.5 8.5-5-5L2 17'/><path d='M16 7h6v6'/>" },
            { "bar-chart", "<path d='M12 20V10M18 20V4M6 20v-4'/>" },
            { "phone", "<path d='M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z'/>" },
            { "bell", "<path d='M6 8a6 6 0 0 1 12 0c0 7 3 9 3 9H3s3-2 3-9M10.3 21a1.94 1.94 0 0 0 3.4 0'/>" },
            { "eye", "<path d='M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z'/><circle cx='12' cy='12' r='3'/>" },
            { "check-circle", "<path d='M22 11.08V12a10 10 0 1 1-5.93-9.14'/><path d='m9 11 3 3L22 4'/>" },
            { "wallet", "<path d='M19 7V4a1 1 0 0 0-1-1H5a2 2 0 0 0 0 4h15a1 1 0 0 1 1 1v4h-3a2 2 0 0 0 0 4h3a1 1 0 0 0 1-1v-2a1 1 0 0 0-1-1'/><path d='M3 5v14a2 2 0 0 0 2 2h15a1 1 0 0 0 1-1v-4'/>" },
            { "activity", "<path d='M22 12h-2.48a2 2 0 0 0-1.93 1.46l-2.35 8.36a.25.25 0 0 1-.48 0L9.24 2.18a.25.25 0 0 0-.48 0l-2.35 8.36A2 2 0 0 1 4.49 12H2'/>" },
            { "briefcase", "<path d='M16 20V4a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16'/><rect width='20' height='14' x='2' y='6' rx='2'/>" },
            { "arrow-right", "<path d='M5 12h14M12 5l7 7-7 7'/>" },
            { "history", "<path d='M3 12a9 9 0 1 0 9-9 9.75 9.75 0 0 0-6.74 2.74L3 8'/><path d='M3 3v5h5M12 7v5l4 2'/>" },
            { "refresh","<path d='M3 12a9 9 0 0 1 9-9 9.75 9.75 0 0 1 6.74 2.74L21 8M21 3v5h-5M21 12a9 9 0 0 1-9 9 9.75 9.75 0 0 1-6.74-2.74L3 16M8 16H3v5'/>" },
        };

        public static IHtmlString Icon(this HtmlHelper h, string name, int size = 18, string cls = null)
        {
            string p;
            if (!Paths.TryGetValue(name, out p)) p = "<circle cx='12' cy='12' r='9'/>";
            return new HtmlString(string.Format("<svg class='ic {3}' width='{1}' height='{1}' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2' stroke-linecap='round' stroke-linejoin='round' aria-hidden='true'>{0}</svg>", p, size, "", cls));
        }

        public static string Money(this HtmlHelper h, decimal v) { return MoneyText(v); }
        public static string MoneyText(decimal v) { return v.ToString("N0", Vi) + " ₫"; }
        public static string Date(this HtmlHelper h, DateTime d) { return d.ToString("dd/MM/yyyy", Vi); }
        public static string DateTimeText(this HtmlHelper h, DateTime d) { return d.ToString("HH:mm dd/MM/yyyy", Vi); }

        public static IHtmlString Stars(this HtmlHelper h, int n)
        {
            var s = "";
            for (int i = 0; i < n; i++) s += "<svg width='14' height='14' viewBox='0 0 24 24' fill='#f5b301' stroke='#f5b301' aria-hidden='true'><path d='m12 2 3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z'/></svg>";
            return new HtmlString("<span class='stars' role='img' aria-label='" + n + " sao'>" + s + "</span>");
        }

        public static string Img(this HtmlHelper h, string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "/Content/images/placeholder.svg";
            return path.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? "/Content/images/placeholder.svg" : path;
        }

        public static IHtmlString StatusBadge(this HtmlHelper h, string status)
        {
            string cls;
            switch (BookingStatus.Code(status))
            {
                case "Paid": cls = "ok"; break;
                case "Completed": cls = "ok"; break;
                case "CheckedIn": cls = "info"; break;
                case "Cancelled": case "Expired": cls = "bad"; break;
                default: cls = "warn"; break;
            }
            return new HtmlString("<span class='badge-st " + cls + "'>" + HttpUtility.HtmlEncode(status) + "</span>");
        }

        public static IHtmlString PayBadge(this HtmlHelper h, string status)
        {
            string cls = status == PaymentStatus.Succeeded || status == "Đã thanh toán" ? "ok" : status == PaymentStatus.Failed ? "bad" : status == PaymentStatus.Reconcile ? "warn" : "info";
            return new HtmlString("<span class='badge-st " + cls + "'>" + HttpUtility.HtmlEncode(status) + "</span>");
        }

        public static IHtmlString Toasts(this HtmlHelper h)
        {
            var td = h.ViewContext.TempData; var sb = new System.Text.StringBuilder();
            if (td["ok"] != null) sb.Append("<div class='toast-msg ok' role='status'>" + HttpUtility.HtmlEncode(td["ok"].ToString()) + "</div>");
            if (td["err"] != null) sb.Append("<div class='toast-msg err' role='alert'>" + HttpUtility.HtmlEncode(td["err"].ToString()) + "</div>");
            return new HtmlString(sb.ToString());
        }

        /// <summary>Tạo URL phân trang giữ nguyên toàn bộ query hiện tại.</summary>
        public static string PageUrl(this UrlHelper u, int page)
        {
            var q = HttpUtility.ParseQueryString(u.RequestContext.HttpContext.Request.Url.Query);
            q["page"] = page.ToString();
            return u.RequestContext.HttpContext.Request.Path + "?" + q;
        }
    }
}
