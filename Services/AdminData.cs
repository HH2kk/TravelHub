using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using TravelHub.Data;

namespace TravelHub.Services
{
    // ───────────────────────── Chuyến bay / vé máy bay ─────────────────────────

    public static class FlightInfo
    {
        public static readonly string[] Statuses = { "Scheduled", "Delayed", "Departed", "Arrived", "Cancelled" };
        public static string StatusText(string s)
        {
            switch (s) { case "Delayed": return "Chậm chuyến"; case "Departed": return "Đang bay"; case "Arrived": return "Đã hạ cánh"; case "Cancelled": return "Đã hủy"; default: return "Đúng lịch"; }
        }
        public static string StatusCls(string s) { return s == "Cancelled" ? "bad" : s == "Delayed" ? "warn" : s == "Departed" ? "info" : s == "Arrived" ? "gray" : "ok"; }
        public static string TicketCls(string code) { return code == "Cancelled" ? "bad" : code == "CheckedIn" ? "info" : code == "Completed" ? "gray" : "ok"; }
        public static string Duration(DateTime a, DateTime b) { var t = b - a; return (int)t.TotalHours + "g" + (t.Minutes > 0 ? t.Minutes.ToString("00") : ""); }
    }

    public class FlightRow
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public int AirlineId { get; set; }
        public string AirlineName { get; set; }
        public string AirlineIata { get; set; }
        public string FromCode { get; set; }
        public string FromName { get; set; }
        public string ToCode { get; set; }
        public string ToName { get; set; }
        public DateTime Departs { get; set; }
        public DateTime Arrives { get; set; }
        public decimal Price { get; set; }
        public decimal? BusinessPrice { get; set; }
        public int Seats { get; set; }
        public int SeatsLeft { get; set; }
        public bool Active { get; set; }
        public string Status { get; set; }
        public string Aircraft { get; set; }
        public string Gate { get; set; }
        public int BaggageKg { get; set; }
        public string Note { get; set; }
        public int TicketsActive { get; set; }
        public int TicketsCancelled { get; set; }
        public decimal TicketValue { get; set; }
        public int Sold { get { return Math.Max(0, Seats - SeatsLeft); } }
        public int LoadPct { get { return Seats == 0 ? 0 : (int)Math.Round(Sold * 100.0 / Seats); } }
    }

    public class TicketRow
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string Passenger { get; set; }
        public string DocNo { get; set; }
        public string Seat { get; set; }
        public string Class { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public string StatusCode { get; set; }
        public DateTime BookedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CancelReason { get; set; }
        public bool IsSample { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public int? BaggageKg { get; set; }
        public int FlightId { get; set; }
        public string FlightCode { get; set; }
        public string FlightStatus { get; set; }
        public string AirlineName { get; set; }
        public string AirlineIata { get; set; }
        public string FromCode { get; set; }
        public string FromName { get; set; }
        public string ToCode { get; set; }
        public string ToName { get; set; }
        public DateTime Departs { get; set; }
        public DateTime Arrives { get; set; }
        public string Aircraft { get; set; }
        public string Gate { get; set; }
    }

    public class TicketFilter { public string q; public string status; public string airline; public int? flight; public int? customer; public DateTime? from, to; }

    public class FlightFilter { public string q; public string airline; public string status; public string when; }

    public static class FlightAdmin
    {
        const string FlightSelect = @"SELECT c.MaChuyenBay Id, c.MaChuyen Code, c.MaHangBay AirlineId, h.TenHangBay AirlineName, h.MaIATA AirlineIata,
            ISNULL(c.MaSanBayDi,'') FromCode, c.SanBayDi FromName, ISNULL(c.MaSanBayDen,'') ToCode, c.SanBayDen ToName, c.ThoiGianDi Departs, c.ThoiGianDen Arrives,
            c.GiaVe Price, c.GiaThuongGia BusinessPrice, c.SoGhe Seats, c.SoGheConLai SeatsLeft, c.TrangThai Active, c.TrangThaiChuyen Status, ISNULL(c.LoaiMayBay,N'') Aircraft, ISNULL(c.CuaRa,N'') Gate, c.HanhLyKg BaggageKg, c.GhiChu Note,
            (SELECT COUNT(*) FROM VeMayBay v WHERE v.MaChuyenBay=c.MaChuyenBay AND v.TrangThaiCode<>'Cancelled') TicketsActive,
            (SELECT COUNT(*) FROM VeMayBay v WHERE v.MaChuyenBay=c.MaChuyenBay AND v.TrangThaiCode='Cancelled') TicketsCancelled,
            (SELECT ISNULL(SUM(v.GiaVe),0) FROM VeMayBay v WHERE v.MaChuyenBay=c.MaChuyenBay AND v.TrangThaiCode<>'Cancelled') TicketValue
            FROM ChuyenBay c JOIN HangBay h ON h.MaHangBay=c.MaHangBay ";

        const string TicketSelect = @"SELECT v.MaVe Id, ISNULL(v.MaVeCode,'TH-F'+CAST(v.MaVe AS varchar(10))) Code, v.MaKH CustomerId, kh.HoTen CustomerName, kh.Email CustomerEmail,
            v.HoTenHanhKhach Passenger, v.SoGiayTo DocNo, v.SoGhe Seat, ISNULL(v.HangGhe,N'Phổ thông') Class, ISNULL(v.GiaVe,c.GiaVe) Price, v.TrangThai Status, ISNULL(v.TrangThaiCode,'Booked') StatusCode,
            v.NgayDat BookedAt, v.NgayCapNhat UpdatedAt, v.LyDoHuy CancelReason, v.LaDuLieuMau IsSample, ISNULL(v.EmailLienHe,kh.Email) ContactEmail, ISNULL(v.SdtLienHe,kh.SoDienThoai) ContactPhone, v.HanhLyKg BaggageKg,
            c.MaChuyenBay FlightId, c.MaChuyen FlightCode, c.TrangThaiChuyen FlightStatus, h.TenHangBay AirlineName, h.MaIATA AirlineIata,
            ISNULL(c.MaSanBayDi,'') FromCode, c.SanBayDi FromName, ISNULL(c.MaSanBayDen,'') ToCode, c.SanBayDen ToName, c.ThoiGianDi Departs, c.ThoiGianDen Arrives, ISNULL(c.LoaiMayBay,N'') Aircraft, ISNULL(c.CuaRa,N'') Gate
            FROM VeMayBay v JOIN ChuyenBay c ON c.MaChuyenBay=v.MaChuyenBay JOIN HangBay h ON h.MaHangBay=c.MaHangBay JOIN KhachHang kh ON kh.MaKH=v.MaKH ";

        static string Like(string s) { return string.IsNullOrWhiteSpace(s) ? null : "%" + s.Trim().Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%"; }

        static DateTime _lastSync = DateTime.MinValue;

        /// <summary>Tự chuyển vòng đời: chuyến đã hạ cánh → "Đã hạ cánh"; vé còn hiệu lực của chuyến đó → "Đã bay". Chạy tối đa 1 lần/phút.</summary>
        public static void SyncLifecycle()
        {
            if ((DateTime.UtcNow - _lastSync).TotalSeconds < 60) return;
            _lastSync = DateTime.UtcNow;
            using (var c = TravelDb.Open())
            {
                c.Execute("UPDATE ChuyenBay SET TrangThaiChuyen='Arrived' WHERE TrangThaiChuyen IN ('Scheduled','Departed','Delayed') AND ThoiGianDen < GETDATE()");
                c.Execute("UPDATE ChuyenBay SET TrangThaiChuyen='Departed' WHERE TrangThaiChuyen IN ('Scheduled') AND ThoiGianDi < GETDATE() AND ThoiGianDen >= GETDATE()");
                c.Execute(@"UPDATE v SET TrangThai=N'Đã bay', TrangThaiCode='Completed', NgayCapNhat=GETDATE() FROM VeMayBay v JOIN ChuyenBay f ON f.MaChuyenBay=v.MaChuyenBay
                    WHERE f.TrangThaiChuyen='Arrived' AND ISNULL(v.TrangThaiCode,'Booked') IN ('Booked','CheckedIn')");
            }
        }

        public static List<FlightRow> Flights(FlightFilter f, out int total, int skip, int take)
        {
            using (var c = TravelDb.Open())
            {
                var where = @"WHERE (@q IS NULL OR c.MaChuyen LIKE @q OR c.SanBayDi LIKE @q COLLATE Vietnamese_100_CI_AI OR c.SanBayDen LIKE @q COLLATE Vietnamese_100_CI_AI OR c.MaSanBayDi LIKE @q OR c.MaSanBayDen LIKE @q)
                    AND (@air IS NULL OR h.MaIATA=@air) AND (@st IS NULL OR c.TrangThaiChuyen=@st)
                    AND (@w IS NULL OR (@w='upcoming' AND c.ThoiGianDi>=GETDATE()) OR (@w='past' AND c.ThoiGianDi<GETDATE()) OR (@w='today' AND CAST(c.ThoiGianDi AS date)=CAST(GETDATE() AS date)))";
                var p = new DynamicParameters(new { q = Like(f.q), air = string.IsNullOrEmpty(f.airline) ? null : f.airline, st = string.IsNullOrEmpty(f.status) ? null : f.status, w = string.IsNullOrEmpty(f.when) ? null : f.when, skip, take });
                total = c.ExecuteScalar<int>("SELECT COUNT(*) FROM ChuyenBay c JOIN HangBay h ON h.MaHangBay=c.MaHangBay " + where, p);
                return c.Query<FlightRow>(FlightSelect + where + " ORDER BY c.ThoiGianDi DESC OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY", p).ToList();
            }
        }

        public static FlightRow Flight(int id)
        {
            using (var c = TravelDb.Open()) return c.QueryFirstOrDefault<FlightRow>(FlightSelect + "WHERE c.MaChuyenBay=@id", new { id });
        }

        public static List<FlightRow> Upcoming(int take)
        {
            using (var c = TravelDb.Open()) return c.Query<FlightRow>("SELECT TOP (@take) * FROM (" + FlightSelect + "WHERE c.ThoiGianDi>=GETDATE() AND c.TrangThaiChuyen<>'Cancelled') x ORDER BY Departs", new { take }).ToList();
        }

        public static List<TicketRow> Tickets(TicketFilter f, out int total, int skip, int take)
        {
            using (var c = TravelDb.Open())
            {
                var where = @"WHERE (@q IS NULL OR v.MaVeCode LIKE @q OR v.HoTenHanhKhach LIKE @q COLLATE Vietnamese_100_CI_AI OR v.SoGiayTo LIKE @q OR kh.Email LIKE @q OR c.MaChuyen LIKE @q)
                    AND (@st IS NULL OR v.TrangThaiCode=@st) AND (@air IS NULL OR h.MaIATA=@air) AND (@fl IS NULL OR v.MaChuyenBay=@fl) AND (@kh IS NULL OR v.MaKH=@kh)
                    AND (@from IS NULL OR c.ThoiGianDi>=@from) AND (@to IS NULL OR c.ThoiGianDi<DATEADD(day,1,@to))";
                var p = new DynamicParameters(new { q = Like(f.q), st = string.IsNullOrEmpty(f.status) ? null : f.status, air = string.IsNullOrEmpty(f.airline) ? null : f.airline, fl = f.flight, kh = f.customer, from = f.from, to = f.to, skip, take });
                total = c.ExecuteScalar<int>("SELECT COUNT(*) FROM VeMayBay v JOIN ChuyenBay c ON c.MaChuyenBay=v.MaChuyenBay JOIN HangBay h ON h.MaHangBay=c.MaHangBay JOIN KhachHang kh ON kh.MaKH=v.MaKH " + where, p);
                return c.Query<TicketRow>(TicketSelect + where + " ORDER BY v.NgayDat DESC, v.MaVe DESC OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY", p).ToList();
            }
        }

        public static TicketRow Ticket(int id)
        {
            using (var c = TravelDb.Open()) return c.QueryFirstOrDefault<TicketRow>(TicketSelect + "WHERE v.MaVe=@id", new { id });
        }

        public static List<TicketRow> TicketsOfFlight(int flightId)
        {
            using (var c = TravelDb.Open()) return c.Query<TicketRow>(TicketSelect + "WHERE v.MaChuyenBay=@flightId ORDER BY v.SoGhe", new { flightId }).ToList();
        }

        public static List<TicketRow> TicketsOfCustomer(int customerId)
        {
            using (var c = TravelDb.Open()) return c.Query<TicketRow>(TicketSelect + "WHERE v.MaKH=@customerId ORDER BY c.ThoiGianDi DESC", new { customerId }).ToList();
        }

        /// <summary>Hủy vé: giữ lịch sử (không xóa), hoàn lại ghế nếu vé còn hiệu lực và chuyến chưa khởi hành.</summary>
        public static bool CancelTicket(int id, string reason)
        {
            using (var c = TravelDb.Open())
            using (var tx = c.BeginTransaction())
            {
                var t = c.QueryFirstOrDefault(@"SELECT v.TrangThaiCode, v.MaChuyenBay, f.ThoiGianDi FROM VeMayBay v WITH (UPDLOCK) JOIN ChuyenBay f ON f.MaChuyenBay=v.MaChuyenBay WHERE v.MaVe=@id", new { id }, tx);
                if (t == null || (string)(t.TrangThaiCode ?? "Booked") == "Cancelled" || (string)(t.TrangThaiCode ?? "Booked") == "Completed") return false;
                c.Execute("UPDATE VeMayBay SET TrangThai=N'Đã hủy', TrangThaiCode='Cancelled', LyDoHuy=@r, NgayCapNhat=GETDATE() WHERE MaVe=@id", new { r = reason, id }, tx);
                if ((DateTime)t.ThoiGianDi > DateTime.Now)
                    c.Execute("UPDATE ChuyenBay SET SoGheConLai = CASE WHEN SoGheConLai < SoGhe THEN SoGheConLai + 1 ELSE SoGheConLai END WHERE MaChuyenBay=@f", new { f = (int)t.MaChuyenBay }, tx);
                tx.Commit();
                return true;
            }
        }

        public static bool CheckIn(int id)
        {
            using (var c = TravelDb.Open())
                return c.Execute("UPDATE VeMayBay SET TrangThai=N'Đã làm thủ tục', TrangThaiCode='CheckedIn', NgayCapNhat=GETDATE() WHERE MaVe=@id AND ISNULL(TrangThaiCode,'Booked')='Booked'", new { id }) == 1;
        }

        /// <summary>Đổi trạng thái chuyến. Hủy chuyến sẽ hủy luôn các vé còn hiệu lực (kèm lý do) trong cùng giao dịch.</summary>
        public static int SetFlightStatus(int id, string status, string note)
        {
            if (!FlightInfo.Statuses.Contains(status)) throw new ArgumentException("status");
            using (var c = TravelDb.Open())
            using (var tx = c.BeginTransaction())
            {
                int affected = 0;
                c.Execute("UPDATE ChuyenBay SET TrangThaiChuyen=@s, GhiChu=CASE WHEN @n IS NULL OR @n='' THEN GhiChu ELSE @n END WHERE MaChuyenBay=@id", new { s = status, n = note, id }, tx);
                if (status == "Cancelled")
                    affected = c.Execute("UPDATE VeMayBay SET TrangThai=N'Đã hủy', TrangThaiCode='Cancelled', LyDoHuy=ISNULL(NULLIF(@n,''),N'Hãng hủy chuyến'), NgayCapNhat=GETDATE() WHERE MaChuyenBay=@id AND ISNULL(TrangThaiCode,'Booked') IN ('Booked','CheckedIn')", new { n = note, id }, tx);
                tx.Commit();
                return affected;
            }
        }
    }

    // ───────────────────────── Hồ sơ khách 360° ─────────────────────────

    public class CustomerListRow
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime Joined { get; set; }
        public int? AccountId { get; set; }
        public string UserName { get; set; }
        public bool? AccountActive { get; set; }
        public int Bookings { get; set; }
        public decimal HotelSpend { get; set; }
        public int Tickets { get; set; }
        public decimal TicketSpend { get; set; }
        public DateTime? LastActivity { get; set; }
        public decimal Lifetime { get { return HotelSpend + TicketSpend; } }
    }

    public class CustomerBooking
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public int HotelId { get; set; }
        public string Hotel { get; set; }
        public string City { get; set; }
        public string Rooms { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int Nights { get; set; }
        public int Adults { get; set; }
        public int Children { get; set; }
        public decimal Gross { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public decimal Paid { get; set; }
        public string Method { get; set; }
        public string Promo { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CancelReason { get; set; }
        public string Note { get; set; }
    }

    public class CustomerPayment { public int Id { get; set; } public int BookingId { get; set; } public string BookingCode { get; set; } public decimal Amount { get; set; } public string Method { get; set; } public string Status { get; set; } public string Reference { get; set; } public DateTime At { get; set; } }

    public class CustomerReview { public int Id { get; set; } public string Hotel { get; set; } public int Stars { get; set; } public string Content { get; set; } public DateTime At { get; set; } public bool Visible { get; set; } public bool IsSample { get; set; } }

    public class CustomerFavorite { public int HotelId { get; set; } public string Hotel { get; set; } public string City { get; set; } public int Stars { get; set; } public DateTime At { get; set; } }

    public class CustomerEvent { public DateTime At { get; set; } public string Kind { get; set; } public string Title { get; set; } public string Detail { get; set; } public string Url { get; set; } }

    public class BehaviorRow { public DateTime At { get; set; } public string Type { get; set; } public string Target { get; set; } public int? Duration { get; set; } }

    public class Customer360
    {
        public dynamic Profile;
        public int BookingCount, PaidBookings, CancelledBookings, TicketCount, TicketsCancelled, FavoriteCount, ReviewCount;
        public decimal HotelSpend, Refunded, TicketSpend, Collected;
        public double AvgStars;
        public DateTime? FirstAt, LastAt;
        public List<CustomerBooking> Bookings = new List<CustomerBooking>();
        public List<TicketRow> Tickets = new List<TicketRow>();
        public List<CustomerPayment> Payments = new List<CustomerPayment>();
        public List<CustomerReview> Reviews = new List<CustomerReview>();
        public List<CustomerFavorite> Favorites = new List<CustomerFavorite>();
        public List<BehaviorRow> Behavior = new List<BehaviorRow>();
        public List<CustomerEvent> Timeline = new List<CustomerEvent>();
        public List<KeyValuePair<string, int>> TopCities = new List<KeyValuePair<string, int>>();
        public decimal Lifetime { get { return HotelSpend + TicketSpend; } }
    }

    public static class CustomerAdmin
    {
        const string PaidSql = "N'Thành công', N'Đã thanh toán'";

        public static List<CustomerListRow> List(string q, string sort, int skip, int take, out int total)
        {
            using (var c = TravelDb.Open())
            {
                string like = string.IsNullOrWhiteSpace(q) ? null : "%" + q.Trim().Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%";
                const string from = @"FROM KhachHang kh LEFT JOIN TaiKhoan tk ON tk.MaKH=kh.MaKH
                    OUTER APPLY (SELECT COUNT(*) N, ISNULL(SUM(CASE WHEN o.TrangThai NOT IN (N'Đã hủy',N'Hết hạn',N'Chờ thanh toán') THEN o.ThanhTien END),0) S, MAX(o.NgayDat) L FROM DonDatPhong o WHERE o.MaKH=kh.MaKH) b
                    OUTER APPLY (SELECT COUNT(*) N, ISNULL(SUM(CASE WHEN ISNULL(v.TrangThaiCode,'Booked')<>'Cancelled' THEN v.GiaVe END),0) S, MAX(v.NgayDat) L FROM VeMayBay v WHERE v.MaKH=kh.MaKH) t
                    WHERE (@q IS NULL OR kh.HoTen LIKE @q COLLATE Vietnamese_100_CI_AI OR kh.Email LIKE @q OR kh.SoDienThoai LIKE @q OR tk.TenDangNhap LIKE @q)";
                string order = sort == "spend" ? "(b.S + t.S) DESC" : sort == "bookings" ? "(b.N + t.N) DESC" : sort == "new" ? "kh.NgayTao DESC" : "(CASE WHEN b.L > ISNULL(t.L,'1900-01-01') THEN b.L ELSE t.L END) DESC, kh.MaKH";
                total = c.ExecuteScalar<int>("SELECT COUNT(*) " + from, new { q = like });
                return c.Query<CustomerListRow>(@"SELECT kh.MaKH Id, kh.HoTen Name, kh.Email, kh.SoDienThoai Phone, kh.NgayTao Joined, tk.MaTaiKhoan AccountId, tk.TenDangNhap UserName, tk.TrangThai AccountActive,
                    b.N Bookings, b.S HotelSpend, t.N Tickets, t.S TicketSpend, CASE WHEN b.L > ISNULL(t.L,'1900-01-01') THEN b.L ELSE t.L END LastActivity " + from + " ORDER BY " + order + " OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY", new { q = like, skip, take }).ToList();
            }
        }

        public static Customer360 Load(int id)
        {
            using (var c = TravelDb.Open())
            {
                var m = new Customer360();
                m.Profile = c.QueryFirstOrDefault(@"SELECT kh.MaKH, kh.HoTen, kh.Email, kh.SoDienThoai, kh.NgaySinh, kh.GioiTinh, kh.HinhAnh, kh.NgayTao, kh.TrangThai,
                    tk.MaTaiKhoan AccountId, tk.TenDangNhap UserName, tk.TrangThai AccountActive, tk.EmailXacThuc EmailVerified, tk.LanDangNhapCuoi LastLogin, tk.KhoaDen LockedUntil, tk.SoLanDangNhapSai FailedLogins, vt.MaVaiTroCode Role, vt.TenVaiTro RoleName
                    FROM KhachHang kh LEFT JOIN TaiKhoan tk ON tk.MaKH=kh.MaKH LEFT JOIN VaiTro vt ON vt.MaVaiTro=tk.MaVaiTro WHERE kh.MaKH=@id", new { id });
                if (m.Profile == null) return null;

                m.Bookings = c.Query<CustomerBooking>(@"SELECT o.MaDon Id, o.MaDonCode Code, ks.MaKS HotelId, ks.TenKS Hotel, ISNULL(dd.TenDiaDiem,N'') City,
                    ISNULL((SELECT STRING_AGG(CAST(p.TenPhong AS nvarchar(max)) + N' ×' + CAST(ct.SoLuong AS nvarchar(10)), N', ') FROM ChiTietDatPhong ct JOIN Phong p ON p.MaPhong=ct.MaPhong WHERE ct.MaDon=o.MaDon), N'') Rooms,
                    o.NgayNhanPhong CheckIn, o.NgayTraPhong CheckOut, DATEDIFF(day,o.NgayNhanPhong,o.NgayTraPhong) Nights, o.SoNguoiLon Adults, o.SoTreEm Children, o.TongTien Gross, o.GiamGia Discount, o.ThanhTien Total,
                    (SELECT ISNULL(SUM(t.SoTien),0) FROM ThanhToan t WHERE t.MaDon=o.MaDon AND t.TrangThai IN (" + PaidSql + @")) Paid,
                    (SELECT TOP 1 t.PhuongThuc FROM ThanhToan t WHERE t.MaDon=o.MaDon ORDER BY t.MaThanhToan DESC) Method,
                    (SELECT km.MaCode FROM KhuyenMai km WHERE km.MaKM=o.MaKM) Promo,
                    o.TrangThai Status, o.NgayDat CreatedAt, o.LyDoHuy CancelReason, o.GhiChu Note
                    FROM DonDatPhong o JOIN KhachSan ks ON ks.MaKS=o.MaKS LEFT JOIN DiaDiem dd ON dd.MaDiaDiem=ks.MaDiaDiem WHERE o.MaKH=@id ORDER BY o.NgayDat DESC", new { id }).ToList();

                m.Tickets = FlightAdmin.TicketsOfCustomer(id);

                m.Payments = c.Query<CustomerPayment>(@"SELECT t.MaThanhToan Id, o.MaDon BookingId, o.MaDonCode BookingCode, t.SoTien Amount, t.PhuongThuc Method, t.TrangThai Status, ISNULL(t.MaThamChieuVNPAY, t.MaGiaoDich) Reference, ISNULL(t.NgayThanhToan, t.NgayTao) [At]
                    FROM ThanhToan t JOIN DonDatPhong o ON o.MaDon=t.MaDon WHERE o.MaKH=@id ORDER BY t.MaThanhToan DESC", new { id }).ToList();

                m.Reviews = c.Query<CustomerReview>(@"SELECT d.MaDanhGia Id, ks.TenKS Hotel, d.SoSao Stars, d.NoiDung Content, d.NgayDanhGia [At], d.TrangThai Visible, d.LaDuLieuMau IsSample FROM DanhGia d JOIN KhachSan ks ON ks.MaKS=d.MaKS WHERE d.MaKH=@id ORDER BY d.NgayDanhGia DESC", new { id }).ToList();

                m.Favorites = c.Query<CustomerFavorite>(@"SELECT ks.MaKS HotelId, ks.TenKS Hotel, ISNULL(dd.TenDiaDiem,N'') City, ks.SoSao Stars, y.NgayThem [At] FROM YeuThich y JOIN KhachSan ks ON ks.MaKS=y.MaKS LEFT JOIN DiaDiem dd ON dd.MaDiaDiem=ks.MaDiaDiem WHERE y.MaKH=@id ORDER BY y.NgayThem DESC", new { id }).ToList();

                m.Behavior = c.Query<BehaviorRow>(@"SELECT TOP 40 b.[Timestamp] [At], b.BehaviorType [Type], b.Duration,
                    COALESCE(ks.TenKS, dd.TenDiaDiem, bv.TieuDe, ph.TenPhong, cb.MaChuyen + N' ' + cb.SanBayDi + N' → ' + cb.SanBayDen, b.Metadata) Target
                    FROM UserBehavior b LEFT JOIN KhachSan ks ON ks.MaKS=b.MaKS LEFT JOIN DiaDiem dd ON dd.MaDiaDiem=b.MaDiaDiem LEFT JOIN BaiViet bv ON bv.MaBaiViet=b.MaBaiViet
                    LEFT JOIN Phong ph ON ph.MaPhong=b.MaPhong LEFT JOIN ChuyenBay cb ON cb.MaChuyenBay=b.MaChuyenBay WHERE b.MaKH=@id ORDER BY b.[Timestamp] DESC", new { id }).ToList();

                m.BookingCount = m.Bookings.Count;
                var okStates = new[] { "Đã thanh toán", "Đã nhận phòng", "Hoàn thành" };
                m.PaidBookings = m.Bookings.Count(b => okStates.Contains(b.Status));
                m.CancelledBookings = m.Bookings.Count(b => b.Status == "Đã hủy" || b.Status == "Hết hạn");
                m.HotelSpend = m.Bookings.Where(b => okStates.Contains(b.Status)).Sum(b => b.Total);
                m.Collected = m.Payments.Where(p => p.Status == "Thành công" || p.Status == "Đã thanh toán").Sum(p => p.Amount);
                m.Refunded = c.ExecuteScalar<decimal>("SELECT ISNULL(SUM(r.SoTien),0) FROM YeuCauHoanTien r JOIN DonDatPhong o ON o.MaDon=r.MaDon WHERE o.MaKH=@id AND r.TrangThai='REFUNDED'", new { id });
                m.TicketCount = m.Tickets.Count(t => t.StatusCode != "Cancelled");
                m.TicketsCancelled = m.Tickets.Count(t => t.StatusCode == "Cancelled");
                m.TicketSpend = m.Tickets.Where(t => t.StatusCode != "Cancelled").Sum(t => t.Price);
                m.FavoriteCount = m.Favorites.Count;
                m.ReviewCount = m.Reviews.Count;
                m.AvgStars = m.Reviews.Count == 0 ? 0 : m.Reviews.Average(r => r.Stars);
                m.TopCities = m.Bookings.Where(b => b.City != "" && !(b.Status == "Đã hủy" || b.Status == "Hết hạn")).GroupBy(b => b.City).Select(g => new KeyValuePair<string, int>(g.Key, g.Count())).OrderByDescending(x => x.Value).Take(4).ToList();

                foreach (var b in m.Bookings) m.Timeline.Add(new CustomerEvent { At = b.CreatedAt, Kind = "hotel", Title = "Đặt phòng " + b.Code + " – " + b.Hotel, Detail = b.Nights + " đêm · " + Vnd(b.Total) + " · " + b.Status, Url = "/Admin/Bookings/Details/" + b.Id });
                foreach (var t in m.Tickets) m.Timeline.Add(new CustomerEvent { At = t.BookedAt, Kind = "flight", Title = "Đặt vé " + t.Code + " – " + t.FromCode + " → " + t.ToCode, Detail = t.FlightCode + " · " + t.Departs.ToString("dd/MM/yyyy HH:mm") + " · " + Vnd(t.Price) + " · " + t.Status, Url = "/Admin/Tickets/Details/" + t.Id });
                foreach (var p in m.Payments) m.Timeline.Add(new CustomerEvent { At = p.At, Kind = "pay", Title = "Thanh toán " + p.Method + " – " + p.BookingCode, Detail = Vnd(p.Amount) + " · " + p.Status, Url = "/Admin/Bookings/Details/" + p.BookingId });
                foreach (var r in m.Reviews) m.Timeline.Add(new CustomerEvent { At = r.At, Kind = "review", Title = "Đánh giá " + r.Stars + "★ – " + r.Hotel, Detail = r.Content, Url = null });
                m.Timeline = m.Timeline.OrderByDescending(e => e.At).Take(40).ToList();
                var dates = m.Timeline.Select(e => e.At).ToList(); dates.Add((DateTime)m.Profile.NgayTao);
                m.FirstAt = dates.Min(); m.LastAt = m.Timeline.Count == 0 ? (DateTime?)null : m.Timeline.Max(e => e.At);
                return m;
            }
        }

        public static int? CustomerIdOfAccount(int accountId)
        {
            using (var c = TravelDb.Open()) return c.ExecuteScalar<int?>("SELECT MaKH FROM TaiKhoan WHERE MaTaiKhoan=@accountId", new { accountId });
        }

        static string Vnd(decimal v) { return TravelHub.Infrastructure.Ui.MoneyText(v); }
    }

    // ───────────────────────── Dashboard ─────────────────────────

    public class NamedValue { public string Name { get; set; } public decimal Value { get; set; } public int Count { get; set; } public string Extra { get; set; } }

    public class DashRecentBooking { public int Id { get; set; } public string Code { get; set; } public string Customer { get; set; } public string Hotel { get; set; } public decimal Total { get; set; } public string Status { get; set; } public DateTime At { get; set; } public int Nights { get; set; } }

    public class TodoItem { public string Level, Text, Url; public int Count; }

    public class AdminDashboard
    {
        public int Days;
        public decimal Revenue, RevenuePrev, Refunded, BookingValue, PendingValue;
        public int Bookings, BookingsPrev, Customers, NewCustomers, NewCustomersPrev, Hotels, Rooms;
        public int TicketsActive, TicketsPrev, TicketsCancelled, FlightsUpcoming, FlightsToday, FlightsDelayed, TicketsDepartingSoon;
        public decimal TicketValue;
        public double AvgLoad;
        public List<KeyValuePair<DateTime, decimal>> DailyRevenue = new List<KeyValuePair<DateTime, decimal>>();
        public List<KeyValuePair<DateTime, int>> DailyBookings = new List<KeyValuePair<DateTime, int>>();
        public List<KeyValuePair<DateTime, int>> DailyTickets = new List<KeyValuePair<DateTime, int>>();
        public List<KeyValuePair<string, int>> BookingStatus = new List<KeyValuePair<string, int>>();
        public List<KeyValuePair<string, int>> TicketStatus = new List<KeyValuePair<string, int>>();
        public List<NamedValue> TopHotels = new List<NamedValue>();
        public List<NamedValue> TopDestinations = new List<NamedValue>();
        public List<NamedValue> TopRoutes = new List<NamedValue>();
        public List<NamedValue> TopCustomers = new List<NamedValue>();
        public List<NamedValue> Airlines = new List<NamedValue>();
        public List<FlightRow> Upcoming = new List<FlightRow>();
        public List<TicketRow> RecentTickets = new List<TicketRow>();
        public List<DashRecentBooking> RecentBookings = new List<DashRecentBooking>();
        public List<TodoItem> Todo = new List<TodoItem>();
    }

    public static class DashboardService
    {
        public static AdminDashboard Load(int days)
        {
            var d = new AdminDashboard { Days = days };
            FlightAdmin.SyncLifecycle();
            using (var c = TravelDb.Open())
            {
                var from = DateTime.Today.AddDays(-(days - 1));
                var prevFrom = from.AddDays(-days);
                const string paid = "N'Thành công', N'Đã thanh toán'";
                d.Customers = c.ExecuteScalar<int>("SELECT COUNT(*) FROM KhachHang");
                d.NewCustomers = c.ExecuteScalar<int>("SELECT COUNT(*) FROM KhachHang WHERE NgayTao>=@from", new { from });
                d.NewCustomersPrev = c.ExecuteScalar<int>("SELECT COUNT(*) FROM KhachHang WHERE NgayTao>=@pf AND NgayTao<@from", new { from, pf = prevFrom });
                d.Hotels = c.ExecuteScalar<int>("SELECT COUNT(*) FROM KhachSan WHERE TrangThai=1");
                d.Rooms = c.ExecuteScalar<int>("SELECT COUNT(*) FROM Phong WHERE TrangThai=1");

                var collected = c.ExecuteScalar<decimal>("SELECT ISNULL(SUM(SoTien),0) FROM ThanhToan WHERE TrangThai IN (" + paid + ") AND NgayThanhToan>=@from", new { from });
                d.Refunded = c.ExecuteScalar<decimal>("SELECT ISNULL(SUM(SoTien),0) FROM YeuCauHoanTien WHERE TrangThai='REFUNDED' AND NgayXuLy>=@from", new { from });
                d.Revenue = collected - d.Refunded;
                d.RevenuePrev = c.ExecuteScalar<decimal>("SELECT ISNULL(SUM(SoTien),0) FROM ThanhToan WHERE TrangThai IN (" + paid + ") AND NgayThanhToan>=@pf AND NgayThanhToan<@from", new { from, pf = prevFrom });
                d.Bookings = c.ExecuteScalar<int>("SELECT COUNT(*) FROM DonDatPhong WHERE NgayDat>=@from", new { from });
                d.BookingsPrev = c.ExecuteScalar<int>("SELECT COUNT(*) FROM DonDatPhong WHERE NgayDat>=@pf AND NgayDat<@from", new { from, pf = prevFrom });
                d.BookingValue = c.ExecuteScalar<decimal>("SELECT ISNULL(SUM(ThanhTien),0) FROM DonDatPhong WHERE TrangThai NOT IN (N'Đã hủy', N'Hết hạn') AND NgayDat>=@from", new { from });
                d.PendingValue = c.ExecuteScalar<decimal>("SELECT ISNULL(SUM(ThanhTien),0) FROM DonDatPhong WHERE TrangThai IN (N'Chờ thanh toán', N'Chờ xác nhận') AND (NgayHetHan IS NULL OR NgayHetHan>GETDATE())");

                d.TicketsActive = c.ExecuteScalar<int>("SELECT COUNT(*) FROM VeMayBay WHERE ISNULL(TrangThaiCode,'Booked')<>'Cancelled' AND NgayDat>=@from", new { from });
                d.TicketsPrev = c.ExecuteScalar<int>("SELECT COUNT(*) FROM VeMayBay WHERE ISNULL(TrangThaiCode,'Booked')<>'Cancelled' AND NgayDat>=@pf AND NgayDat<@from", new { from, pf = prevFrom });
                d.TicketsCancelled = c.ExecuteScalar<int>("SELECT COUNT(*) FROM VeMayBay WHERE TrangThaiCode='Cancelled' AND NgayDat>=@from", new { from });
                d.TicketValue = c.ExecuteScalar<decimal>("SELECT ISNULL(SUM(GiaVe),0) FROM VeMayBay WHERE ISNULL(TrangThaiCode,'Booked')<>'Cancelled' AND NgayDat>=@from", new { from });
                d.FlightsUpcoming = c.ExecuteScalar<int>("SELECT COUNT(*) FROM ChuyenBay WHERE ThoiGianDi>=GETDATE() AND TrangThaiChuyen<>'Cancelled'");
                d.FlightsToday = c.ExecuteScalar<int>("SELECT COUNT(*) FROM ChuyenBay WHERE CAST(ThoiGianDi AS date)=CAST(GETDATE() AS date)");
                d.FlightsDelayed = c.ExecuteScalar<int>("SELECT COUNT(*) FROM ChuyenBay WHERE TrangThaiChuyen='Delayed' AND ThoiGianDi>=DATEADD(day,-1,GETDATE())");
                d.TicketsDepartingSoon = c.ExecuteScalar<int>("SELECT COUNT(*) FROM VeMayBay v JOIN ChuyenBay f ON f.MaChuyenBay=v.MaChuyenBay WHERE ISNULL(v.TrangThaiCode,'Booked')='Booked' AND f.ThoiGianDi BETWEEN GETDATE() AND DATEADD(day,3,GETDATE())");
                d.AvgLoad = c.ExecuteScalar<double?>("SELECT AVG(100.0*(SoGhe-SoGheConLai)/NULLIF(SoGhe,0)) FROM ChuyenBay WHERE ThoiGianDi>=GETDATE() AND TrangThaiChuyen<>'Cancelled'") ?? 0;

                // chuỗi theo ngày
                var rev = c.Query("SELECT CAST(NgayThanhToan AS date) D, SUM(SoTien) V FROM ThanhToan WHERE TrangThai IN (" + paid + ") AND NgayThanhToan>=@from GROUP BY CAST(NgayThanhToan AS date)", new { from }).ToDictionary(x => (DateTime)x.D, x => (decimal)x.V);
                var bk = c.Query("SELECT CAST(NgayDat AS date) D, COUNT(*) V FROM DonDatPhong WHERE NgayDat>=@from GROUP BY CAST(NgayDat AS date)", new { from }).ToDictionary(x => (DateTime)x.D, x => (int)x.V);
                var tk = c.Query("SELECT CAST(NgayDat AS date) D, COUNT(*) V FROM VeMayBay WHERE NgayDat>=@from AND ISNULL(TrangThaiCode,'Booked')<>'Cancelled' GROUP BY CAST(NgayDat AS date)", new { from }).ToDictionary(x => (DateTime)x.D, x => (int)x.V);
                for (int i = 0; i < days; i++)
                {
                    var day = from.AddDays(i);
                    d.DailyRevenue.Add(new KeyValuePair<DateTime, decimal>(day, rev.ContainsKey(day) ? rev[day] : 0));
                    d.DailyBookings.Add(new KeyValuePair<DateTime, int>(day, bk.ContainsKey(day) ? bk[day] : 0));
                    d.DailyTickets.Add(new KeyValuePair<DateTime, int>(day, tk.ContainsKey(day) ? tk[day] : 0));
                }

                d.BookingStatus = c.Query("SELECT TrangThai S, COUNT(*) N FROM DonDatPhong GROUP BY TrangThai ORDER BY COUNT(*) DESC").Select(x => new KeyValuePair<string, int>((string)x.S, (int)x.N)).ToList();
                d.TicketStatus = c.Query("SELECT TrangThai S, COUNT(*) N FROM VeMayBay GROUP BY TrangThai ORDER BY COUNT(*) DESC").Select(x => new KeyValuePair<string, int>((string)x.S, (int)x.N)).ToList();

                d.TopHotels = c.Query<NamedValue>(@"SELECT TOP 5 ks.TenKS Name, SUM(o.ThanhTien) Value, COUNT(*) [Count], ISNULL(dd.TenDiaDiem,N'') Extra FROM DonDatPhong o JOIN KhachSan ks ON ks.MaKS=o.MaKS LEFT JOIN DiaDiem dd ON dd.MaDiaDiem=ks.MaDiaDiem
                    WHERE o.TrangThai NOT IN (N'Đã hủy', N'Hết hạn') GROUP BY ks.TenKS, dd.TenDiaDiem ORDER BY SUM(o.ThanhTien) DESC").ToList();
                d.TopDestinations = c.Query<NamedValue>(@"SELECT TOP 5 dd.TenDiaDiem Name, SUM(o.ThanhTien) Value, COUNT(*) [Count] FROM DonDatPhong o JOIN KhachSan ks ON ks.MaKS=o.MaKS JOIN DiaDiem dd ON dd.MaDiaDiem=ks.MaDiaDiem
                    WHERE o.TrangThai NOT IN (N'Đã hủy', N'Hết hạn') GROUP BY dd.TenDiaDiem ORDER BY SUM(o.ThanhTien) DESC").ToList();
                d.TopRoutes = c.Query<NamedValue>(@"SELECT TOP 5 ISNULL(f.MaSanBayDi,'') + N' → ' + ISNULL(f.MaSanBayDen,'') Name, SUM(v.GiaVe) Value, COUNT(*) [Count], f.SanBayDi + N' → ' + f.SanBayDen Extra FROM VeMayBay v JOIN ChuyenBay f ON f.MaChuyenBay=v.MaChuyenBay
                    WHERE ISNULL(v.TrangThaiCode,'Booked')<>'Cancelled' GROUP BY f.MaSanBayDi, f.MaSanBayDen, f.SanBayDi, f.SanBayDen ORDER BY COUNT(*) DESC, SUM(v.GiaVe) DESC").ToList();
                d.Airlines = c.Query<NamedValue>(@"SELECT h.TenHangBay Name, ISNULL(SUM(CASE WHEN ISNULL(v.TrangThaiCode,'Booked')<>'Cancelled' THEN v.GiaVe END),0) Value, COUNT(v.MaVe) [Count], h.MaIATA Extra FROM HangBay h LEFT JOIN ChuyenBay f ON f.MaHangBay=h.MaHangBay LEFT JOIN VeMayBay v ON v.MaChuyenBay=f.MaChuyenBay
                    WHERE h.TrangThai=1 GROUP BY h.TenHangBay, h.MaIATA ORDER BY COUNT(v.MaVe) DESC, h.TenHangBay").ToList();
                d.TopCustomers = c.Query<NamedValue>(@"SELECT TOP 5 CAST(kh.MaKH AS varchar(10)) Extra, kh.HoTen Name, ISNULL(b.S,0) + ISNULL(t.S,0) Value, ISNULL(b.N,0) + ISNULL(t.N,0) [Count] FROM KhachHang kh
                    OUTER APPLY (SELECT SUM(o.ThanhTien) S, COUNT(*) N FROM DonDatPhong o WHERE o.MaKH=kh.MaKH AND o.TrangThai NOT IN (N'Đã hủy', N'Hết hạn', N'Chờ thanh toán')) b
                    OUTER APPLY (SELECT SUM(v.GiaVe) S, COUNT(*) N FROM VeMayBay v WHERE v.MaKH=kh.MaKH AND ISNULL(v.TrangThaiCode,'Booked')<>'Cancelled') t
                    ORDER BY ISNULL(b.S,0) + ISNULL(t.S,0) DESC").Where(x => x.Value > 0).ToList();

                d.RecentBookings = c.Query<DashRecentBooking>(@"SELECT TOP 6 o.MaDon Id, o.MaDonCode Code, ISNULL(o.HoTenKhach, kh.HoTen) Customer, ks.TenKS Hotel, o.ThanhTien Total, o.TrangThai Status, o.NgayDat [At], DATEDIFF(day,o.NgayNhanPhong,o.NgayTraPhong) Nights
                    FROM DonDatPhong o JOIN KhachSan ks ON ks.MaKS=o.MaKS JOIN KhachHang kh ON kh.MaKH=o.MaKH ORDER BY o.NgayDat DESC").ToList();
            }
            d.Upcoming = FlightAdmin.Upcoming(6);
            int tt; d.RecentTickets = FlightAdmin.Tickets(new TicketFilter(), out tt, 0, 6);

            var bd = MenuService.Admin();
            using (var c = TravelDb.Open())
            {
                int lowRev = c.ExecuteScalar<int>("SELECT COUNT(*) FROM DanhGia WHERE TrangThai=1 AND SoSao<=2");
                int expiring = c.ExecuteScalar<int>("SELECT COUNT(*) FROM DonDatPhong WHERE TrangThai=N'Chờ thanh toán' AND NgayHetHan>GETDATE() AND NgayHetHan<DATEADD(hour,1,GETDATE())");
                if (bd.RefundsOpen > 0) d.Todo.Add(new TodoItem { Level = "bad", Count = bd.RefundsOpen, Text = "yêu cầu hoàn tiền cần xử lý", Url = "/Admin/Refunds" });
                if (bd.Reconcile > 0) d.Todo.Add(new TodoItem { Level = "bad", Count = bd.Reconcile, Text = "giao dịch cần đối soát", Url = "/Admin/Refunds" });
                if (bd.NewContacts > 0) d.Todo.Add(new TodoItem { Level = "warn", Count = bd.NewContacts, Text = "liên hệ mới chưa phản hồi", Url = "/Admin/Inbox" });
                if (expiring > 0) d.Todo.Add(new TodoItem { Level = "warn", Count = expiring, Text = "đơn sắp hết hạn giữ phòng (≤1 giờ)", Url = "/Admin/Bookings?status=" + Uri.EscapeDataString("Chờ thanh toán") });
                if (d.FlightsDelayed > 0) d.Todo.Add(new TodoItem { Level = "warn", Count = d.FlightsDelayed, Text = "chuyến bay đang chậm", Url = "/Admin/Flights?status=Delayed" });
                if (d.TicketsDepartingSoon > 0) d.Todo.Add(new TodoItem { Level = "info", Count = d.TicketsDepartingSoon, Text = "vé có chuyến bay trong 3 ngày tới", Url = "/Admin/Tickets?status=Booked" });
                if (lowRev > 0) d.Todo.Add(new TodoItem { Level = "info", Count = lowRev, Text = "đánh giá ≤ 2 sao nên xem lại", Url = "/Admin/Reviews?stars=2" });
            }
            return d;
        }
    }
}
