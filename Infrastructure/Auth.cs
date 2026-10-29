using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using TravelHub.Models;
using TravelHub.Services;

namespace TravelHub.Infrastructure
{
    /// <summary>Principal chứa phiên đăng nhập. Role lấy từ ticket do SERVER ký (không bao giờ từ client).</summary>
    public class TravelPrincipal : IPrincipal
    {
        public TravelPrincipal(IIdentity identity, UserSession session) { Identity = identity; Session = session; }
        public IIdentity Identity { get; private set; }
        public UserSession Session { get; private set; }
        public bool IsInRole(string role) { return Session != null && string.Equals(Session.Role, role, StringComparison.OrdinalIgnoreCase); }
    }

    public static class Auth
    {
        public static void SignIn(HttpResponseBase response, UserSession s, bool persistent)
        {
            var data = string.Join("|", s.AccountId, s.CustomerId, s.Role, s.EmailConfirmed ? "1" : "0", Uri.EscapeDataString(s.FullName ?? ""), Uri.EscapeDataString(s.Email ?? ""));
            var ticket = new FormsAuthenticationTicket(2, s.UserName, DateTime.Now, DateTime.Now.AddMinutes(persistent ? 60 * 24 * 7 : 60), persistent, data);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket)) { HttpOnly = true, Secure = FormsAuthentication.RequireSSL };
            if (persistent) cookie.Expires = ticket.Expiration;
            response.Cookies.Add(cookie);
        }

        public static void SignOut() { FormsAuthentication.SignOut(); }

        /// <summary>Gọi trong Application_AuthenticateRequest.</summary>
        public static void Authenticate(HttpContext ctx)
        {
            var cookie = ctx.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (cookie == null || string.IsNullOrEmpty(cookie.Value)) return;
            try
            {
                var t = FormsAuthentication.Decrypt(cookie.Value);
                if (t == null || t.Expired) return;
                var p = t.UserData.Split('|');
                var s = new UserSession
                {
                    AccountId = int.Parse(p[0]), CustomerId = string.IsNullOrEmpty(p[1]) ? (int?)null : int.Parse(p[1]), Role = p[2],
                    EmailConfirmed = p[3] == "1", FullName = Uri.UnescapeDataString(p[4]), Email = Uri.UnescapeDataString(p[5]), UserName = t.Name
                };
                ctx.User = new TravelPrincipal(new FormsIdentity(t), s);
            }
            catch { /* ticket hỏng → coi như chưa đăng nhập */ }
        }

        public static UserSession Current
        {
            get
            {
                var p = HttpContext.Current != null ? HttpContext.Current.User as TravelPrincipal : null;
                return p == null ? null : p.Session;
            }
        }
    }

    public class AppController : Controller
    {
        protected UserSession Me { get { return Auth.Current; } }
        protected bool IsStaff { get { return Me != null && AppRoles.AdminArea.Split(',').Contains(Me.Role); } }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.Me = Me;
            if (Me != null && Me.CustomerId.HasValue && filterContext.HttpContext.Request.HttpMethod == "GET" && !filterContext.HttpContext.Request.IsAjaxRequest())
                ViewBag.UserMenu = TravelHub.Services.MenuService.ForUser(Me.CustomerId.Value);
            base.OnActionExecuting(filterContext);
        }

        protected void Ok(string msg) { TempData["ok"] = msg; }
        protected void Err(string msg) { TempData["err"] = msg; }

        protected ActionResult SafeRedirect(string url, string fallbackAction = "Index", string fallbackController = "Home")
        {
            if (!string.IsNullOrEmpty(url) && Url.IsLocalUrl(url)) return Redirect(url);
            return RedirectToAction(fallbackAction, fallbackController);
        }
    }

    /// <summary>MVC5 không có AutoValidateAntiforgery: mọi POST/PUT/DELETE phải kèm token (trừ action có [IgnoreAntiForgery], vd IPN).</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class IgnoreAntiForgeryAttribute : Attribute { }

    public sealed class AutoValidateAntiForgeryFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext ctx)
        {
            var m = ctx.HttpContext.Request.HttpMethod;
            if (m == "GET" || m == "HEAD" || m == "OPTIONS" || m == "TRACE") return;
            var ad = ctx.ActionDescriptor;
            if (ad.IsDefined(typeof(IgnoreAntiForgeryAttribute), true) || ad.ControllerDescriptor.IsDefined(typeof(IgnoreAntiForgeryAttribute), true)) return;
            try { System.Web.Helpers.AntiForgery.Validate(); }
            catch (System.Web.Mvc.HttpAntiForgeryException) { ctx.Result = new HttpStatusCodeResult(400, "Phiên làm việc không hợp lệ hoặc đã hết hạn. Vui lòng tải lại trang."); }
        }
    }

    /// <summary>Chỉ cho phép role chỉ định; chưa đăng nhập → trang đăng nhập; sai quyền → 403.</summary>
    public sealed class RoleAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext ctx)
        {
            if (ctx.HttpContext.User != null && ctx.HttpContext.User.Identity.IsAuthenticated)
            {
                ctx.Result = new ViewResult { ViewName = "~/Views/Shared/Forbidden.cshtml" };
                ctx.HttpContext.Response.StatusCode = 403;
                ctx.HttpContext.Response.TrySkipIisCustomErrors = true;
            }
            else base.HandleUnauthorizedRequest(ctx);
        }
    }

    /// <summary>Đã đăng nhập VÀ đã xác thực email (bắt buộc cho đặt phòng/thanh toán).</summary>
    public sealed class RequireVerifiedEmailAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext ctx)
        {
            var me = Auth.Current;
            if (me == null) { ctx.Result = new RedirectResult("/Account/Login?returnUrl=" + HttpUtility.UrlEncode(ctx.HttpContext.Request.RawUrl)); return; }
            if (!me.EmailConfirmed)
            {
                // Trạng thái trong cookie có thể cũ: kiểm tra lại DB trước khi chặn.
                if (!AccountStore.IsEmailConfirmed(me.AccountId))
                {
                    ctx.Controller.TempData["err"] = "Vui lòng xác minh email trước khi đặt phòng hoặc thanh toán.";
                    ctx.Result = new RedirectResult("/Account/VerifyRequired");
                    return;
                }
            }
            if (!me.CustomerId.HasValue) { ctx.Result = new HttpStatusCodeResult(403); }
        }
    }

    /// <summary>Giới hạn số request theo IP + action (chống brute-force/spam). Bộ nhớ trong tiến trình.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RateLimitAttribute : ActionFilterAttribute
    {
        static readonly ConcurrentDictionary<string, Tuple<DateTime, int>> Hits = new ConcurrentDictionary<string, Tuple<DateTime, int>>();
        readonly int _max, _seconds;
        public RateLimitAttribute(int max, int seconds) { _max = max; _seconds = seconds; }

        public override void OnActionExecuting(ActionExecutingContext ctx)
        {
            if (ctx.HttpContext.Request.HttpMethod != "POST") return;
            var key = ctx.HttpContext.Request.UserHostAddress + "|" + ctx.ActionDescriptor.ControllerDescriptor.ControllerName + "/" + ctx.ActionDescriptor.ActionName;
            var now = DateTime.UtcNow;
            var e = Hits.AddOrUpdate(key, _ => Tuple.Create(now, 1), (_, old) => (now - old.Item1).TotalSeconds > _seconds ? Tuple.Create(now, 1) : Tuple.Create(old.Item1, old.Item2 + 1));
            if (e.Item2 > _max)
            {
                ctx.Result = new HttpStatusCodeResult(429, "Quá nhiều yêu cầu. Vui lòng thử lại sau.");
            }
        }
    }

    public static class AccountStore
    {
        public static bool IsEmailConfirmed(int accountId)
        {
            using (var c = TravelHub.Data.TravelDb.Open())
                return Dapper.SqlMapper.ExecuteScalar<bool>(c, "SELECT EmailXacThuc FROM TaiKhoan WHERE MaTaiKhoan=@id AND TrangThai=1", new { id = accountId });
        }
    }
}
