using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Dapper;
using TravelHub.Data;
using TravelHub.Infrastructure;
using TravelHub.Services;

namespace TravelHub.Areas.Admin.Controllers
{
    public class FlightEditModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Chọn hãng bay")] public int AirlineId { get; set; }
        [Required(ErrorMessage = "Nhập mã chuyến"), StringLength(20), RegularExpression(@"^[A-Za-z0-9]{3,10}$", ErrorMessage = "Mã chuyến gồm chữ và số (VD: VN123)")] public string Code { get; set; }
        [Required(ErrorMessage = "Nhập mã sân bay đi"), RegularExpression(@"^[A-Za-z]{3}$", ErrorMessage = "Mã IATA gồm 3 chữ cái")] public string FromCode { get; set; }
        [Required(ErrorMessage = "Nhập nơi đi"), StringLength(100)] public string FromName { get; set; }
        [Required(ErrorMessage = "Nhập mã sân bay đến"), RegularExpression(@"^[A-Za-z]{3}$", ErrorMessage = "Mã IATA gồm 3 chữ cái")] public string ToCode { get; set; }
        [Required(ErrorMessage = "Nhập nơi đến"), StringLength(100)] public string ToName { get; set; }
        [Required(ErrorMessage = "Chọn giờ khởi hành")] public DateTime? Departs { get; set; }
        [Required(ErrorMessage = "Chọn giờ đến")] public DateTime? Arrives { get; set; }
        [Range(1, 100000000, ErrorMessage = "Giá vé không hợp lệ")] public decimal Price { get; set; }
        public decimal? BusinessPrice { get; set; }
        [Range(1, 900, ErrorMessage = "Số ghế 1–900")] public int Seats { get; set; }
        public int SeatsLeft { get; set; }
        [StringLength(60)] public string Aircraft { get; set; }
        [StringLength(10)] public string Gate { get; set; }
        [Range(0, 60)] public int BaggageKg { get; set; }
        [StringLength(300)] public string Note { get; set; }
        public bool Active { get; set; }
    }

    [RoleAuthorize(Roles = "Admin,Staff")]
    public class FlightsController : AdminController
    {
        public ActionResult Index(string q, string airline, string status, string when, int page = 1)
        {
            if (page < 1) page = 1; const int size = 15;
            FlightAdmin.SyncLifecycle(); int total;
            var f = new FlightFilter { q = q, airline = airline, status = status, when = when };
            var rows = FlightAdmin.Flights(f, out total, (page - 1) * size, size);
            using (var c = TravelDb.Open()) ViewBag.Airlines = c.Query("SELECT MaIATA, TenHangBay FROM HangBay ORDER BY TenHangBay").ToList();
            ViewBag.Total = total; ViewBag.Page = page; ViewBag.Size = size; ViewBag.F = f; ViewBag.Title = "Chuyến bay";
            return View(rows);
        }

        public ActionResult Details(int id)
        {
            var f = FlightAdmin.Flight(id);
            if (f == null) return HttpNotFound();
            ViewBag.Tickets = FlightAdmin.TicketsOfFlight(id); ViewBag.Title = "Chuyến " + f.Code;
            return View(f);
        }

        [RoleAuthorize(Roles = "Admin")]
        public ActionResult Create()
        {
            LoadAirlines(); ViewBag.Title = "Thêm chuyến bay";
            var d = DateTime.Today.AddDays(7).AddHours(8);
            return View("Edit", new FlightEditModel { Active = true, Seats = 180, SeatsLeft = 180, BaggageKg = 20, Departs = d, Arrives = d.AddMinutes(90) });
        }

        [RoleAuthorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            var f = FlightAdmin.Flight(id);
            if (f == null) return HttpNotFound();
            LoadAirlines(); ViewBag.Title = "Sửa chuyến " + f.Code; ViewBag.Sold = f.Sold;
            return View(new FlightEditModel { Id = f.Id, AirlineId = f.AirlineId, Code = f.Code, FromCode = f.FromCode, FromName = f.FromName, ToCode = f.ToCode, ToName = f.ToName, Departs = f.Departs, Arrives = f.Arrives, Price = f.Price, BusinessPrice = f.BusinessPrice, Seats = f.Seats, SeatsLeft = f.SeatsLeft, Aircraft = f.Aircraft, Gate = f.Gate, BaggageKg = f.BaggageKg, Note = f.Note, Active = f.Active });
        }

        void LoadAirlines() { using (var c = TravelDb.Open()) ViewBag.Airlines = c.Query("SELECT MaHangBay, TenHangBay, MaIATA FROM HangBay WHERE TrangThai=1 ORDER BY TenHangBay").ToList(); }

        [HttpPost, RoleAuthorize(Roles = "Admin")]
        public ActionResult Save(FlightEditModel m)
        {
            if (m.Departs.HasValue && m.Arrives.HasValue && m.Arrives <= m.Departs) ModelState.AddModelError("Arrives", "Giờ đến phải sau giờ khởi hành.");
            if (m.FromCode != null && m.ToCode != null && string.Equals(m.FromCode, m.ToCode, StringComparison.OrdinalIgnoreCase)) ModelState.AddModelError("ToCode", "Sân bay đến phải khác sân bay đi.");
            if (m.BusinessPrice.HasValue && m.BusinessPrice < m.Price) ModelState.AddModelError("BusinessPrice", "Giá thương gia không được thấp hơn giá phổ thông.");
            using (var c = TravelDb.Open())
            {
                int sold = 0;
                if (m.Id > 0) { var cur = FlightAdmin.Flight(m.Id); if (cur == null) return HttpNotFound(); sold = cur.Sold; }
                if (m.Seats < sold) ModelState.AddModelError("Seats", "Tổng ghế không được nhỏ hơn số ghế đã bán (" + sold + ").");
                if (c.ExecuteScalar<int>("SELECT COUNT(*) FROM ChuyenBay WHERE MaChuyen=@c AND MaChuyenBay<>@id", new { c = m.Code, id = m.Id }) > 0) ModelState.AddModelError("Code", "Mã chuyến đã tồn tại.");
                if (!ModelState.IsValid) { LoadAirlines(); ViewBag.Title = m.Id > 0 ? "Sửa chuyến bay" : "Thêm chuyến bay"; ViewBag.Sold = sold; return View("Edit", m); }
                var left = m.Seats - sold;
                var p = new { m.AirlineId, code = m.Code.Trim().ToUpperInvariant(), fc = m.FromCode.Trim().ToUpperInvariant(), fn = m.FromName.Trim(), tc = m.ToCode.Trim().ToUpperInvariant(), tn = m.ToName.Trim(), dep = m.Departs, arr = m.Arrives, m.Price, m.BusinessPrice, m.Seats, left, plane = m.Aircraft, gate = m.Gate, m.BaggageKg, note = m.Note, m.Active, id = m.Id };
                if (m.Id == 0)
                {
                    m.Id = c.ExecuteScalar<int>(@"INSERT ChuyenBay (MaHangBay, MaChuyen, SanBayDi, SanBayDen, ThoiGianDi, ThoiGianDen, GiaVe, SoGhe, SoGheConLai, TrangThai, MaSanBayDi, MaSanBayDen, LoaiMayBay, CuaRa, HanhLyKg, GiaThuongGia, GhiChu, TrangThaiChuyen)
                        OUTPUT INSERTED.MaChuyenBay VALUES (@AirlineId, @code, @fn, @tn, @dep, @arr, @Price, @Seats, @left, @Active, @fc, @tc, @plane, @gate, @BaggageKg, @BusinessPrice, @note, 'Scheduled')", p);
                    Audit("FLIGHT_CREATE", "ChuyenBay", m.Id.ToString(), p.code);
                }
                else
                {
                    c.Execute(@"UPDATE ChuyenBay SET MaHangBay=@AirlineId, MaChuyen=@code, SanBayDi=@fn, SanBayDen=@tn, ThoiGianDi=@dep, ThoiGianDen=@arr, GiaVe=@Price, SoGhe=@Seats, SoGheConLai=@left, TrangThai=@Active,
                        MaSanBayDi=@fc, MaSanBayDen=@tc, LoaiMayBay=@plane, CuaRa=@gate, HanhLyKg=@BaggageKg, GiaThuongGia=@BusinessPrice, GhiChu=@note WHERE MaChuyenBay=@id", p);
                    Audit("FLIGHT_UPDATE", "ChuyenBay", m.Id.ToString(), p.code);
                }
            }
            Ok("Đã lưu chuyến bay."); return RedirectToAction("Details", new { id = m.Id });
        }

        [HttpPost]
        public ActionResult SetStatus(int id, string status, string note)
        {
            var f = FlightAdmin.Flight(id);
            if (f == null) return HttpNotFound();
            if (!FlightInfo.Statuses.Contains(status)) return new HttpStatusCodeResult(400);
            if (status == "Cancelled" && string.IsNullOrWhiteSpace(note)) { Err("Cần nhập lý do khi hủy chuyến."); return RedirectToAction("Details", new { id }); }
            var n = FlightAdmin.SetFlightStatus(id, status, note);
            Audit("FLIGHT_STATUS", "ChuyenBay", id.ToString(), status + (n > 0 ? " (hủy " + n + " vé)" : ""));
            Ok("Đã cập nhật: " + FlightInfo.StatusText(status) + (n > 0 ? ". Đã hủy " + n + " vé còn hiệu lực — cần liên hệ hành khách để hoàn/đổi vé." : ""));
            return RedirectToAction("Details", new { id });
        }

        [HttpPost, RoleAuthorize(Roles = "Admin")]
        public ActionResult ToggleSale(int id)
        {
            using (var c = TravelDb.Open()) c.Execute("UPDATE ChuyenBay SET TrangThai = 1 - TrangThai WHERE MaChuyenBay=@id", new { id });
            Audit("FLIGHT_TOGGLE", "ChuyenBay", id.ToString(), null); Ok("Đã đổi trạng thái mở bán.");
            return RedirectToAction("Details", new { id });
        }
    }

    [RoleAuthorize(Roles = "Admin,Staff")]
    public class TicketsController : AdminController
    {
        static TicketFilter Parse(string q, string status, string airline, int? flight, int? customer, DateTime? from, DateTime? to) { return new TicketFilter { q = q, status = status, airline = airline, flight = flight, customer = customer, from = from, to = to }; }

        public ActionResult Index(string q, string status, string airline, int? flight, int? customer, DateTime? from, DateTime? to, int page = 1)
        {
            FlightAdmin.SyncLifecycle();
            if (page < 1) page = 1; const int size = 20; int total;
            var f = Parse(q, status, airline, flight, customer, from, to);
            var rows = FlightAdmin.Tickets(f, out total, (page - 1) * size, size);
            using (var c = TravelDb.Open())
            {
                ViewBag.Airlines = c.Query("SELECT MaIATA, TenHangBay FROM HangBay ORDER BY TenHangBay").ToList();
                ViewBag.Sum = c.QueryFirst(@"SELECT COUNT(*) N, SUM(CASE WHEN ISNULL(TrangThaiCode,'Booked')='Booked' THEN 1 ELSE 0 END) B, SUM(CASE WHEN TrangThaiCode='Cancelled' THEN 1 ELSE 0 END) C,
                    ISNULL(SUM(CASE WHEN ISNULL(TrangThaiCode,'Booked')<>'Cancelled' THEN GiaVe END),0) V FROM VeMayBay");
            }
            ViewBag.Total = total; ViewBag.Page = page; ViewBag.Size = size; ViewBag.F = f; ViewBag.Title = "Vé máy bay";
            return View(rows);
        }

        public ActionResult Export(string q, string status, string airline, int? flight, int? customer, DateTime? from, DateTime? to)
        {
            int total; var rows = FlightAdmin.Tickets(Parse(q, status, airline, flight, customer, from, to), out total, 0, 5000);
            var sb = new StringBuilder("﻿Mã vé,Hành khách,Số giấy tờ,Email liên hệ,Chuyến,Hãng,Từ,Đến,Khởi hành,Ghế,Hạng,Giá vé,Trạng thái,Ngày đặt,Dữ liệu mẫu\r\n");
            Func<string, string> qt = s => { s = s ?? ""; if (s.Length > 0 && "=+-@".IndexOf(s[0]) >= 0) s = "'" + s; return "\"" + s.Replace("\"", "\"\"") + "\""; };
            foreach (var t in rows) sb.AppendLine(string.Join(",", qt(t.Code), qt(t.Passenger), qt(t.DocNo), qt(t.ContactEmail), qt(t.FlightCode), qt(t.AirlineName), qt(t.FromCode), qt(t.ToCode), t.Departs.ToString("yyyy-MM-dd HH:mm"), qt(t.Seat), qt(t.Class), t.Price.ToString("0"), qt(t.Status), t.BookedAt.ToString("yyyy-MM-dd HH:mm"), t.IsSample ? "x" : ""));
            Audit("EXPORT_TICKETS", "VeMayBay", null, rows.Count + " dòng");
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "tickets-" + DateTime.Now.ToString("yyyyMMdd-HHmm") + ".csv");
        }

        public ActionResult Details(int id)
        {
            var t = FlightAdmin.Ticket(id);
            if (t == null) return HttpNotFound();
            ViewBag.Title = "Vé " + t.Code;
            return View(t);
        }

        [HttpPost]
        public ActionResult Cancel(int id, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) { Err("Cần nhập lý do hủy vé."); return RedirectToAction("Details", new { id }); }
            if (FlightAdmin.CancelTicket(id, "[Nhân viên] " + reason.Trim())) { Audit("TICKET_CANCEL", "VeMayBay", id.ToString(), reason); Ok("Đã hủy vé và hoàn lại ghế cho chuyến."); }
            else Err("Không thể hủy vé ở trạng thái hiện tại.");
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        public ActionResult CheckIn(int id)
        {
            if (FlightAdmin.CheckIn(id)) { Audit("TICKET_CHECKIN", "VeMayBay", id.ToString(), null); Ok("Đã đánh dấu làm thủ tục."); }
            else Err("Chỉ vé đang ở trạng thái “Đã đặt” mới làm thủ tục được.");
            return RedirectToAction("Details", new { id });
        }
    }

    public class AirlineRow { public int Id { get; set; } public string Name { get; set; } public string Iata { get; set; } public bool Active { get; set; } public int Flights { get; set; } public int Tickets { get; set; } public decimal Value { get; set; } }

    [RoleAuthorize(Roles = "Admin")]
    public class AirlinesController : AdminController
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Hãng bay";
            using (var c = TravelDb.Open())
                return View(c.Query<AirlineRow>(@"SELECT h.MaHangBay Id, h.TenHangBay Name, ISNULL(h.MaIATA,'') Iata, h.TrangThai Active, COUNT(DISTINCT f.MaChuyenBay) Flights, COUNT(v.MaVe) Tickets,
                    ISNULL(SUM(CASE WHEN ISNULL(v.TrangThaiCode,'Booked')<>'Cancelled' THEN v.GiaVe END),0) Value
                    FROM HangBay h LEFT JOIN ChuyenBay f ON f.MaHangBay=h.MaHangBay LEFT JOIN VeMayBay v ON v.MaChuyenBay=f.MaChuyenBay GROUP BY h.MaHangBay, h.TenHangBay, h.MaIATA, h.TrangThai ORDER BY h.TenHangBay").ToList());
        }

        [HttpPost]
        public ActionResult Save(int id, string name, string iata)
        {
            name = (name ?? "").Trim(); iata = (iata ?? "").Trim().ToUpperInvariant();
            if (name.Length < 2 || name.Length > 100) { Err("Tên hãng bay 2–100 ký tự."); return RedirectToAction("Index"); }
            if (!System.Text.RegularExpressions.Regex.IsMatch(iata, "^[A-Z0-9]{2,3}$")) { Err("Mã IATA gồm 2–3 ký tự chữ/số."); return RedirectToAction("Index"); }
            using (var c = TravelDb.Open())
            {
                if (c.ExecuteScalar<int>("SELECT COUNT(*) FROM HangBay WHERE MaIATA=@iata AND MaHangBay<>@id", new { iata, id }) > 0) { Err("Mã IATA đã được dùng."); return RedirectToAction("Index"); }
                if (id == 0) c.Execute("INSERT HangBay (TenHangBay, MaIATA, TrangThai) VALUES (@name, @iata, 1)", new { name, iata });
                else c.Execute("UPDATE HangBay SET TenHangBay=@name, MaIATA=@iata WHERE MaHangBay=@id", new { name, iata, id });
            }
            Audit("AIRLINE_SAVE", "HangBay", id.ToString(), iata); Ok("Đã lưu hãng bay.");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Toggle(int id)
        {
            using (var c = TravelDb.Open()) c.Execute("UPDATE HangBay SET TrangThai = 1 - TrangThai WHERE MaHangBay=@id", new { id });
            Audit("AIRLINE_TOGGLE", "HangBay", id.ToString(), null); Ok("Đã đổi trạng thái hãng bay.");
            return RedirectToAction("Index");
        }
    }

    [RoleAuthorize(Roles = "Admin,Staff")]
    public class CustomersController : AdminController
    {
        public ActionResult Index(string q, string sort, int page = 1)
        {
            if (page < 1) page = 1; const int size = 15; int total;
            var rows = CustomerAdmin.List(q, sort, (page - 1) * size, size, out total);
            using (var c = TravelDb.Open())
                ViewBag.Sum = c.QueryFirst(@"SELECT (SELECT COUNT(*) FROM KhachHang) N, (SELECT COUNT(DISTINCT MaKH) FROM DonDatPhong) WithBooking, (SELECT COUNT(DISTINCT MaKH) FROM VeMayBay) WithTicket,
                    (SELECT COUNT(*) FROM KhachHang WHERE NgayTao>=DATEADD(day,-30,GETDATE())) New30");
            ViewBag.Total = total; ViewBag.Page = page; ViewBag.Size = size; ViewBag.Q = q; ViewBag.Sort = sort; ViewBag.Title = "Khách hàng";
            return View(rows);
        }

        public ActionResult Details(int id)
        {
            var m = CustomerAdmin.Load(id);
            if (m == null) return HttpNotFound();
            ViewBag.Title = "Khách: " + (string)m.Profile.HoTen;
            return View(m);
        }
    }

    [RoleAuthorize(Roles = "Admin,Staff")]
    public class SearchController : AdminController
    {
        public ActionResult Index(string q)
        {
            ViewBag.Title = "Tìm kiếm"; ViewBag.Q = q;
            var res = new Dictionary<string, List<dynamic>>();
            if (!string.IsNullOrWhiteSpace(q) && q.Trim().Length >= 2)
            {
                var like = Like(q);
                using (var c = TravelDb.Open())
                {
                    res["Đặt phòng"] = c.Query(@"SELECT TOP 8 o.MaDon Id, o.MaDonCode Title, ISNULL(o.HoTenKhach,kh.HoTen) + N' · ' + ks.TenKS Sub, o.TrangThai Badge FROM DonDatPhong o JOIN KhachSan ks ON ks.MaKS=o.MaKS JOIN KhachHang kh ON kh.MaKH=o.MaKH
                        WHERE o.MaDonCode LIKE @l OR o.HoTenKhach LIKE @l COLLATE Vietnamese_100_CI_AI OR o.EmailKhach LIKE @l OR kh.Email LIKE @l ORDER BY o.NgayDat DESC", new { l = like }).ToList();
                    res["Vé máy bay"] = c.Query(@"SELECT TOP 8 v.MaVe Id, ISNULL(v.MaVeCode,'TH-F'+CAST(v.MaVe AS varchar(10))) Title, v.HoTenHanhKhach + N' · ' + f.MaChuyen + N' ' + ISNULL(f.MaSanBayDi,'') + N'→' + ISNULL(f.MaSanBayDen,'') Sub, v.TrangThai Badge FROM VeMayBay v JOIN ChuyenBay f ON f.MaChuyenBay=v.MaChuyenBay
                        WHERE v.MaVeCode LIKE @l OR v.HoTenHanhKhach LIKE @l COLLATE Vietnamese_100_CI_AI OR v.SoGiayTo LIKE @l ORDER BY v.NgayDat DESC", new { l = like }).ToList();
                    res["Chuyến bay"] = c.Query(@"SELECT TOP 8 f.MaChuyenBay Id, f.MaChuyen Title, f.SanBayDi + N' → ' + f.SanBayDen + N' · ' + CONVERT(nvarchar(16), f.ThoiGianDi, 103) Sub, f.TrangThaiChuyen Badge FROM ChuyenBay f
                        WHERE f.MaChuyen LIKE @l OR f.SanBayDi LIKE @l COLLATE Vietnamese_100_CI_AI OR f.SanBayDen LIKE @l COLLATE Vietnamese_100_CI_AI OR f.MaSanBayDi LIKE @l OR f.MaSanBayDen LIKE @l ORDER BY f.ThoiGianDi DESC", new { l = like }).ToList();
                    res["Khách hàng"] = c.Query(@"SELECT TOP 8 kh.MaKH Id, kh.HoTen Title, kh.Email + ISNULL(N' · ' + kh.SoDienThoai, N'') Sub, '' Badge FROM KhachHang kh LEFT JOIN TaiKhoan tk ON tk.MaKH=kh.MaKH
                        WHERE kh.HoTen LIKE @l COLLATE Vietnamese_100_CI_AI OR kh.Email LIKE @l OR kh.SoDienThoai LIKE @l OR tk.TenDangNhap LIKE @l ORDER BY kh.HoTen", new { l = like }).ToList();
                    res["Chỗ nghỉ"] = c.Query(@"SELECT TOP 8 MaKS Id, TenKS Title, DiaChi Sub, '' Badge FROM KhachSan WHERE TenKS LIKE @l COLLATE Vietnamese_100_CI_AI OR DiaChi LIKE @l COLLATE Vietnamese_100_CI_AI ORDER BY TenKS", new { l = like }).ToList();
                }
            }
            ViewBag.Results = res;
            return View();
        }
    }
}
