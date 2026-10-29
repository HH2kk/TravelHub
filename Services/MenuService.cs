using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using TravelHub.Data;
using TravelHub.Models;

namespace TravelHub.Services
{
    public class MenuCount { public string Key { get; set; } public int Count { get; set; } }

    public class MenuDestination { public int Id { get; set; } public string Name { get; set; } public string Region { get; set; } public string Style { get; set; } public string ImageUrl { get; set; } public int HotelCount { get; set; } }

    public class MenuAttraction { public int Id { get; set; } public int DestinationId { get; set; } public string Name { get; set; } public string Category { get; set; } public string DestinationName { get; set; } }

    /// <summary>Dữ liệu mega-menu: toàn bộ lấy từ database, chỉ liệt kê mục có nội dung thật (đếm &gt; 0).</summary>
    public class MegaMenu
    {
        public List<MenuDestination> Destinations = new List<MenuDestination>();
        public List<MenuCount> Styles = new List<MenuCount>();
        public List<MenuCount> Regions = new List<MenuCount>();
        public List<MenuCount> PropertyTypes = new List<MenuCount>();
        public List<MenuCount> StarBuckets = new List<MenuCount>();
        public int WithBreakfast, FreeCancel, TotalHotels;
        public decimal MinPrice, MaxPrice;
        public List<PromotionItem> Promotions = new List<PromotionItem>();
        public List<MenuCount> ArticleCategories = new List<MenuCount>();
        public List<ArticleRow> LatestArticles = new List<ArticleRow>();
        public List<MenuAttraction> Attractions = new List<MenuAttraction>();
    }

    public class UserMenu
    {
        public int Favorites;
        public int UpcomingTrips;
        public int PendingCount;
        public int PendingBookingId;
        public string PendingCode, PendingHotel;
        public DateTime? PendingExpires;
        public int NextTripId;
        public string NextTripHotel;
        public DateTime? NextTripDate;
    }

    public class AdminBadges { public int PendingBookings, Reconcile, RefundsOpen, NewContacts; }

    public static class MenuService
    {
        static readonly object Gate = new object();
        static MegaMenu _cache; static DateTime _cacheUntil = DateTime.MinValue;

        public static void Invalidate() { lock (Gate) { _cache = null; _cacheUntil = DateTime.MinValue; } }

        public static MegaMenu Get()
        {
            lock (Gate) { if (_cache != null && _cacheUntil > DateTime.UtcNow) return _cache; }
            var m = Build();
            lock (Gate) { _cache = m; _cacheUntil = DateTime.UtcNow.AddMinutes(2); }
            return m;
        }

        static MegaMenu Build()
        {
            var m = new MegaMenu();
            try
            {
                using (var c = TravelDb.Open())
                {
                    m.Destinations = c.Query<MenuDestination>(@"
SELECT d.MaDiaDiem Id, d.TenDiaDiem Name, ISNULL(d.Vung,N'') Region, ISNULL(d.PhongCach,N'') Style, d.HinhAnh ImageUrl,
       (SELECT COUNT(*) FROM KhachSan k WHERE k.MaDiaDiem=d.MaDiaDiem AND k.TrangThai=1) HotelCount
FROM DiaDiem d WHERE d.TrangThai=1 ORDER BY (SELECT COUNT(*) FROM KhachSan k WHERE k.MaDiaDiem=d.MaDiaDiem AND k.TrangThai=1) DESC, d.TenDiaDiem").ToList();
                    m.Styles = m.Destinations.Where(d => d.Style != "").GroupBy(d => d.Style).Select(g => new MenuCount { Key = g.Key, Count = g.Count() }).OrderByDescending(x => x.Count).ToList();
                    m.Regions = new[] { "Bắc", "Trung", "Nam" }.Select(r => new MenuCount { Key = r, Count = m.Destinations.Count(d => d.Region == r) }).Where(x => x.Count > 0).ToList();
                    m.PropertyTypes = c.Query<MenuCount>("SELECT ISNULL(LoaiHinh,N'Khách sạn') [Key], COUNT(*) [Count] FROM KhachSan WHERE TrangThai=1 GROUP BY ISNULL(LoaiHinh,N'Khách sạn') ORDER BY COUNT(*) DESC").ToList();
                    m.StarBuckets = new[] { 5, 4, 3 }.Select(s => new MenuCount { Key = s.ToString(), Count = c.ExecuteScalar<int>("SELECT COUNT(*) FROM KhachSan WHERE TrangThai=1 AND SoSao>=@s", new { s }) }).Where(x => x.Count > 0).ToList();
                    var agg = c.QueryFirst(@"SELECT COUNT(*) T, SUM(CASE WHEN BuaSang=1 THEN 1 ELSE 0 END) B, SUM(CASE WHEN ChinhSachHuyLoai<>'NONREFUND' THEN 1 ELSE 0 END) F FROM KhachSan WHERE TrangThai=1");
                    m.TotalHotels = (int)agg.T; m.WithBreakfast = (int)(agg.B ?? 0); m.FreeCancel = (int)(agg.F ?? 0);
                    var pr = c.QueryFirst("SELECT ISNULL(MIN(GiaPhong),0) Lo, ISNULL(MAX(GiaPhong),0) Hi FROM Phong p JOIN KhachSan k ON k.MaKS=p.MaKS WHERE p.TrangThai=1 AND k.TrangThai=1");
                    m.MinPrice = (decimal)pr.Lo; m.MaxPrice = (decimal)pr.Hi;
                    m.Promotions = c.Query<PromotionItem>(@"SELECT TOP 3 MaKM Id, TenKM Name, MaCode Code, MoTa Description, PhanTramGiam PercentOff, SoTienGiam AmountOff FROM KhuyenMai
                        WHERE TrangThai=1 AND (NgayBatDau IS NULL OR NgayBatDau<=GETDATE()) AND (NgayKetThuc IS NULL OR NgayKetThuc>=GETDATE()) AND (SoLuong IS NULL OR SoLuongDaDung < SoLuong) AND MaKS IS NULL
                        ORDER BY ISNULL(NgayKetThuc,'9999-12-31')").ToList();
                    m.ArticleCategories = c.Query<MenuCount>("SELECT ISNULL(DanhMuc,N'Kinh nghiệm') [Key], COUNT(*) [Count] FROM BaiViet WHERE TrangThai=1 GROUP BY ISNULL(DanhMuc,N'Kinh nghiệm') ORDER BY COUNT(*) DESC").ToList();
                    m.LatestArticles = c.Query<ArticleRow>("SELECT TOP 3 MaBaiViet Id, TieuDe Title, HinhAnh ImageUrl, ISNULL(DanhMuc,N'Kinh nghiệm') Category FROM BaiViet WHERE TrangThai=1 ORDER BY NgayDang DESC").ToList();
                    m.Attractions = c.Query<MenuAttraction>(@"SELECT TOP 5 a.MaDDDL Id, a.MaDiaDiem DestinationId, a.TenDiaDiem Name, a.LoaiDiaDiem Category, d.TenDiaDiem DestinationName
                        FROM DiaDiemDuLich a JOIN DiaDiem d ON d.MaDiaDiem=a.MaDiaDiem WHERE a.TrangThai=1 AND d.TrangThai=1 ORDER BY a.NoiBat DESC, a.MaDDDL DESC").ToList();
                }
            }
            catch (Exception ex) { AppLog.Error("MenuService.Build", ex); }   // menu lỗi không được làm hỏng trang
            return m;
        }

        /// <summary>Trạng thái cá nhân cho header: yêu thích, đơn chờ thanh toán (còn hạn giữ phòng), chuyến đi sắp tới.</summary>
        public static UserMenu ForUser(int customerId)
        {
            var u = new UserMenu();
            try
            {
                using (var c = TravelDb.Open())
                {
                    u.Favorites = c.ExecuteScalar<int>("SELECT COUNT(*) FROM YeuThich yt JOIN KhachSan k ON k.MaKS=yt.MaKS AND k.TrangThai=1 WHERE yt.MaKH=@k", new { k = customerId });
                    var pend = c.Query(@"SELECT o.MaDon, o.MaDonCode, o.NgayHetHan, k.TenKS FROM DonDatPhong o JOIN KhachSan k ON k.MaKS=o.MaKS
                        WHERE o.MaKH=@k AND o.TrangThai IN (N'Chờ thanh toán') AND (o.NgayHetHan IS NULL OR o.NgayHetHan > GETDATE()) ORDER BY o.NgayHetHan", new { k = customerId }).ToList();
                    u.PendingCount = pend.Count;
                    if (pend.Count > 0) { var p = pend[0]; u.PendingBookingId = p.MaDon; u.PendingCode = p.MaDonCode; u.PendingExpires = p.NgayHetHan; u.PendingHotel = p.TenKS; }
                    var trips = c.Query(@"SELECT o.MaDon, k.TenKS, o.NgayNhanPhong FROM DonDatPhong o JOIN KhachSan k ON k.MaKS=o.MaKS
                        WHERE o.MaKH=@k AND o.TrangThai IN (N'Đã thanh toán', N'Đã nhận phòng') AND o.NgayTraPhong >= CAST(GETDATE() AS date) ORDER BY o.NgayNhanPhong", new { k = customerId }).ToList();
                    u.UpcomingTrips = trips.Count;
                    if (trips.Count > 0) { var t = trips[0]; u.NextTripId = t.MaDon; u.NextTripHotel = t.TenKS; u.NextTripDate = t.NgayNhanPhong; }
                }
            }
            catch (Exception ex) { AppLog.Error("MenuService.ForUser", ex); }
            return u;
        }

        public static AdminBadges Admin()
        {
            var b = new AdminBadges();
            try
            {
                using (var c = TravelDb.Open())
                {
                    b.PendingBookings = c.ExecuteScalar<int>("SELECT COUNT(*) FROM DonDatPhong WHERE TrangThai IN (N'Chờ thanh toán', N'Chờ xác nhận') AND (NgayHetHan IS NULL OR NgayHetHan > GETDATE())");
                    b.Reconcile = c.ExecuteScalar<int>("SELECT COUNT(*) FROM ThanhToan WHERE TrangThai=N'Cần đối soát' AND TrangThaiCode<>'SUPERSEDED'");
                    b.RefundsOpen = c.ExecuteScalar<int>("SELECT COUNT(*) FROM YeuCauHoanTien WHERE TrangThai IN ('PENDING','APPROVED','MANUAL_REQUIRED')");
                    b.NewContacts = c.ExecuteScalar<int>("SELECT COUNT(*) FROM LienHe WHERE TrangThai='NEW'");
                }
            }
            catch (Exception ex) { AppLog.Error("MenuService.Admin", ex); }
            return b;
        }
    }
}
