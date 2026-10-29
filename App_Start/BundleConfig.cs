using System.Web.Optimization;

namespace TravelHub
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            BundleTable.EnableOptimizations = true;   // minify + URL có hash nội dung → cache an toàn và tự làm mới khi sửa file
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include("~/Scripts/jquery-{version}.js"));
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include("~/Scripts/jquery.validate.js", "~/Scripts/jquery.validate.unobtrusive.js"));
            bundles.Add(new Bundle("~/bundles/bootstrap").Include("~/Scripts/bootstrap.bundle.js"));
            bundles.Add(new ScriptBundle("~/bundles/site").Include("~/Scripts/site.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/bootstrap.css", "~/Content/site.css"));
            bundles.Add(new StyleBundle("~/Content/admincss").Include("~/Content/bootstrap.css", "~/Content/site.css", "~/Content/admin.css"));
        }
    }
}
