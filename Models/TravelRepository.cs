using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TravelHub.Models;

namespace TravelHub.Data
{
    public class TravelRepository
    {
        public List<DestinationItem> GetDestinations()
        {
            var list = new List<DestinationItem>();

            using (var conn = TravelDb.Open())
            using (var cmd = new SqlCommand(
                "SELECT MaDiaDiem, TenDiaDiem, ThanhPho, MoTa, HinhAnh " +
                "FROM DiaDiem WHERE TrangThai = 1 ORDER BY MaDiaDiem", conn))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    var id = GetInt(r, "MaDiaDiem");

                    list.Add(new DestinationItem
                    {
                        Id = id,
                        Name = GetStr(r, "TenDiaDiem"),
                        City = GetStr(r, "ThanhPho"),
                        Description = GetStr(r, "MoTa"),

                        // LẤY ẢNH TRỰC TIẾP TỪ DATABASE
                        ImageUrl = GetStr(r, "HinhAnh"),

                        TravelStyle = null
                    });
                }
            }

            return list;
        }

        public DestinationItem GetDestination(int id)
        {
            foreach (var d in GetDestinations())
                if (d.Id == id) return d;
            return null;
        }

        public List<HotelCard> SearchHotels(
    int? destinationId,
    decimal? maxPrice,
    int? stars,
    string sort,
    DateTime? checkIn = null,
    DateTime? checkOut = null,
    int guests = 1)
        {
            var sql = @"
SELECT 
    ks.MaKS,
    ks.TenKS,
    ks.DiaChi,
    ks.MaDiaDiem,
    dd.TenDiaDiem,
    ks.SoSao,
    ks.GiaTu,
    ks.HinhAnhDaiDien,
    ks.MoTa,

    ISNULL(
        AVG(CAST(dg.SoSao AS float)),
        0
    ) AS Rating,

    COUNT(dg.MaDanhGia) AS ReviewCount

FROM KhachSan ks

INNER JOIN DiaDiem dd
    ON dd.MaDiaDiem = ks.MaDiaDiem

LEFT JOIN DanhGia dg
    ON dg.MaKS = ks.MaKS
    AND dg.TrangThai = 1

WHERE ks.TrangThai = 1

    AND (
        @dest IS NULL
        OR ks.MaDiaDiem = @dest
    )

    AND (
        @stars IS NULL
        OR ks.SoSao >= @stars
    )

    AND (
        @maxPrice IS NULL
        OR ks.GiaTu <= @maxPrice
    )

    /* =========================================
       KIỂM TRA KHÁCH SẠN CÓ PHÒNG TRỐNG
       ========================================= */

    AND (
        @checkIn IS NULL
        OR @checkOut IS NULL

        OR EXISTS
        (
            SELECT 1

            FROM Phong p

            WHERE p.MaKS = ks.MaKS
                AND p.TrangThai = 1
                AND p.SoNguoiToiDa >= @guests

                AND p.SoLuong >
                (
                    SELECT ISNULL(
                        SUM(ct.SoLuong),
                        0
                    )

                    FROM ChiTietDatPhong ct

                    INNER JOIN DonDatPhong d
                        ON d.MaDon = ct.MaDon

                    WHERE ct.MaPhong = p.MaPhong

                        AND ISNULL(d.TrangThai, N'') NOT IN (N'Đã hủy', N'Hủy', N'Cancelled', N'Canceled')

                        AND d.NgayNhanPhong < @checkOut

                        AND d.NgayTraPhong > @checkIn
                )
        )
    )

GROUP BY
    ks.MaKS,
    ks.TenKS,
    ks.DiaChi,
    ks.MaDiaDiem,
    dd.TenDiaDiem,
    ks.SoSao,
    ks.GiaTu,
    ks.HinhAnhDaiDien,
    ks.MoTa
";

            if (sort == "price-asc")
            {
                sql += " ORDER BY ks.GiaTu ASC";
            }
            else if (sort == "price-desc")
            {
                sql += " ORDER BY ks.GiaTu DESC";
            }
            else if (sort == "rating")
            {
                sql +=
                    " ORDER BY Rating DESC, ks.SoSao DESC";
            }
            else
            {
                sql +=
                    " ORDER BY ReviewCount DESC, Rating DESC, ks.GiaTu ASC";
            }

            var list = new List<HotelCard>();

            using (var conn = TravelDb.Open())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue(
                    "@dest",
                    (object)destinationId ?? DBNull.Value
                );

                cmd.Parameters.AddWithValue(
                    "@stars",
                    (object)stars ?? DBNull.Value
                );

                cmd.Parameters.AddWithValue(
                    "@maxPrice",
                    (object)maxPrice ?? DBNull.Value
                );

                cmd.Parameters.AddWithValue(
                    "@checkIn",
                    (object)checkIn ?? DBNull.Value
                );

                cmd.Parameters.AddWithValue(
                    "@checkOut",
                    (object)checkOut ?? DBNull.Value
                );

                cmd.Parameters.AddWithValue(
                    "@guests",
                    guests
                );

                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(MapHotel(r));
                    }
                }
            }

            return list;
        }

        public HotelCard GetHotel(int id)
        {
            using (var conn = TravelDb.Open())
            using (var cmd = new SqlCommand(@"
SELECT ks.MaKS, ks.TenKS, ks.DiaChi, ks.MaDiaDiem, dd.TenDiaDiem, ks.SoSao, ks.GiaTu,
       ks.HinhAnhDaiDien, ks.MoTa,
       ISNULL(AVG(CAST(dg.SoSao AS float)), 0) AS Rating,
       COUNT(dg.MaDanhGia) AS ReviewCount
FROM KhachSan ks
INNER JOIN DiaDiem dd ON dd.MaDiaDiem = ks.MaDiaDiem
LEFT JOIN DanhGia dg ON dg.MaKS = ks.MaKS AND dg.TrangThai = 1
WHERE ks.MaKS = @id
GROUP BY ks.MaKS, ks.TenKS, ks.DiaChi, ks.MaDiaDiem, dd.TenDiaDiem, ks.SoSao, ks.GiaTu, ks.HinhAnhDaiDien, ks.MoTa", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read()) return MapHotel(r);
                }
            }
            return null;
        }

        public List<HotelCard> GetPartnerHotels(int accountId)
        {
            var sql = @"
SELECT 
    ks.MaKS,
    ks.TenKS,
    ks.DiaChi,
    ks.MaDiaDiem,
    dd.TenDiaDiem,
    ks.SoSao,
    ks.GiaTu,
    ks.HinhAnhDaiDien,
    ks.MoTa,

    ISNULL(
        AVG(CAST(dg.SoSao AS float)),
        0
    ) AS Rating,

    COUNT(dg.MaDanhGia) AS ReviewCount

FROM KhachSan ks

INNER JOIN DiaDiem dd
    ON dd.MaDiaDiem = ks.MaDiaDiem

LEFT JOIN DanhGia dg
    ON dg.MaKS = ks.MaKS
    AND dg.TrangThai = 1

WHERE ks.MaTaiKhoanDoiTac = @accountId AND ks.TrangThai = 1

GROUP BY
    ks.MaKS,
    ks.TenKS,
    ks.DiaChi,
    ks.MaDiaDiem,
    dd.TenDiaDiem,
    ks.SoSao,
    ks.GiaTu,
    ks.HinhAnhDaiDien,
    ks.MoTa

ORDER BY ks.MaKS;
";

            var list = new List<HotelCard>();

            using (var conn = TravelDb.Open())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue(
                    "@accountId",
                    accountId
                );

                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(MapHotel(r));
                    }
                }
            }

            return list;

        }
        public List<HotelCard> GetInactivePartnerHotels(int accountId)
{
    const string sql = @"
        SELECT
            ks.MaKS,
            ks.TenKS,
            ks.DiaChi,
            ks.MaDiaDiem,
            dd.TenDiaDiem,
            ks.SoSao,
            ks.GiaTu,
            ks.HinhAnhDaiDien,
            ks.MoTa,
            ISNULL(
                AVG(CAST(dg.SoSao AS float)), 0
            ) AS Rating,
            COUNT(dg.MaDanhGia) AS ReviewCount
        FROM dbo.KhachSan ks
        INNER JOIN dbo.DiaDiem dd
            ON dd.MaDiaDiem = ks.MaDiaDiem
        LEFT JOIN dbo.DanhGia dg
            ON dg.MaKS = ks.MaKS
            AND dg.TrangThai = 1
        WHERE ks.MaTaiKhoanDoiTac = @accountId
          AND ks.TrangThai = 0
        GROUP BY
            ks.MaKS,
            ks.TenKS,
            ks.DiaChi,
            ks.MaDiaDiem,
            dd.TenDiaDiem,
            ks.SoSao,
            ks.GiaTu,
            ks.HinhAnhDaiDien,
            ks.MoTa
        ORDER BY ks.MaKS DESC";

    var list = new List<HotelCard>();

    using (var conn = TravelDb.Open())
    using (var cmd = new SqlCommand(sql, conn))
    {
        cmd.Parameters.AddWithValue("@accountId", accountId);

        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                list.Add(MapHotel(reader));
            }
        }
    }

    return list;
}
        public string GetHotelPolicy(int hotelId)
        {
            using (var conn = TravelDb.Open())
            using (var cmd = new SqlCommand("SELECT ChinhSach FROM KhachSan WHERE MaKS=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", hotelId);
                var v = cmd.ExecuteScalar();
                if (v != null && v != DBNull.Value && !string.IsNullOrWhiteSpace(Convert.ToString(v)))
                    return Convert.ToString(v);
            }
            return "Chính sách lưu trú và hủy phòng vui lòng xem thông tin khi đặt phòng hoặc liên hệ khách sạn.";
        }

        public string GetHotelPhone(int hotelId)
        {
            using (var conn = TravelDb.Open())
            using (var cmd = new SqlCommand("SELECT SoDienThoai FROM KhachSan WHERE MaKS=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", hotelId);
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read()) return GetStr(r, "SoDienThoai");
                }
            }
            return null;
        }

        public List<string> GetAmenities(int hotelId)
        {
            var list = new List<string>();
            using (var conn = TravelDb.Open())
            using (var cmd = new SqlCommand(@"
SELECT t.TenTienIch FROM KhachSan_TienIch kt
INNER JOIN TienIch t ON t.MaTienIch = kt.MaTienIch
WHERE kt.MaKS = @id AND t.TrangThai = 1", conn))
            {
                cmd.Parameters.AddWithValue("@id", hotelId);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read()) list.Add(GetStr(r, "TenTienIch"));
                }
            }
            return list;
        }

        public List<ReviewItem> GetReviews(int hotelId)
        {
            var list = new List<ReviewItem>();
            using (var conn = TravelDb.Open())
            using (var cmd = new SqlCommand(@"
SELECT kh.HoTen, dg.SoSao, dg.NoiDung, dg.NgayDanhGia
FROM DanhGia dg INNER JOIN KhachHang kh ON kh.MaKH = dg.MaKH
WHERE dg.MaKS = @id AND dg.TrangThai = 1
ORDER BY dg.NgayDanhGia DESC", conn))
            {
                cmd.Parameters.AddWithValue("@id", hotelId);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new ReviewItem
                        {
                            CustomerName = GetStr(r, "HoTen"),
                            Rating = GetInt(r, "SoSao"),
                            Content = GetStr(r, "NoiDung"),
                            CreatedAt = GetDate(r, "NgayDanhGia")
                        });
                    }
                }
            }
            return list;
        }

        public List<RoomItem> GetRooms(
            int hotelId,
            DateTime checkIn,
            DateTime checkOut,
            int guests)
        {
            var list = new List<RoomItem>();
            using (var conn = TravelDb.Open())
                            using (var cmd = new SqlCommand(@"
                SELECT p.MaPhong, p.MaKS, p.TenPhong, p.LoaiPhong, p.SoNguoiToiDa, p.SoGiuong, p.GiaPhong, p.SoLuong, p.MoTa, p.HinhAnh,
                (p.SoLuong - dbo.fn_PhongConLai(p.MaPhong, @in, @out)) AS Occupied
                FROM Phong p
                WHERE p.MaKS = @hid
                  AND p.TrangThai = 1
                  AND p.SoNguoiToiDa >= @guests
                ORDER BY p.GiaPhong", conn))
            {
                cmd.Parameters.AddWithValue("@hid", hotelId);
                cmd.Parameters.AddWithValue("@in", checkIn.Date);
                cmd.Parameters.AddWithValue("@out", checkOut.Date);
                cmd.Parameters.AddWithValue("@guests", guests);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new RoomItem
                        {
                            Id = GetInt(r, "MaPhong"),
                            HotelId = GetInt(r, "MaKS"),
                            Name = GetStr(r, "TenPhong"),
                            RoomType = GetStr(r, "LoaiPhong"),
                            MaxGuests = GetInt(r, "SoNguoiToiDa"),
                            BedCount = r["SoGiuong"] is DBNull ? (int?)null : Convert.ToInt32(r["SoGiuong"]),
                            Price = GetDec(r, "GiaPhong"),
                            TotalInventory = GetInt(r, "SoLuong"),
                            Occupied = GetInt(r, "Occupied"),
                            Description = GetStr(r, "MoTa"),
                            ImageUrl = GetStr(r, "HinhAnh")
                        });
                    }
                }
            }
            return list;
        }

        public RoomItem GetRoom(
            int id,
            DateTime checkIn,
            DateTime checkOut,
            int guests)
        {
            using (var conn = TravelDb.Open())
            using (var cmd = new SqlCommand(@"
        SELECT MaKS
        FROM Phong
        WHERE MaPhong = @id
          AND TrangThai = 1
    ", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);

                var hotelId = cmd.ExecuteScalar();

                if (hotelId == null || hotelId is DBNull)
                    return null;

                foreach (var room in GetRooms(
                    Convert.ToInt32(hotelId),
                    checkIn,
                    checkOut,
                    guests))
                {
                    if (room.Id == id)
                        return room;
                }
            }

            return null;
        }

        public List<PromotionItem> GetPromotions()
        {
            var list = new List<PromotionItem>();
            using (var conn = TravelDb.Open())
            using (var cmd = new SqlCommand(@"
SELECT MaKM, TenKM, MaCode, MoTa, PhanTramGiam, SoTienGiam
FROM KhuyenMai
WHERE TrangThai = 1 AND (NgayBatDau IS NULL OR NgayBatDau <= GETDATE()) AND (NgayKetThuc IS NULL OR NgayKetThuc >= GETDATE())", conn))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    list.Add(new PromotionItem
                    {
                        Id = GetInt(r, "MaKM"),
                        Name = GetStr(r, "TenKM"),
                        Code = GetStr(r, "MaCode"),
                        Description = GetStr(r, "MoTa"),
                        PercentOff = r["PhanTramGiam"] is DBNull ? (decimal?)null : Convert.ToDecimal(r["PhanTramGiam"]),
                        AmountOff = r["SoTienGiam"] is DBNull ? (decimal?)null : Convert.ToDecimal(r["SoTienGiam"])
                    });
                }
            }
            return list;
        }

        public List<ArticleItem> GetArticles()
        {
            var list = new List<ArticleItem>();
            using (var conn = TravelDb.Open())
            using (var cmd = new SqlCommand("SELECT TOP 8 MaBaiViet, MaDiaDiem, TieuDe, MoTaNgan, HinhAnh, TacGia FROM BaiViet WHERE TrangThai = 1 ORDER BY NgayDang DESC", conn))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    var destId = r["MaDiaDiem"] is DBNull ? 1 : Convert.ToInt32(r["MaDiaDiem"]);
                    list.Add(new ArticleItem
                    {
                        Id = GetInt(r, "MaBaiViet"),
                        DestinationId = destId,
                        Title = GetStr(r, "TieuDe"),
                        Summary = GetStr(r, "MoTaNgan"),
                        ImageUrl = GetStr(r, "HinhAnh"),
                        Author = GetStr(r, "TacGia")
                    });
                }
            }
            return list;
        }

        public List<AttractionItem> GetAttractions(int destinationId)
        {
            var list = new List<AttractionItem>();
            using (var conn = TravelDb.Open())

            using (var cmd = new SqlCommand(
                "SELECT MaDDDL, TenDiaDiem, LoaiDiaDiem, MoTa, HinhAnh, GiaVe " +
                "FROM DiaDiemDuLich WHERE MaDiaDiem=@id AND TrangThai=1",
                conn))

            {
                cmd.Parameters.AddWithValue("@id", destinationId);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new AttractionItem
                        {
                            Id = GetInt(r, "MaDDDL"),
                            Name = GetStr(r, "TenDiaDiem"),
                            Category = GetStr(r, "LoaiDiaDiem"),
                            Description = GetStr(r, "MoTa"),
                            ImageUrl = GetStr(r, "HinhAnh"),
                            TicketPrice = r["GiaVe"] is DBNull ? (decimal?)null : Convert.ToDecimal(r["GiaVe"])
                        });
                    }
                }
            }
            return list;
        }

        public List<FlightItem> SearchFlights(string origin, string destination)
        {
            var sql = @"
SELECT cb.MaChuyenBay, hb.TenHangBay, cb.MaChuyen, cb.SanBayDi, cb.SanBayDen, cb.ThoiGianDi, cb.ThoiGianDen, cb.GiaVe, cb.SoGheConLai
FROM ChuyenBay cb
INNER JOIN HangBay hb ON hb.MaHangBay = cb.MaHangBay
WHERE cb.TrangThai = 1
  AND (@o IS NULL OR cb.SanBayDi LIKE @o)
  AND (@d IS NULL OR cb.SanBayDen LIKE @d)
ORDER BY cb.ThoiGianDi";
            var list = new List<FlightItem>();
            using (var conn = TravelDb.Open())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@o", string.IsNullOrWhiteSpace(origin) ? (object)DBNull.Value : "%" + origin.Trim() + "%");
                cmd.Parameters.AddWithValue("@d", string.IsNullOrWhiteSpace(destination) ? (object)DBNull.Value : "%" + destination.Trim() + "%");
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new FlightItem
                        {
                            Id = GetInt(r, "MaChuyenBay"),
                            Airline = GetStr(r, "TenHangBay"),
                            FlightNumber = GetStr(r, "MaChuyen"),
                            Origin = GetStr(r, "SanBayDi"),
                            Destination = GetStr(r, "SanBayDen"),
                            DepartureAt = GetDate(r, "ThoiGianDi"),
                            ArrivalAt = GetDate(r, "ThoiGianDen"),
                            Price = GetDec(r, "GiaVe"),
                            SeatsAvailable = GetInt(r, "SoGheConLai")
                        });
                    }
                }
            }
            return list;
        }

        public List<string> GetAirports()
        {
            var list = new List<string>();
            using (var conn = TravelDb.Open())
            using (var cmd = new SqlCommand("SELECT DISTINCT SanBayDi FROM ChuyenBay UNION SELECT DISTINCT SanBayDen FROM ChuyenBay", conn))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read()) list.Add(Convert.ToString(r[0]));
            }
            return list;
        }


        private static HotelCard MapHotel(SqlDataReader r)
        {
            var id = GetInt(r, "MaKS");
            return new HotelCard
            {
                Id = id,
                Name = GetStr(r, "TenKS"),
                Address = GetStr(r, "DiaChi"),
                DestinationId = GetInt(r, "MaDiaDiem"),
                DestinationName = GetStr(r, "TenDiaDiem"),
                Stars = GetInt(r, "SoSao"),
                PriceFrom = GetDec(r, "GiaTu"),
                CoverImageUrl = GetStr(r, "HinhAnhDaiDien"),
                Description = GetStr(r, "MoTa"),
                Rating = Convert.ToDouble(r["Rating"]),
                ReviewCount = GetInt(r, "ReviewCount"),
                PropertyType = "Khách sạn"
            };
        }

        private static BookingItem MapBooking(SqlDataReader r)
        {
            return new BookingItem
            {
                Id = GetInt(r, "MaDon"),
                BookingCode = GetStr(r, "MaDonCode"),
                HotelId = GetInt(r, "MaKS"),
                HotelName = GetStr(r, "TenKS"),
                CheckIn = GetDate(r, "NgayNhanPhong"),
                CheckOut = GetDate(r, "NgayTraPhong"),
                Adults = GetInt(r, "SoNguoiLon"),
                Total = GetDec(r, "ThanhTien"),
                StatusText = GetStr(r, "TrangThai"),
                StatusCode = GetStr(r, "TrangThaiCode"),
                RoomName = GetStr(r, "TenPhong"),
                Nights = GetInt(r, "SoDem")
            };
        }

        private static int GetInt(SqlDataReader r, string col)
        {
            var v = r[col];
            return v is DBNull ? 0 : Convert.ToInt32(v);
        }

        private static decimal GetDec(SqlDataReader r, string col)
        {
            var v = r[col];
            return v is DBNull ? 0 : Convert.ToDecimal(v);
        }

        private static string GetStr(SqlDataReader r, string col)
        {
            var v = r[col];
            return v is DBNull ? null : Convert.ToString(v);
        }

        private static DateTime GetDate(SqlDataReader r, string col)
        {
            var v = r[col];
            return v is DBNull ? DateTime.MinValue : Convert.ToDateTime(v);
        }

        private static bool GetBool(SqlDataReader r, string col)
        {
            var v = r[col];
            if (v is DBNull) return false;
            if (v is bool) return (bool)v;
            return Convert.ToInt32(v) != 0;
        }


        public List<HotelCard> GetFavoriteHotels(int customerId)
        {
            var list = new List<HotelCard>();
            const string sql = @"
SELECT ks.MaKS, ks.TenKS, ks.DiaChi, ks.MaDiaDiem, dd.TenDiaDiem,
       ks.SoSao, ks.GiaTu, ks.HinhAnhDaiDien, ks.MoTa,
       ISNULL(AVG(CAST(dg.SoSao AS float)), 0) AS Rating,
       COUNT(dg.MaDanhGia) AS ReviewCount
FROM YeuThich yt
INNER JOIN KhachSan ks ON ks.MaKS = yt.MaKS
INNER JOIN DiaDiem dd ON dd.MaDiaDiem = ks.MaDiaDiem
LEFT JOIN DanhGia dg ON dg.MaKS = ks.MaKS AND dg.TrangThai = 1
WHERE yt.MaKH = @customerId AND ks.TrangThai = 1
GROUP BY ks.MaKS, ks.TenKS, ks.DiaChi, ks.MaDiaDiem, dd.TenDiaDiem,
         ks.SoSao, ks.GiaTu, ks.HinhAnhDaiDien, ks.MoTa
ORDER BY ks.TenKS";
            using (var conn = TravelDb.Open())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@customerId", customerId);
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(MapHotel(r));
            }
            return list;
        }

        public bool IsHotelFavorite(int customerId, int hotelId)
        {
            using (var conn = TravelDb.Open())
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM YeuThich WHERE MaKH=@customerId AND MaKS=@hotelId", conn))
            {
                cmd.Parameters.AddWithValue("@customerId", customerId);
                cmd.Parameters.AddWithValue("@hotelId", hotelId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public void AddFavoriteHotel(int customerId, int hotelId)
        {
            using (var conn = TravelDb.Open())
            using (var tx = conn.BeginTransaction(IsolationLevel.Serializable))
            {
                using (var check = new SqlCommand("SELECT COUNT(1) FROM YeuThich WITH (UPDLOCK, HOLDLOCK) WHERE MaKH=@customerId AND MaKS=@hotelId", conn, tx))
                {
                    check.Parameters.AddWithValue("@customerId", customerId);
                    check.Parameters.AddWithValue("@hotelId", hotelId);
                    if (Convert.ToInt32(check.ExecuteScalar()) == 0)
                    {
                        using (var add = new SqlCommand("INSERT INTO YeuThich (MaKH, MaKS, NgayThem) SELECT @customerId, @hotelId, GETDATE() WHERE EXISTS (SELECT 1 FROM KhachSan WHERE MaKS=@hotelId AND TrangThai=1)", conn, tx))
                        {
                            add.Parameters.AddWithValue("@customerId", customerId);
                            add.Parameters.AddWithValue("@hotelId", hotelId);
                            if (add.ExecuteNonQuery() != 1) throw new InvalidOperationException("Khách sạn không tồn tại hoặc đã ngừng hoạt động.");
                        }
                    }
                }
                tx.Commit();
            }
        }

        public void RemoveFavoriteHotel(int customerId, int hotelId)
        {
            using (var conn = TravelDb.Open())
            using (var cmd = new SqlCommand("DELETE FROM YeuThich WHERE MaKH=@customerId AND MaKS=@hotelId", conn))
            {
                cmd.Parameters.AddWithValue("@customerId", customerId);
                cmd.Parameters.AddWithValue("@hotelId", hotelId);
                cmd.ExecuteNonQuery();
            }
        }

        public HotelCard GetPartnerHotel(int hotelId, int accountId)
        {
            foreach (var hotel in GetPartnerHotels(accountId))
                if (hotel.Id == hotelId) return hotel;
            return null;
        }

        public bool UpdatePartnerHotel(int hotelId, PartnerHotelCreateViewModel model, int accountId)
        {
            const string sql = @"UPDATE KhachSan SET TenKS=@name, MaDiaDiem=@destination,
DiaChi=@address, SoSao=@stars, GiaTu=@price, MoTa=@description,
HinhAnhDaiDien=@image WHERE MaKS=@id AND MaTaiKhoanDoiTac=@accountId AND TrangThai=1";
            using (var conn = TravelDb.Open())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@name", model.TenKS.Trim());
                cmd.Parameters.AddWithValue("@destination", model.MaDiaDiem);
                cmd.Parameters.AddWithValue("@address", model.DiaChi.Trim());
                cmd.Parameters.AddWithValue("@stars", model.SoSao);
                cmd.Parameters.AddWithValue("@price", model.GiaTu);
                cmd.Parameters.AddWithValue("@description", (object)model.MoTa ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@image", (object)model.HinhAnhDaiDien ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id", hotelId);
                cmd.Parameters.AddWithValue("@accountId", accountId);
                return cmd.ExecuteNonQuery() == 1;
            }
        }

        public bool DeletePartnerHotel(int hotelId, int accountId)
        {
            const string sql = @"
        UPDATE dbo.KhachSan
        SET TrangThai = 0
        WHERE MaKS = @id
          AND MaTaiKhoanDoiTac = @accountId
          AND TrangThai = 1;

        SELECT CASE
            WHEN EXISTS (
                SELECT 1
                FROM dbo.KhachSan
                WHERE MaKS = @id
                  AND MaTaiKhoanDoiTac = @accountId
                  AND TrangThai = 0
            )
            THEN 1
            ELSE 0
        END;
    ";

            using (var conn = TravelDb.Open())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", hotelId);
                cmd.Parameters.AddWithValue("@accountId", accountId);

                object result = cmd.ExecuteScalar();

                return result != null
                    && result != DBNull.Value
                    && Convert.ToInt32(result) == 1;
            }
        }
        
        public bool RestorePartnerHotel(int hotelId, int accountId)
        {
            const string sql = @"
        UPDATE KhachSan
        SET TrangThai = 1
        WHERE MaKS = @id
          AND MaTaiKhoanDoiTac = @accountId
          AND TrangThai = 0";

            using (var conn = TravelDb.Open())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", hotelId);
                cmd.Parameters.AddWithValue("@accountId", accountId);

                return cmd.ExecuteNonQuery() == 1;
            }
        }

        public int CreatePartnerHotel(
            PartnerHotelCreateViewModel model,
            int accountId)
        {
            var sql = @"
            INSERT INTO KhachSan
            (
                TenKS,
                MaDiaDiem,
                DiaChi,
                SoSao,
                GiaTu,
                MoTa,
                HinhAnhDaiDien,
                MaTaiKhoanDoiTac,
                TrangThai,
                NgayTao
            )
            VALUES
            (
                @TenKS,
                @MaDiaDiem,
                @DiaChi,
                @SoSao,
                @GiaTu,
                @MoTa,
                @HinhAnhDaiDien,
                @MaTaiKhoanDoiTac,
                1,
                GETDATE()
            );

            SELECT CAST(SCOPE_IDENTITY() AS int);
            ";

            using (var conn = TravelDb.Open())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue(
                    "@TenKS",
                    model.TenKS
                );

                cmd.Parameters.AddWithValue(
                    "@MaDiaDiem",
                    model.MaDiaDiem
                );

                cmd.Parameters.AddWithValue(
                    "@DiaChi",
                    model.DiaChi
                );

                cmd.Parameters.AddWithValue(
                    "@SoSao",
                    model.SoSao
                );

                cmd.Parameters.AddWithValue(
                    "@GiaTu",
                    model.GiaTu
                );


                cmd.Parameters.AddWithValue(
                    "@MoTa",
                    (object)model.MoTa ?? DBNull.Value
                );

                cmd.Parameters.AddWithValue(
                    "@HinhAnhDaiDien",
                    (object)model.HinhAnhDaiDien ?? DBNull.Value
                );

                cmd.Parameters.AddWithValue(
                    "@MaTaiKhoanDoiTac",
                    accountId
                );

                return (int)cmd.ExecuteScalar();
            }
        }

        public List<HinhAnhKhachSan> GetHotelImages(int hotelId)
        {
            using (var conn = TravelDb.Open())
            using (var cmd = new SqlCommand(@"
                SELECT MaHinhAnh,
                       MaKS,
                       DuongDanAnh,
                       MoTa,
                       LaAnhDaiDien
                FROM HinhAnhKhachSan
                WHERE MaKS = @hotelId
                ORDER BY LaAnhDaiDien DESC, MaHinhAnh
            ", conn))
            {
                cmd.Parameters.AddWithValue("@hotelId", hotelId);

                var list = new List<HinhAnhKhachSan>();

                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new HinhAnhKhachSan
                        {
                            MaHinhAnh = Convert.ToInt32(r["MaHinhAnh"]),
                            MaKS = Convert.ToInt32(r["MaKS"]),
                            DuongDanAnh = r["DuongDanAnh"].ToString(),
                            MoTa = r["MoTa"].ToString(),
                            LaAnhDaiDien = Convert.ToBoolean(r["LaAnhDaiDien"])
                        });
                    }
                }

                return list;
            }
        }
    }

}
