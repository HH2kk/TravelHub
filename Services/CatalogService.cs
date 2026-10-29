using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using TravelHub.Data;
using TravelHub.Models;

namespace TravelHub.Services
{
    public class HotelDetail : HotelCard
    {
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Policy { get; set; }
        public string CancelPolicyCode { get; set; }
        public string CancelPolicyText { get; set; }
        public string CheckInTime { get; set; }
        public string CheckOutTime { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }
        public List<HinhAnhKhachSan> Images { get; set; } = new List<HinhAnhKhachSan>();
        public List<AmenityOption> AmenityList { get; set; } = new List<AmenityOption>();
    }

    public class ArticleRow
    {
        public int Id { get; set; }
        public int? DestinationId { get; set; }
        public string DestinationName { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public string Author { get; set; }
        public string Category { get; set; }
        public DateTime PostedAt { get; set; }
        public int Views { get; set; }
        public string SeoTitle { get; set; }
        public string SeoDescription { get; set; }
    }

    public class TestimonialRow { public string Name { get; set; } public int Rating { get; set; } public string Content { get; set; } public string HotelName { get; set; } }

    public static class CatalogService
    {
        public static HotelDetail GetHotel(int id)
        {
            using (var c = TravelDb.Open())
            {
                var h = c.QueryFirstOrDefault<HotelDetail>(@"
SELECT ks.MaKS Id, ks.TenKS Name, ks.DiaChi Address, ks.MaDiaDiem DestinationId, dd.TenDiaDiem DestinationName, ks.SoSao Stars,
       ISNULL((SELECT MIN(GiaPhong) FROM Phong WHERE MaKS=ks.MaKS AND TrangThai=1), ks.GiaTu) PriceFrom, ks.HinhAnhDaiDien CoverImageUrl, ks.MoTa Description,
       CAST(ISNULL(rv.Rating,0) AS float) Rating, ISNULL(rv.Cnt,0) ReviewCount, ISNULL(ks.LoaiHinh,N'Khách sạn') PropertyType, ks.BuaSang BreakfastIncluded,
       ks.SoDienThoai Phone, ks.Email Email, ks.ChinhSach Policy, ks.ChinhSachHuyLoai CancelPolicyCode, ks.GioNhanPhong CheckInTime, ks.GioTraPhong CheckOutTime,
       ks.ViDo Lat, ks.KinhDo Lng
FROM KhachSan ks INNER JOIN DiaDiem dd ON dd.MaDiaDiem=ks.MaDiaDiem
OUTER APPLY (SELECT AVG(CAST(g.SoSao AS float)) Rating, COUNT(*) Cnt FROM DanhGia g WHERE g.MaKS=ks.MaKS AND g.TrangThai=1) rv
WHERE ks.MaKS=@id AND ks.TrangThai=1", new { id });
                if (h == null) return null;
                h.CancelPolicyText = CancellationPolicy.Describe(h.CancelPolicyCode);
                h.CancelPolicy = CancellationPolicy.ShortLabel(h.CancelPolicyCode);
                h.Images = c.Query<HinhAnhKhachSan>("SELECT MaHinhAnh, MaKS, DuongDanAnh, MoTa, LaAnhDaiDien, Loai, ThuTu, HienThi FROM HinhAnhKhachSan WHERE MaKS=@id AND HienThi=1 ORDER BY LaAnhDaiDien DESC, ThuTu, MaHinhAnh", new { id }).ToList();
                if (h.Images.Count == 0 && !string.IsNullOrEmpty(h.CoverImageUrl))
                    h.Images.Add(new HinhAnhKhachSan { MaKS = id, DuongDanAnh = h.CoverImageUrl, MoTa = h.Name, LaAnhDaiDien = true });
                h.AmenityList = c.Query<AmenityOption>("SELECT t.MaTienIch Id, t.TenTienIch Name FROM KhachSan_TienIch kt INNER JOIN TienIch t ON t.MaTienIch=kt.MaTienIch WHERE kt.MaKS=@id AND t.TrangThai=1", new { id }).ToList();
                return h;
            }
        }

        /// <summary>Phòng + tình trạng còn trống theo khoảng ngày (nếu có) + bộ ảnh riêng của từng phòng.</summary>
        public static List<RoomItem> GetRooms(int hotelId, DateTime? checkIn, DateTime? checkOut, int guests, int rooms)
        {
            using (var c = TravelDb.Open())
            {
                bool dates = checkIn.HasValue && checkOut.HasValue && checkOut.Value.Date > checkIn.Value.Date;
                var list = c.Query<RoomItem>(@"
SELECT p.MaPhong Id, p.MaKS HotelId, p.TenPhong Name, p.LoaiPhong RoomType, p.SoNguoiToiDa MaxGuests, p.SoGiuong BedCount, p.GiaPhong Price,
       p.SoLuong TotalInventory, p.MoTa Description, p.HinhAnh ImageUrl,
       CASE WHEN @dates=1 THEN p.SoLuong - dbo.fn_PhongConLai(p.MaPhong,@ci,@co) ELSE 0 END Occupied
FROM Phong p WHERE p.MaKS=@h AND p.TrangThai=1 ORDER BY p.GiaPhong",
                    new { h = hotelId, dates, ci = checkIn ?? DateTime.Today, co = checkOut ?? DateTime.Today.AddDays(1) }).ToList();
                var ids = list.Select(r => r.Id).ToList();
                if (ids.Count > 0)
                {
                    var imgs = c.Query<RoomImageItem>(@"SELECT MaHinhAnhPhong Id, MaPhong RoomId, DuongDanAnh Url, MoTa Caption, Loai Kind, ThuTu [Order], LaAnhDaiDien IsCover, HienThi Visible
                                                        FROM HinhAnhPhong WHERE MaPhong IN @ids AND HienThi=1 ORDER BY LaAnhDaiDien DESC, ThuTu, MaHinhAnhPhong", new { ids }).ToList();
                    foreach (var r in list) r.Images = imgs.Where(i => i.RoomId == r.Id).ToList();
                }
                // Phòng chỉ có ảnh chính trong bảng Phong → dùng làm ảnh duy nhất.
                foreach (var r in list)
                    if (r.Images.Count == 0 && !string.IsNullOrEmpty(r.ImageUrl))
                        r.Images.Add(new RoomImageItem { RoomId = r.Id, Url = r.ImageUrl, Caption = r.Name, Kind = "Phòng ngủ", IsCover = true, Visible = true });
                return list;
            }
        }

        public static RoomItem GetRoom(int roomId, DateTime? ci, DateTime? co)
        {
            using (var c = TravelDb.Open())
            {
                var hid = c.ExecuteScalar<int?>("SELECT MaKS FROM Phong WHERE MaPhong=@r AND TrangThai=1", new { r = roomId });
                return hid == null ? null : GetRooms(hid.Value, ci, co, 1, 1).FirstOrDefault(x => x.Id == roomId);
            }
        }

        public static List<HotelCard> Featured(string mode, int take)
        {
            string order;
            switch (mode)
            {
                case "deals": order = "ISNULL((SELECT MIN(GiaPhong) FROM Phong WHERE MaKS=ks.MaKS AND TrangThai=1), ks.GiaTu) ASC"; break;
                case "popular": order = "ISNULL(rv.Cnt,0) DESC, ISNULL(rv.Rating,0) DESC"; break;
                default: order = "ISNULL(rv.Rating,0) DESC, ISNULL(rv.Cnt,0) DESC"; break;
            }
            using (var c = TravelDb.Open())
                return c.Query<HotelCard>(@"
SELECT TOP (@take) ks.MaKS Id, ks.TenKS Name, ks.DiaChi Address, ks.MaDiaDiem DestinationId, dd.TenDiaDiem DestinationName, ks.SoSao Stars,
       ISNULL((SELECT MIN(GiaPhong) FROM Phong WHERE MaKS=ks.MaKS AND TrangThai=1), ks.GiaTu) PriceFrom, ks.HinhAnhDaiDien CoverImageUrl, ks.MoTa Description,
       CAST(ISNULL(rv.Rating,0) AS float) Rating, ISNULL(rv.Cnt,0) ReviewCount, ISNULL(ks.LoaiHinh,N'Khách sạn') PropertyType, ks.BuaSang BreakfastIncluded, ks.ChinhSachHuyLoai CancelPolicy
FROM KhachSan ks INNER JOIN DiaDiem dd ON dd.MaDiaDiem=ks.MaDiaDiem
OUTER APPLY (SELECT AVG(CAST(g.SoSao AS float)) Rating, COUNT(*) Cnt FROM DanhGia g WHERE g.MaKS=ks.MaKS AND g.TrangThai=1) rv
WHERE ks.TrangThai=1 ORDER BY " + order, new { take }).ToList();
        }

        public static List<TestimonialRow> Testimonials(int take)
        {
            using (var c = TravelDb.Open())
                return c.Query<TestimonialRow>(@"SELECT TOP (@take) kh.HoTen Name, g.SoSao Rating, g.NoiDung Content, ks.TenKS HotelName
                    FROM DanhGia g INNER JOIN KhachHang kh ON kh.MaKH=g.MaKH INNER JOIN KhachSan ks ON ks.MaKS=g.MaKS
                    WHERE g.TrangThai=1 AND g.NoiDung IS NOT NULL AND LEN(g.NoiDung) > 15 ORDER BY g.SoSao DESC, g.NgayDanhGia DESC", new { take }).ToList();
        }

        public class SiteStats { public int Destinations, Hotels, RoomTypes, Reviews; }

        public static SiteStats Stats()
        {
            using (var c = TravelDb.Open())
                return c.QueryFirst<SiteStats>(@"SELECT (SELECT COUNT(*) FROM DiaDiem WHERE TrangThai=1) Destinations, (SELECT COUNT(*) FROM KhachSan WHERE TrangThai=1) Hotels,
                    (SELECT COUNT(*) FROM Phong WHERE TrangThai=1) RoomTypes, (SELECT COUNT(*) FROM DanhGia WHERE TrangThai=1) Reviews");
        }

        public static HashSet<int> FavoriteIds(int? customerId)
        {
            if (!customerId.HasValue) return new HashSet<int>();
            using (var c = TravelDb.Open())
                return new HashSet<int>(c.Query<int>("SELECT MaKS FROM YeuThich WHERE MaKH=@k", new { k = customerId }));
        }

        public static List<ReviewItem> Reviews(int hotelId)
        {
            using (var c = TravelDb.Open())
                return c.Query<ReviewItem>(@"SELECT g.MaDanhGia Id, kh.HoTen CustomerName, g.SoSao Rating, g.NoiDung Content, g.NgayDanhGia CreatedAt
                    FROM DanhGia g INNER JOIN KhachHang kh ON kh.MaKH=g.MaKH WHERE g.MaKS=@h AND g.TrangThai=1 ORDER BY g.NgayDanhGia DESC", new { h = hotelId }).ToList();
        }

        public static List<DestinationItem> Destinations()
        {
            using (var c = TravelDb.Open())
                return c.Query<DestinationItem>(@"SELECT d.MaDiaDiem Id, d.TenDiaDiem Name, d.ThanhPho City, d.MoTa Description, d.HinhAnh ImageUrl, d.PhongCach TravelStyle, d.Vung Region,
                    (SELECT COUNT(*) FROM KhachSan k WHERE k.MaDiaDiem=d.MaDiaDiem AND k.TrangThai=1) HotelCount FROM DiaDiem d WHERE d.TrangThai=1 ORDER BY d.MaDiaDiem").ToList();
        }

        // ---- Bài viết ----
        public static List<ArticleRow> Articles(string q, string category, int take = 24)
        {
            using (var c = TravelDb.Open())
                return c.Query<ArticleRow>(@"
SELECT TOP (@take) b.MaBaiViet Id, b.MaDiaDiem DestinationId, d.TenDiaDiem DestinationName, b.TieuDe Title, b.MoTaNgan Summary, b.HinhAnh ImageUrl,
       b.TacGia Author, ISNULL(b.DanhMuc,N'Kinh nghiệm') Category, b.NgayDang PostedAt, b.LuotXem Views
FROM BaiViet b LEFT JOIN DiaDiem d ON d.MaDiaDiem=b.MaDiaDiem
WHERE b.TrangThai=1 AND (@q IS NULL OR b.TieuDe LIKE @q COLLATE Vietnamese_100_CI_AI OR b.MoTaNgan LIKE @q COLLATE Vietnamese_100_CI_AI)
  AND (@cat IS NULL OR ISNULL(b.DanhMuc,N'Kinh nghiệm') = @cat)
ORDER BY b.NgayDang DESC",
                    new { take, q = string.IsNullOrWhiteSpace(q) ? null : "%" + q.Trim().Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%", cat = string.IsNullOrWhiteSpace(category) ? null : category }).ToList();
        }

        public static List<string> ArticleCategories()
        {
            using (var c = TravelDb.Open())
                return c.Query<string>("SELECT DISTINCT ISNULL(DanhMuc,N'Kinh nghiệm') FROM BaiViet WHERE TrangThai=1").ToList();
        }

        public static ArticleRow Article(int id, bool countView)
        {
            using (var c = TravelDb.Open())
            {
                if (countView) c.Execute("UPDATE BaiViet SET LuotXem=LuotXem+1 WHERE MaBaiViet=@id AND TrangThai=1", new { id });
                return c.QueryFirstOrDefault<ArticleRow>(@"
SELECT b.MaBaiViet Id, b.MaDiaDiem DestinationId, d.TenDiaDiem DestinationName, b.TieuDe Title, b.MoTaNgan Summary, b.NoiDung Content, b.HinhAnh ImageUrl,
       b.TacGia Author, ISNULL(b.DanhMuc,N'Kinh nghiệm') Category, b.NgayDang PostedAt, b.LuotXem Views, b.SeoTieuDe SeoTitle, b.SeoMoTa SeoDescription
FROM BaiViet b LEFT JOIN DiaDiem d ON d.MaDiaDiem=b.MaDiaDiem WHERE b.MaBaiViet=@id AND b.TrangThai=1", new { id });
            }
        }

        public static List<HotelCard> HotelsInDestination(int destId)
        {
            return HotelSearchService.Search(new SearchCriteria { DestinationId = destId, PageSize = 12 }).Items;
        }
    }
}
