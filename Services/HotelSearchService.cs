using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using TravelHub.Data;
using TravelHub.Models;

namespace TravelHub.Services
{
    public class SearchCriteria
    {
        public int? DestinationId { get; set; }
        public string Q { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public int Adults { get; set; } = 2;
        public int Children { get; set; }
        public int Rooms { get; set; } = 1;
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinStars { get; set; }
        public double? MinRating { get; set; }
        public List<string> Types { get; set; } = new List<string>();
        public List<int> Amenities { get; set; } = new List<int>();
        public bool Breakfast { get; set; }
        public bool FreeCancel { get; set; }
        public string Sort { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 9;

        public int Nights { get { return CheckIn.HasValue && CheckOut.HasValue ? Math.Max(0, (int)(CheckOut.Value.Date - CheckIn.Value.Date).TotalDays) : 0; } }
        public bool HasDates { get { return Nights > 0; } }
        public int Guests { get { return Math.Max(1, Adults + Children); } }
    }

    public class SearchResult
    {
        public List<HotelCard> Items { get; set; } = new List<HotelCard>();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Pages { get { return PageSize == 0 ? 0 : (Total + PageSize - 1) / PageSize; } }
    }

    public class AmenityOption { public int Id { get; set; } public string Name { get; set; } }

    public static class HotelSearchService
    {
        public static SearchResult Search(SearchCriteria s)
        {
            if (s.Page < 1) s.Page = 1;
            if (s.PageSize < 1 || s.PageSize > 48) s.PageSize = 9;
            if (s.Rooms < 1) s.Rooms = 1;
            var p = new DynamicParameters();
            var where = new StringBuilder(" WHERE ks.TrangThai = 1 ");
            bool dates = s.HasDates;

            // Điều kiện phòng đủ điều kiện (đủ sức chứa, còn trống mọi đêm nếu có ngày).
            var roomCond = new StringBuilder("rp.MaKS = ks.MaKS AND rp.TrangThai = 1 AND rp.SoNguoiToiDa * @rooms >= @guests");
            if (dates) roomCond.Append(" AND dbo.fn_PhongConLai(rp.MaPhong, @ci, @co) >= @rooms");
            p.Add("rooms", s.Rooms); p.Add("guests", s.Guests);
            if (dates) { p.Add("ci", s.CheckIn.Value.Date); p.Add("co", s.CheckOut.Value.Date); }
            if (s.MinPrice.HasValue) { roomCond.Append(" AND rp.GiaPhong >= @minp"); p.Add("minp", s.MinPrice); }
            if (s.MaxPrice.HasValue) { roomCond.Append(" AND rp.GiaPhong <= @maxp"); p.Add("maxp", s.MaxPrice); }
            // Có ngày hoặc có lọc giá/số khách → bắt buộc có ít nhất 1 phòng đủ điều kiện.
            where.Append(" AND EXISTS (SELECT 1 FROM Phong rp WHERE " + roomCond + ") ");

            if (s.DestinationId.HasValue) { where.Append(" AND ks.MaDiaDiem = @dest"); p.Add("dest", s.DestinationId); }
            if (!string.IsNullOrWhiteSpace(s.Q))
            {
                where.Append(" AND (REPLACE(REPLACE(ks.TenKS,N'Đ',N'D'),N'đ',N'd') LIKE @q COLLATE Vietnamese_100_CI_AI OR REPLACE(REPLACE(ks.DiaChi,N'Đ',N'D'),N'đ',N'd') LIKE @q COLLATE Vietnamese_100_CI_AI OR REPLACE(REPLACE(dd.TenDiaDiem,N'Đ',N'D'),N'đ',N'd') LIKE @q COLLATE Vietnamese_100_CI_AI OR REPLACE(REPLACE(dd.ThanhPho,N'Đ',N'D'),N'đ',N'd') LIKE @q COLLATE Vietnamese_100_CI_AI)");
                p.Add("q", "%" + s.Q.Trim().Replace('Đ', 'D').Replace('đ', 'd').Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%");
            }
            if (s.MinStars.HasValue) { where.Append(" AND ks.SoSao >= @stars"); p.Add("stars", s.MinStars); }
            if (s.Types != null && s.Types.Count > 0) { where.Append(" AND ks.LoaiHinh IN @types"); p.Add("types", s.Types); }
            if (s.Breakfast) where.Append(" AND ks.BuaSang = 1");
            if (s.FreeCancel) where.Append(" AND ks.ChinhSachHuyLoai <> 'NONREFUND'");
            int i = 0;
            foreach (var a in s.Amenities ?? new List<int>())
            {
                where.Append(" AND EXISTS (SELECT 1 FROM KhachSan_TienIch kt WHERE kt.MaKS = ks.MaKS AND kt.MaTienIch = @am" + i + ")");
                p.Add("am" + i, a); i++;
            }
            string having = s.MinRating.HasValue ? " AND ISNULL(rv.Rating,0) >= @minr" : "";
            if (s.MinRating.HasValue) p.Add("minr", s.MinRating);

            string order;
            switch (s.Sort)
            {
                case "price-asc": order = "PriceFrom ASC, ks.MaKS"; break;
                case "price-desc": order = "PriceFrom DESC, ks.MaKS"; break;
                case "rating": order = "ISNULL(rv.Rating,0) DESC, ISNULL(rv.Cnt,0) DESC, ks.MaKS"; break;
                default: order = "ISNULL(rv.Cnt,0) DESC, ISNULL(rv.Rating,0) DESC, ks.MaKS"; break;   // phổ biến
            }
            p.Add("skip", (s.Page - 1) * s.PageSize); p.Add("take", s.PageSize);

            var priceExpr = "ISNULL((SELECT MIN(rp.GiaPhong) FROM Phong rp WHERE " + roomCond + "), ks.GiaTu)";
            var leftExpr = dates ? "ISNULL((SELECT MAX(dbo.fn_PhongConLai(rp.MaPhong, @ci, @co)) FROM Phong rp WHERE " + roomCond + "), 0)" : "0";

            var sql = @"
SELECT ks.MaKS Id, ks.TenKS Name, ks.DiaChi Address, ks.MaDiaDiem DestinationId, dd.TenDiaDiem DestinationName, ks.SoSao Stars,
       " + priceExpr + @" AS PriceFrom, ks.HinhAnhDaiDien CoverImageUrl, ks.MoTa Description,
       CAST(ISNULL(rv.Rating,0) AS float) Rating, ISNULL(rv.Cnt,0) ReviewCount, ISNULL(ks.LoaiHinh, N'Khách sạn') PropertyType,
       ks.BuaSang BreakfastIncluded, ks.ChinhSachHuyLoai CancelPolicy, " + leftExpr + @" RoomsLeft,
       COUNT(*) OVER() AS TotalRows
FROM KhachSan ks
INNER JOIN DiaDiem dd ON dd.MaDiaDiem = ks.MaDiaDiem
OUTER APPLY (SELECT AVG(CAST(g.SoSao AS float)) Rating, COUNT(*) Cnt FROM DanhGia g WHERE g.MaKS = ks.MaKS AND g.TrangThai = 1) rv
" + where + having + @"
ORDER BY " + order + @"
OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY";

            var res = new SearchResult { Page = s.Page, PageSize = s.PageSize };
            using (var c = TravelDb.Open())
            {
                var rows = c.Query<HotelRow>(sql, p).ToList();
                if (rows.Count > 0) res.Total = rows[0].TotalRows;
                else if (s.Page > 1)
                {
                    // trang vượt quá số kết quả: đếm lại để hiển thị đúng tổng
                    var cnt = "SELECT COUNT(*) FROM KhachSan ks INNER JOIN DiaDiem dd ON dd.MaDiaDiem=ks.MaDiaDiem OUTER APPLY (SELECT AVG(CAST(g.SoSao AS float)) Rating FROM DanhGia g WHERE g.MaKS=ks.MaKS AND g.TrangThai=1) rv " + where + having;
                    res.Total = c.ExecuteScalar<int>(cnt, p);
                }
                var ids = rows.Select(r => r.Id).ToList();
                var am = ids.Count == 0 ? new List<AmRow>() : c.Query<AmRow>(
                    @"SELECT kt.MaKS HotelId, t.TenTienIch Name FROM KhachSan_TienIch kt INNER JOIN TienIch t ON t.MaTienIch=kt.MaTienIch WHERE t.TrangThai=1 AND kt.MaKS IN @ids ORDER BY t.MaTienIch", new { ids }).ToList();
                foreach (var r in rows)
                {
                    var h = r.ToCard();
                    h.CancelPolicy = CancellationPolicy.ShortLabel(r.CancelPolicy);
                    h.TotalForStay = dates ? h.PriceFrom * s.Nights * s.Rooms : 0;
                    h.Amenities = am.Where(a => a.HotelId == r.Id).Select(a => a.Name).Take(5).ToList();
                    res.Items.Add(h);
                }
            }
            return res;
        }

        public static List<AmenityOption> Amenities()
        {
            using (var c = TravelDb.Open())
                return c.Query<AmenityOption>("SELECT MaTienIch Id, TenTienIch Name FROM TienIch WHERE TrangThai=1 ORDER BY TenTienIch").ToList();
        }

        public static List<string> PropertyTypes()
        {
            using (var c = TravelDb.Open())
                return c.Query<string>("SELECT DISTINCT LoaiHinh FROM KhachSan WHERE TrangThai=1 AND LoaiHinh IS NOT NULL ORDER BY LoaiHinh").ToList();
        }

        class AmRow { public int HotelId { get; set; } public string Name { get; set; } }

        class HotelRow
        {
            public int Id { get; set; } public string Name { get; set; } public string Address { get; set; }
            public int DestinationId { get; set; } public string DestinationName { get; set; } public int Stars { get; set; }
            public decimal PriceFrom { get; set; } public string CoverImageUrl { get; set; } public string Description { get; set; }
            public double Rating { get; set; } public int ReviewCount { get; set; } public string PropertyType { get; set; }
            public bool BreakfastIncluded { get; set; } public string CancelPolicy { get; set; } public int RoomsLeft { get; set; } public int TotalRows { get; set; }
            public HotelCard ToCard()
            {
                return new HotelCard { Id = Id, Name = Name, Address = Address, DestinationId = DestinationId, DestinationName = DestinationName, Stars = Stars,
                    PriceFrom = PriceFrom, CoverImageUrl = CoverImageUrl, Description = Description, Rating = Rating, ReviewCount = ReviewCount,
                    PropertyType = PropertyType, BreakfastIncluded = BreakfastIncluded, CancelPolicy = CancelPolicy, RoomsLeft = RoomsLeft };
            }
        }
    }
}
