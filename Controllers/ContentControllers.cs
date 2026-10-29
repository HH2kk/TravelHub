using System.Linq;
using System.Web.Mvc;
using TravelHub.Data;
using TravelHub.Infrastructure;
using TravelHub.Models;
using TravelHub.Services;

namespace TravelHub.Controllers
{
    public class DestinationsController : AppController
    {
        public ActionResult Index(string style, string region)
        {
            ViewBag.Title = "Điểm đến";
            var all = CatalogService.Destinations();
            ViewBag.Styles = all.Where(d => !string.IsNullOrEmpty(d.TravelStyle)).GroupBy(d => d.TravelStyle).Select(g => new { Key = g.Key, Count = g.Count() }).ToDictionary(x => x.Key, x => x.Count);
            ViewBag.Regions = all.Where(d => !string.IsNullOrEmpty(d.Region)).GroupBy(d => d.Region).ToDictionary(g => g.Key, g => g.Count());
            ViewBag.Style = style; ViewBag.Region = region;
            var list = all.Where(d => (string.IsNullOrEmpty(style) || d.TravelStyle == style) && (string.IsNullOrEmpty(region) || d.Region == region)).ToList();
            return View(list);
        }

        public ActionResult Details(int id)
        {
            var d = CatalogService.Destinations().FirstOrDefault(x => x.Id == id);
            if (d == null) return HttpNotFound();
            ViewBag.Hotels = CatalogService.HotelsInDestination(id);
            ViewBag.Attractions = new TravelRepository().GetAttractions(id);
            ViewBag.Articles = CatalogService.Articles(null, null, 50).Where(a => a.DestinationId == id).ToList();
            ViewBag.FavIds = CatalogService.FavoriteIds(Me != null ? Me.CustomerId : null);
            ViewBag.Title = d.Name;
            return View(d);
        }
    }

    public class ArticlesController : AppController
    {
        public ActionResult Index(string q, string category)
        {
            ViewBag.Title = "Cẩm nang du lịch";
            ViewBag.Q = q; ViewBag.Category = category; ViewBag.Categories = CatalogService.ArticleCategories();
            return View(CatalogService.Articles(q, category));
        }

        public ActionResult Details(int id)
        {
            var a = CatalogService.Article(id, true);
            if (a == null) return HttpNotFound();
            ViewBag.Title = a.SeoTitle ?? a.Title; ViewBag.Description = a.SeoDescription ?? a.Summary;
            if (a.DestinationId.HasValue) ViewBag.Hotels = CatalogService.HotelsInDestination(a.DestinationId.Value).Take(3).ToList();
            return View(a);
        }
    }

    public class FavoritesController : AppController
    {
        [RoleAuthorize]
        public ActionResult Index()
        {
            ViewBag.Title = "Chỗ nghỉ yêu thích";
            ViewBag.FavIds = CatalogService.FavoriteIds(Me.CustomerId);
            return View(new TravelRepository().GetFavoriteHotels(Me.CustomerId ?? 0));
        }

        [HttpPost]
        public ActionResult Toggle(int id)
        {
            if (Me == null || !Me.CustomerId.HasValue) { Response.StatusCode = 401; return Json(new { error = "login" }); }
            var repo = new TravelRepository();
            bool on = repo.IsHotelFavorite(Me.CustomerId.Value, id);
            try
            {
                if (on) repo.RemoveFavoriteHotel(Me.CustomerId.Value, id); else repo.AddFavoriteHotel(Me.CustomerId.Value, id);
            }
            catch (System.InvalidOperationException) { Response.StatusCode = 404; return Json(new { error = "not-found" }); }
            return Json(new { on = !on });
        }
    }

    public class ContactController : AppController
    {
        public ActionResult Index() { ViewBag.Title = "Liên hệ & hỗ trợ"; return View(new ContactViewModel { Name = Me?.FullName, Email = Me?.Email }); }

        [HttpPost, RateLimit(5, 300)]
        public ActionResult Index(ContactViewModel m)
        {
            ViewBag.Title = "Liên hệ & hỗ trợ";
            if (!ModelState.IsValid) return View(m);
            using (var c = TravelDb.Open())
                Dapper.SqlMapper.Execute(c, "INSERT INTO LienHe (HoTen, Email, TieuDe, NoiDung) VALUES (@Name,@Email,@Subject,@Message)", m);
            var admin = AppConfig.Setting("EmailQuanTri", "");
            if (!string.IsNullOrWhiteSpace(admin))
                EmailService.Enqueue("SUPPORT", null, admin, "[TravelHub] Yêu cầu hỗ trợ: " + m.Subject,
                    "<p><b>" + System.Web.HttpUtility.HtmlEncode(m.Name) + "</b> (" + System.Web.HttpUtility.HtmlEncode(m.Email) + ")</p><p>" + System.Web.HttpUtility.HtmlEncode(m.Message).Replace("\n", "<br>") + "</p>");
            Ok("Cảm ơn bạn! Chúng tôi đã nhận yêu cầu và sẽ phản hồi qua email sớm nhất.");
            return RedirectToAction("Index");
        }
    }

    public class ContactViewModel
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập họ tên"), System.ComponentModel.DataAnnotations.StringLength(100)] public string Name { get; set; }
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập email"), System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "Email không hợp lệ")] public string Email { get; set; }
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập tiêu đề"), System.ComponentModel.DataAnnotations.StringLength(200)] public string Subject { get; set; }
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập nội dung"), System.ComponentModel.DataAnnotations.StringLength(2000)] public string Message { get; set; }
    }
}
