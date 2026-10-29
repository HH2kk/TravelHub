using System.Web.Mvc;
using TravelHub.Infrastructure;

namespace TravelHub
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new AutoValidateAntiForgeryFilter());
        }
    }
}
