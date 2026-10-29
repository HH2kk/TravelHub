using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TravelHub.Data;
using TravelHub.Infrastructure;
using TravelHub.Models;
using TravelHub.Services;

namespace TravelHub.Controllers
{
    public class HotelListViewModel
    {
        public SearchCriteria Criteria;
        public SearchResult Result;
        public List<DestinationItem> Destinations;
        public List<AmenityOption> Amenities;
        public List<string> Types;
        public string Error;
    }

    public class HotelDetailViewModel
    {
        public HotelDetail Hotel;
        public List<RoomItem> Rooms;
        public List<ReviewItem> Reviews;
        public List<AttractionItem> Attractions;
        public SearchCriteria Stay;
        public bool IsFavorite;
        public string Error;
        public string StayQuery;
    }

    public class HotelsController : AppController
    {
        static string Stay(SearchCriteria s)
        {
            if (!s.HasDates) return "";
            return "?checkIn=" + s.CheckIn.Value.ToString("yyyy-MM-dd") + "&checkOut=" + s.CheckOut.Value.ToString("yyyy-MM-dd") + "&adults=" + s.Adults + "&children=" + s.Children + "&rooms=" + s.Rooms;
        }

        // /Hotels?destinationId=1&q=...&checkIn=...&checkOut=...&adults=2&children=0&rooms=1&minPrice=&maxPrice=&stars=&minRating=&types=...&amenities=...&breakfast=true&freeCancel=true&sort=&page=
        public ActionResult Index(SearchCriteria c, string[] types, int[] amenities, int? stars)
        {
            if (stars.HasValue && stars >= 1 && stars <= 5) c.MinStars = stars;
            c.Types = (types ?? new string[0]).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
            c.Amenities = (amenities ?? new int[0]).ToList();
            var vm = new HotelListViewModel { Criteria = c };
            if (c.CheckIn.HasValue != c.CheckOut.HasValue) { vm.Error = "Vui lòng chọn cả ngày nhận phòng và ngày trả phòng."; c.CheckIn = c.CheckOut = null; }
            else if (c.CheckIn.HasValue)
            {
                try { BookingService.ValidateStay(c.CheckIn.Value, c.CheckOut.Value); }
                catch (BookingException ex) { vm.Error = ex.Message; c.CheckIn = c.CheckOut = null; }
            }
            if (c.Adults < 1) c.Adults = 1; if (c.Adults > 20) c.Adults = 20; if (c.Children < 0) c.Children = 0; if (c.Rooms < 1) c.Rooms = 1; if (c.Rooms > 10) c.Rooms = 10;
            vm.Result = HotelSearchService.Search(c);
            vm.Destinations = CatalogService.Destinations();
            vm.Amenities = HotelSearchService.Amenities();
            vm.Types = HotelSearchService.PropertyTypes();
            ViewBag.FavIds = CatalogService.FavoriteIds(Me != null ? Me.CustomerId : null);
            ViewBag.StayQuery = Stay(c);
            ViewBag.Title = "Tìm chỗ nghỉ";
            return View(vm);
        }

        public ActionResult Details(int id, DateTime? checkIn, DateTime? checkOut, int adults = 2, int children = 0, int rooms = 1)
        {
            var hotel = CatalogService.GetHotel(id);
            if (hotel == null) return HttpNotFound();
            var stay = new SearchCriteria { CheckIn = checkIn, CheckOut = checkOut, Adults = Math.Max(1, Math.Min(adults, 20)), Children = Math.Max(0, children), Rooms = Math.Max(1, Math.Min(rooms, 10)) };
            var vm = new HotelDetailViewModel { Hotel = hotel, Stay = stay };
            if (checkIn.HasValue || checkOut.HasValue)
            {
                try
                {
                    if (!checkIn.HasValue || !checkOut.HasValue) throw new BookingException("Vui lòng chọn cả ngày nhận và trả phòng.");
                    BookingService.ValidateStay(checkIn.Value, checkOut.Value);
                }
                catch (BookingException ex) { vm.Error = ex.Message; stay.CheckIn = stay.CheckOut = null; }
            }
            vm.Rooms = CatalogService.GetRooms(id, stay.CheckIn, stay.CheckOut, stay.Guests, stay.Rooms);
            vm.Reviews = CatalogService.Reviews(id);
            vm.Attractions = new TravelRepository().GetAttractions(hotel.DestinationId);
            vm.IsFavorite = Me != null && Me.CustomerId.HasValue && CatalogService.FavoriteIds(Me.CustomerId).Contains(id);
            vm.StayQuery = Stay(stay);
            ViewBag.Title = hotel.Name;
            ViewBag.Description = (hotel.Description ?? "").Length > 150 ? hotel.Description.Substring(0, 150) : hotel.Description;
            return View(vm);
        }

        // Gợi ý điểm đến / chỗ nghỉ cho ô tìm kiếm (JSON, chỉ dữ liệu công khai)
        [HttpGet, OutputCache(Duration = 30, VaryByParam = "q")]
        public ActionResult Suggest(string q)
        {
            q = (q ?? "").Trim(); if (q.Length > 60) q = q.Substring(0, 60);
            var like = q.Length == 0 ? null : "%" + q.Replace('Đ', 'D').Replace('đ', 'd').Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%";
            using (var c = TravelDb.Open())
            {
                var d = Dapper.SqlMapper.Query(c, @"SELECT TOP 6 MaDiaDiem Id, TenDiaDiem Name, ISNULL(PhongCach,N'') Sub FROM DiaDiem WHERE TrangThai=1 AND (@l IS NULL OR REPLACE(REPLACE(TenDiaDiem,N'Đ',N'D'),N'đ',N'd') LIKE @l COLLATE Vietnamese_100_CI_AI OR REPLACE(REPLACE(ThanhPho,N'Đ',N'D'),N'đ',N'd') LIKE @l COLLATE Vietnamese_100_CI_AI) ORDER BY MaDiaDiem", new { l = like })
                    .Select(x => new { type = "dest", id = (int)x.Id, name = (string)x.Name, sub = "Điểm đến" + (string.IsNullOrEmpty((string)x.Sub) ? "" : " · " + (string)x.Sub) });
                var h = Dapper.SqlMapper.Query(c, @"SELECT TOP 5 ks.MaKS Id, ks.TenKS Name, dd.TenDiaDiem Dest FROM KhachSan ks JOIN DiaDiem dd ON dd.MaDiaDiem=ks.MaDiaDiem WHERE ks.TrangThai=1 AND (@l IS NULL OR REPLACE(REPLACE(ks.TenKS,N'Đ',N'D'),N'đ',N'd') LIKE @l COLLATE Vietnamese_100_CI_AI OR REPLACE(REPLACE(ks.DiaChi,N'Đ',N'D'),N'đ',N'd') LIKE @l COLLATE Vietnamese_100_CI_AI) ORDER BY ks.TenKS", new { l = like })
                    .Select(x => new { type = "hotel", id = (int)x.Id, name = (string)x.Name, sub = "Chỗ nghỉ · " + (string)x.Dest });
                return Json(d.Cast<object>().Concat(q.Length == 0 ? h.Take(2).Cast<object>() : h.Cast<object>()).ToList(), JsonRequestBehavior.AllowGet);
            }
        }

        // So sánh tối đa 4 chỗ nghỉ: /Hotels/Compare?ids=1&ids=2
        public ActionResult Compare(int[] ids)
        {
            var list = (ids ?? new int[0]).Distinct().Take(4).Select(CatalogService.GetHotel).Where(h => h != null).ToList();
            ViewBag.Title = "So sánh chỗ nghỉ";
            return View(list);
        }
    }
}
