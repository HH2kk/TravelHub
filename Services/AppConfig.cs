using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Linq;
using Dapper;
using TravelHub.Data;

namespace TravelHub.Services
{
    /// <summary>Đọc cấu hình: biến môi trường TRAVELHUB_* ưu tiên, sau đó appSettings (kèm App_Data\secrets.config).
    /// Mã bí mật không bao giờ được commit.</summary>
    public static class AppConfig
    {
        public static string Get(string key, string fallback = null)
        {
            var env = Environment.GetEnvironmentVariable("TRAVELHUB_" + key.ToUpperInvariant().Replace(":", "__").Replace(".", "_"));
            if (!string.IsNullOrWhiteSpace(env)) return env;
            var v = ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(v) ? fallback : v;
        }

        public static string BaseUrl { get { return (Get("App:BaseUrl", "http://localhost:49688")).TrimEnd('/'); } }

        // ---- Cấu hình lưu trong DB (CauHinhHeThong), cache 60 giây ----
        static readonly ConcurrentDictionary<string, Tuple<DateTime, string>> Cache = new ConcurrentDictionary<string, Tuple<DateTime, string>>();

        public static string Setting(string key, string fallback = null)
        {
            Tuple<DateTime, string> hit;
            if (Cache.TryGetValue(key, out hit) && hit.Item1 > DateTime.UtcNow) return hit.Item2;
            string val = null;
            try
            {
                using (var c = TravelDb.Open())
                    val = c.ExecuteScalar<string>("SELECT GiaTri FROM CauHinhHeThong WHERE Khoa=@k", new { k = key });
            }
            catch { /* DB chưa sẵn sàng: dùng giá trị mặc định */ }
            val = val ?? fallback;
            Cache[key] = Tuple.Create(DateTime.UtcNow.AddSeconds(60), val);
            return val;
        }

        public static decimal SettingDecimal(string key, decimal fallback)
        {
            decimal d;
            return decimal.TryParse(Setting(key), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out d) ? d : fallback;
        }

        public static void SetSetting(string key, string value)
        {
            using (var c = TravelDb.Open())
                c.Execute(@"MERGE CauHinhHeThong AS t USING (SELECT @k Khoa, @v GiaTri) s ON t.Khoa=s.Khoa
                            WHEN MATCHED THEN UPDATE SET GiaTri=s.GiaTri
                            WHEN NOT MATCHED THEN INSERT (Khoa,GiaTri) VALUES (s.Khoa,s.GiaTri);", new { k = key, v = value });
            Tuple<DateTime, string> _; Cache.TryRemove(key, out _);
        }

        public static int HoldMinutes { get { return (int)SettingDecimal("GiuPhongPhut", 15); } }
    }
}
