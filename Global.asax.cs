using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using TravelHub.Infrastructure;
using TravelHub.Services;

namespace TravelHub
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            MvcHandler.DisableMvcResponseHeader = true;
            System.Web.Helpers.AntiForgeryConfig.UniqueClaimTypeIdentifier = System.Security.Claims.ClaimTypes.NameIdentifier;
            try { AdminSeeder.Run(); } catch (Exception ex) { AppLog.Error("AdminSeeder", ex); }
            BackgroundWorker.Start();
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            Auth.Authenticate(Context);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var vi = new CultureInfo("vi-VN");
            Thread.CurrentThread.CurrentCulture = vi;
            Thread.CurrentThread.CurrentUICulture = vi;
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError();
            if (ex == null) return;
            var http = ex as HttpException;
            if (http == null || http.GetHttpCode() >= 500) AppLog.Error("Unhandled " + Request.RawUrl, ex);
        }
    }
}
