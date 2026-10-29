using System;
using System.Threading;
using System.Web.Hosting;
using Dapper;
using TravelHub.Data;

namespace TravelHub.Services
{
    /// <summary>Tác vụ nền (thay BackgroundService của .NET Core): gửi email trong outbox & giải phóng phòng giữ chỗ hết hạn.</summary>
    public class BackgroundWorker : IRegisteredObject
    {
        static BackgroundWorker _instance;
        readonly Timer _timer;
        int _busy;

        public static void Start()
        {
            if (_instance != null) return;
            _instance = new BackgroundWorker();
            HostingEnvironment.RegisterObject(_instance);
        }

        BackgroundWorker() { _timer = new Timer(Tick, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30)); }

        public static void PokeNow() { if (_instance != null) _instance._timer.Change(0, 30000); }

        void Tick(object state)
        {
            if (Interlocked.Exchange(ref _busy, 1) == 1) return;
            try
            {
                try { BookingService.ExpireHolds(); } catch (Exception ex) { AppLog.Error("ExpireHolds", ex); }
                try { EmailService.ProcessPending(); } catch (Exception ex) { AppLog.Error("EmailWorker", ex); }
            }
            finally { Interlocked.Exchange(ref _busy, 0); }
        }

        public void Stop(bool immediate) { _timer.Dispose(); HostingEnvironment.UnregisterObject(this); }
    }

    public static class EmailWorker
    {
        public static void Poke() { BackgroundWorker.PokeNow(); }
    }

    public static class AppLog
    {
        public static void Error(string where, Exception ex)
        {
            System.Diagnostics.Trace.TraceError("[{0}] {1}: {2}", DateTime.UtcNow.ToString("o"), where, ex.Message);
        }

        public static void Audit(int? accountId, string action, string entity, string entityId, string description, string level = "INFO")
        {
            try
            {
                string ip = null;
                try { ip = System.Web.HttpContext.Current?.Request?.UserHostAddress; } catch { }
                using (var c = TravelDb.Open())
                    c.Execute(@"INSERT INTO NhatKyHeThong (MaTaiKhoan, HanhDong, DoiTuong, DoiTuongId, MoTa, DiaChiIP, MucDo, NgayTao)
                                VALUES (@a,@h,@d,@i,@m,@ip,@l,SYSUTCDATETIME())",
                        new { a = accountId, h = action, d = entity, i = entityId, m = description != null && description.Length > 1000 ? description.Substring(0, 1000) : description, ip, l = level });
            }
            catch (Exception ex) { Error("Audit", ex); }
        }
    }
}
