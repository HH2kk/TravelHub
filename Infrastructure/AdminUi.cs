using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using TravelHub.Services;

namespace TravelHub.Infrastructure
{
    /// <summary>Thành phần giao diện Admin: biểu đồ SVG thuần (không phụ thuộc thư viện), huy hiệu, avatar.</summary>
    public static class AdminUi
    {
        static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");
        public static readonly string[] Palette = { "#2563eb", "#0f9d8a", "#f59e0b", "#8b5cf6", "#ef4444", "#64748b", "#0ea5e9" };

        static string E(string s) { return HttpUtility.HtmlEncode(s ?? ""); }
        static string N(double v) { return v.ToString("0.##", CultureInfo.InvariantCulture); }

        /// <summary>Rút gọn tiền: 1,25 tr / 3,4 tỷ.</summary>
        public static string Short(this HtmlHelper h, decimal v)
        {
            if (v >= 1000000000m) return (v / 1000000000m).ToString("0.##", Vi) + " tỷ";
            if (v >= 1000000m) return (v / 1000000m).ToString("0.#", Vi) + " tr";
            if (v >= 1000m) return (v / 1000m).ToString("0", Vi) + "k";
            return v.ToString("0", Vi);
        }

        public static string StatusColor(string status)
        {
            switch (status)
            {
                case "Đã thanh toán": case "Đã đặt": case "Scheduled": return "#0f9d8a";
                case "Hoàn thành": case "Đã bay": case "Arrived": return "#2563eb";
                case "Đã nhận phòng": case "Đã làm thủ tục": return "#0ea5e9";
                case "Chờ thanh toán": case "Chờ xác nhận": case "Delayed": return "#f59e0b";
                case "Đã hủy": case "Hết hạn": case "Cancelled": return "#ef4444";
                default: return "#8b5cf6";
            }
        }

        /// <summary>Chip % thay đổi so với kỳ trước.</summary>
        public static IHtmlString Delta(this HtmlHelper h, decimal cur, decimal prev)
        {
            if (prev == 0 && cur == 0) return new HtmlString("<span class='delta flat'>— không đổi</span>");
            if (prev == 0) return new HtmlString("<span class='delta up'>▲ mới</span>");
            var pct = (double)((cur - prev) / prev * 100m);
            var cls = pct > 0.5 ? "up" : pct < -0.5 ? "down" : "flat";
            var arrow = pct > 0.5 ? "▲" : pct < -0.5 ? "▼" : "•";
            return new HtmlString("<span class='delta " + cls + "'>" + arrow + " " + Math.Abs(pct).ToString("0", Vi) + "% <small>so với kỳ trước</small></span>");
        }

        public static IHtmlString Avatar(this HtmlHelper h, string name, int size = 36)
        {
            name = (name ?? "?").Trim();
            var parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var ini = parts.Length == 0 ? "?" : parts.Length == 1 ? parts[0].Substring(0, 1) : parts[parts.Length - 1].Substring(0, 1) + parts[0].Substring(0, 1);
            var hue = Math.Abs(name.Aggregate(7, (a, ch) => a * 31 + ch)) % 360;
            return new HtmlString("<span class='av' style='width:" + size + "px;height:" + size + "px;font-size:" + (size * 0.38).ToString("0", CultureInfo.InvariantCulture) + "px;background:hsl(" + hue + ",55%,92%);color:hsl(" + hue + ",55%,32%)' aria-hidden='true'>" + E(ini.ToUpperInvariant()) + "</span>");
        }

        public static IHtmlString AirlineBadge(this HtmlHelper h, string iata)
        {
            return new HtmlString("<span class='al' title='" + E(iata) + "'>" + E(iata) + "</span>");
        }

        public static IHtmlString FlightBadge(this HtmlHelper h, string status)
        {
            return new HtmlString("<span class='badge-st " + FlightInfo.StatusCls(status) + "'>" + E(FlightInfo.StatusText(status)) + "</span>");
        }

        public static IHtmlString TicketBadge(this HtmlHelper h, string text, string code)
        {
            return new HtmlString("<span class='badge-st " + FlightInfo.TicketCls(code) + "'>" + E(text) + "</span>");
        }

        /// <summary>Thanh tải ghế: màu theo mức lấp đầy.</summary>
        public static IHtmlString LoadBar(this HtmlHelper h, int pct)
        {
            var cls = pct >= 90 ? "hot" : pct >= 60 ? "mid" : "low";
            return new HtmlString("<div class='load " + cls + "' title='" + pct + "% ghế đã bán'><i style='width:" + Math.Min(100, pct) + "%'></i></div>");
        }

        /// <summary>Biểu đồ vùng + đường (SVG). values/labels cùng độ dài; tooltip khi rê chuột.</summary>
        public static IHtmlString AreaChart(this HtmlHelper h, IList<decimal> values, IList<string> labels, string color, Func<decimal, string> fmt, string id)
        {
            const double W = 640, H = 200, padL = 8, padR = 8, padT = 14, padB = 22;
            int n = values.Count;
            if (n == 0) return new HtmlString("");
            double max = (double)values.Max(); if (max <= 0) max = 1;
            double stepX = n > 1 ? (W - padL - padR) / (n - 1) : 0;
            Func<int, double> x = i => padL + i * stepX;
            Func<int, double> y = i => padT + (H - padT - padB) * (1 - (double)values[i] / max);
            var pts = string.Join(" ", Enumerable.Range(0, n).Select(i => N(x(i)) + "," + N(y(i))));
            var area = "M" + N(x(0)) + "," + N(H - padB) + " L" + pts.Replace(" ", " L") + " L" + N(x(n - 1)) + "," + N(H - padB) + " Z";
            var sb = new StringBuilder();
            sb.Append("<svg class='chart' viewBox='0 0 " + W + " " + H + "' preserveAspectRatio='none' role='img' aria-label='Biểu đồ " + E(id) + "'>");
            sb.Append("<defs><linearGradient id='g-" + id + "' x1='0' y1='0' x2='0' y2='1'><stop offset='0%' stop-color='" + color + "' stop-opacity='.28'/><stop offset='100%' stop-color='" + color + "' stop-opacity='0'/></linearGradient></defs>");
            for (int g = 0; g <= 3; g++) { var gy = padT + (H - padT - padB) * g / 3.0; sb.Append("<line x1='" + padL + "' x2='" + (W - padR) + "' y1='" + N(gy) + "' y2='" + N(gy) + "' stroke='#e2e8f0' stroke-dasharray='3 4' vector-effect='non-scaling-stroke'/>"); }
            sb.Append("<path d='" + area + "' fill='url(#g-" + id + ")'/>");
            sb.Append("<polyline points='" + pts + "' fill='none' stroke='" + color + "' stroke-width='2.4' stroke-linejoin='round' stroke-linecap='round' vector-effect='non-scaling-stroke'/>");
            double colW = n > 1 ? stepX : W;
            for (int i = 0; i < n; i++)
                sb.Append("<g class='hit'><rect x='" + N(x(i) - colW / 2) + "' y='0' width='" + N(colW) + "' height='" + H + "' fill='transparent'><title>" + E(labels[i]) + ": " + E(fmt(values[i])) + "</title></rect><circle cx='" + N(x(i)) + "' cy='" + N(y(i)) + "' r='3.6' fill='#fff' stroke='" + color + "' stroke-width='2' class='dot'/></g>");
            sb.Append("</svg>");
            return new HtmlString(sb.ToString());
        }

        /// <summary>Cột đơn giản (SVG) cho dữ liệu đếm.</summary>
        public static IHtmlString BarChart(this HtmlHelper h, IList<int> values, IList<string> labels, string color, string unit)
        {
            int n = values.Count; if (n == 0) return new HtmlString("");
            int max = Math.Max(1, values.Max());
            var sb = new StringBuilder("<div class='cols' role='img' aria-label='Biểu đồ cột'>");
            for (int i = 0; i < n; i++) sb.Append("<div class='col' title='" + E(labels[i]) + ": " + values[i] + " " + E(unit) + "'><i style='height:" + Math.Max(values[i] == 0 ? 2 : 6, values[i] * 100 / max) + "%;background:" + color + "'></i></div>");
            sb.Append("</div>");
            return new HtmlString(sb.ToString());
        }

        /// <summary>Donut + chú thích.</summary>
        public static IHtmlString Donut(this HtmlHelper h, IList<KeyValuePair<string, int>> items, string centerLabel)
        {
            int total = items.Sum(i => i.Value);
            if (total == 0) return new HtmlString("<div class='text-muted small py-4 text-center'>Chưa có dữ liệu.</div>");
            const double R = 15.9155; // chu vi = 100
            var sb = new StringBuilder("<div class='donut-wrap'><svg class='donut' viewBox='0 0 42 42' role='img' aria-label='Phân bố theo trạng thái'><circle cx='21' cy='21' r='" + N(R) + "' fill='none' stroke='#eef2f7' stroke-width='5'/>");
            double offset = 25; // bắt đầu từ 12 giờ
            foreach (var it in items)
            {
                double pct = it.Value * 100.0 / total;
                sb.Append("<circle cx='21' cy='21' r='" + N(R) + "' fill='none' stroke='" + AdminUi.StatusColor(it.Key) + "' stroke-width='5' stroke-dasharray='" + N(pct) + " " + N(100 - pct) + "' stroke-dashoffset='" + N(offset) + "'><title>" + E(it.Key) + ": " + it.Value + "</title></circle>");
                offset -= pct;
            }
            sb.Append("<text x='21' y='21.2' text-anchor='middle' class='dn'>" + total + "</text><text x='21' y='26' text-anchor='middle' class='dl'>" + E(centerLabel) + "</text></svg><ul class='legend'>");
            foreach (var it in items) sb.Append("<li><i style='background:" + StatusColor(it.Key) + "'></i><span>" + E(it.Key) + "</span><b>" + it.Value + "</b></li>");
            sb.Append("</ul></div>");
            return new HtmlString(sb.ToString());
        }
    }
}
