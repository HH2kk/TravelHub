using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Hosting;
using Dapper;
using TravelHub.Data;

namespace TravelHub.Services
{
    /// <summary>Template email nằm trong App_Data\EmailTemplates. {{Key}} được HTML-encode; {{KeyHtml}} giữ nguyên HTML.</summary>
    public static class EmailTemplates
    {
        static string ReadTemplate(string name)
        {
            var dir = HostingEnvironment.MapPath("~/App_Data/EmailTemplates") ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "EmailTemplates");
            return File.ReadAllText(Path.Combine(dir, name + ".html"), Encoding.UTF8);
        }

        public static string Fill(string tpl, IDictionary<string, string> values)
        {
            foreach (var kv in values)
            {
                var v = kv.Value ?? "";
                tpl = tpl.Replace("{{" + kv.Key + "}}", kv.Key.EndsWith("Html") ? v : HttpUtility.HtmlEncode(v));
            }
            return tpl;
        }

        public static string Render(string name, IDictionary<string, string> values)
        {
            var body = Fill(ReadTemplate(name), values);
            var title = values.ContainsKey("Title") ? values["Title"] : "TravelHub";
            return Fill(ReadTemplate("_layout"), new Dictionary<string, string> {
                { "Title", title }, { "BodyHtml", body },
                { "SupportEmail", AppConfig.Setting("EmailHoTro", "support@travelhub.vn") }, { "BaseUrl", AppConfig.BaseUrl } });
        }
    }

    public static class EmailService
    {
        const int MaxAttempts = 5;

        /// <summary>Xếp email vào hàng đợi. Nếu khoaTrung đã tồn tại thì bỏ qua (chống gửi trùng khi webhook lặp).</summary>
        public static bool Enqueue(string type, string dedupeKey, string to, string subject, string html, string receiptNo = null)
        {
            if (string.IsNullOrWhiteSpace(to)) return false;
            try
            {
                using (var c = TravelDb.Open())
                    c.Execute(@"INSERT INTO EmailOutbox (Loai, KhoaTrung, Den, TieuDe, NoiDungHtml, TepDinhKem) VALUES (@type,@k,@to,@s,@h,@r)",
                        new { type, k = dedupeKey, to, s = subject, h = html, r = receiptNo });
                EmailWorker.Poke();
                return true;
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627) { return false; }
        }

        public static void ResetForRetry(string dedupeKey)
        {
            using (var c = TravelDb.Open())
                c.Execute("UPDATE EmailOutbox SET TrangThai='PENDING', SoLanThu=0 WHERE KhoaTrung=@k", new { k = dedupeKey });
            EmailWorker.Poke();
        }

        static SmtpClient CreateClient()
        {
            var host = AppConfig.Get("Smtp:Host");
            var pickup = AppConfig.Get("Smtp:PickupDirectory");
            var client = new SmtpClient();
            if (!string.IsNullOrWhiteSpace(pickup))
            {
                Directory.CreateDirectory(pickup);
                client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                client.PickupDirectoryLocation = pickup;
                return client;
            }
            if (string.IsNullOrWhiteSpace(host)) return null;
            client.Host = host;
            client.Port = int.Parse(AppConfig.Get("Smtp:Port", "587"));
            client.EnableSsl = !string.Equals(AppConfig.Get("Smtp:EnableSsl", "true"), "false", StringComparison.OrdinalIgnoreCase);
            var user = AppConfig.Get("Smtp:User");
            if (!string.IsNullOrEmpty(user)) client.Credentials = new NetworkCredential(user, AppConfig.Get("Smtp:Password"));
            client.Timeout = 20000;
            return client;
        }

        /// <summary>Gửi các email PENDING. Trả về số email đã gửi thành công.</summary>
        public static int ProcessPending()
        {
            int sent = 0;
            List<dynamic> rows;
            using (var c = TravelDb.Open())
                rows = new List<dynamic>(c.Query("SELECT TOP 50 * FROM EmailOutbox WHERE TrangThai='PENDING' AND SoLanThu < @m ORDER BY MaEmail", new { m = MaxAttempts }));
            if (rows.Count == 0) return 0;

            using (var client = CreateClient())
            {
                foreach (var r in rows)
                {
                    long id = r.MaEmail;
                    if (client == null)
                    {
                        Update(id, "PENDING", false, "SMTP chưa được cấu hình (Smtp:Host hoặc Smtp:PickupDirectory).");
                        continue;
                    }
                    try
                    {
                        var from = AppConfig.Get("Smtp:From", "no-reply@travelhub.vn");
                        using (var msg = new MailMessage(new MailAddress(from, "TravelHub"), new MailAddress((string)r.Den)))
                        {
                            msg.Subject = r.TieuDe; msg.SubjectEncoding = Encoding.UTF8; msg.BodyEncoding = Encoding.UTF8;
                            msg.IsBodyHtml = true; msg.Body = r.NoiDungHtml;
                            if (r.TepDinhKem != null)
                            {
                                var pdf = ReceiptService.RenderPdfBySoBienNhan((string)r.TepDinhKem);
                                if (pdf != null) msg.Attachments.Add(new Attachment(new MemoryStream(pdf), r.TepDinhKem + ".pdf", "application/pdf"));
                            }
                            client.Send(msg);
                        }
                        Update(id, "SENT", true, null); sent++;
                    }
                    catch (Exception ex)
                    {
                        Update(id, "PENDING", false, Truncate(ex.Message, 480), true);
                    }
                }
            }
            return sent;
        }

        static void Update(long id, string status, bool sent, string error, bool countAttempt = false)
        {
            using (var c = TravelDb.Open())
                c.Execute(@"UPDATE EmailOutbox SET TrangThai = CASE WHEN @cnt=1 AND SoLanThu+1 >= @max AND @st='PENDING' THEN 'FAILED' ELSE @st END,
                            SoLanThu = SoLanThu + CASE WHEN @cnt=1 OR @sent=1 THEN 1 ELSE 0 END,
                            NgayGui = CASE WHEN @sent=1 THEN SYSUTCDATETIME() ELSE NgayGui END, LoiCuoi=@err WHERE MaEmail=@id",
                    new { id, st = status, sent, err = error, cnt = countAttempt, max = MaxAttempts });
        }

        static string Truncate(string s, int n) { return s != null && s.Length > n ? s.Substring(0, n) : s; }
    }
}
