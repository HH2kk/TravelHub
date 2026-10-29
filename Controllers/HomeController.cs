using System.Linq;
using System.Web.Mvc;
using TravelHub.Data;
using TravelHub.Infrastructure;
using TravelHub.Models;
using TravelHub.Services;

namespace TravelHub.Controllers
{
    public class HomeViewModel
    {
        public System.Collections.Generic.List<DestinationItem> Destinations;
        public System.Collections.Generic.List<HotelCard> Favorites, Popular, Deals;
        public System.Collections.Generic.List<string> Types;
        public System.Collections.Generic.List<ArticleRow> Articles;
        public System.Collections.Generic.List<TestimonialRow> Testimonials;
        public System.Collections.Generic.List<PromotionItem> Promotions;
        public CatalogService.SiteStats Stats;
    }

    public class HomeController : AppController
    {
        public ActionResult Index()
        {
            var repo = new TravelRepository();
            var vm = new HomeViewModel
            {
                Destinations = CatalogService.Destinations(),
                Favorites = CatalogService.Featured("rating", 4),
                Popular = CatalogService.Featured("popular", 4),
                Deals = CatalogService.Featured("deals", 4),
                Types = HotelSearchService.PropertyTypes(),
                Articles = CatalogService.Articles(null, null, 3),
                Testimonials = CatalogService.Testimonials(3),
                Promotions = repo.GetPromotions(),
                Stats = CatalogService.Stats()
            };
            ViewBag.FavIds = CatalogService.FavoriteIds(Me != null ? Me.CustomerId : null);
            ViewBag.Title = "Hành trình của bạn, bắt đầu từ đây";
            return View(vm);
        }

        public ActionResult Policy() { ViewBag.Title = "Điều khoản & chính sách hủy"; return View(); }

        [HttpPost, RateLimit(5, 60)]
        public ActionResult Subscribe(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
            { Err("Email không hợp lệ."); return Redirect(Request.UrlReferrer != null && Url.IsLocalUrl(Request.UrlReferrer.PathAndQuery) ? Request.UrlReferrer.PathAndQuery : "/"); }
            using (var c = TravelDb.Open())
                Dapper.SqlMapper.Execute(c, "IF NOT EXISTS (SELECT 1 FROM DangKyNhanTin WHERE Email=@e) INSERT INTO DangKyNhanTin (Email) VALUES (@e)", new { e = email.Trim() });
            Ok("Cảm ơn bạn! Bạn đã đăng ký nhận ưu đãi từ TravelHub.");
            return RedirectToAction("Index");
        }
    }

    public class ErrorController : AppController
    {
        public ActionResult Index() { Response.StatusCode = 500; Response.TrySkipIisCustomErrors = true; return View("~/Views/Shared/Error.cshtml"); }
        public ActionResult NotFound() { Response.StatusCode = 404; Response.TrySkipIisCustomErrors = true; return View("~/Views/Shared/NotFound.cshtml"); }
        public ActionResult Forbidden() { Response.StatusCode = 403; Response.TrySkipIisCustomErrors = true; return View("~/Views/Shared/Forbidden.cshtml"); }
    }
}
