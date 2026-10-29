using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;
using TravelHub.Controllers;
using TravelHub.Data;
using TravelHub.Infrastructure;
using TravelHub.Models;
using TravelHub.Services;

namespace TravelHub.Areas.Admin.Controllers
{
    public class HotelEditModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Nhập tên"), StringLength(150), Display(Name = "Tên chỗ nghỉ")] public string TenKS { get; set; }
        [Required, Display(Name = "Điểm đến")] public int MaDiaDiem { get; set; }
        [Required(ErrorMessage = "Nhập địa chỉ"), StringLength(255), Display(Name = "Địa chỉ")] public string DiaChi { get; set; }
        [Range(1, 5), Display(Name = "Hạng sao")] public int SoSao { get; set; } = 3;
        [Range(0, 1000000000), Display(Name = "Giá từ (₫/đêm)")] public decimal GiaTu { get; set; }
        [Display(Name = "Mô tả")] public string MoTa { get; set; }
        [StringLength(50), Display(Name = "Loại hình")] public string LoaiHinh { get; set; } = "Khách sạn";
        [StringLength(20), Display(Name = "Điện thoại")] public string SoDienThoai { get; set; }
        [EmailAddress, StringLength(100), Display(Name = "Email")] public string Email { get; set; }
        [Display(Name = "Chính sách chung")] public string ChinhSach { get; set; }
        [Display(Name = "Chính sách hủy")] public string ChinhSachHuyLoai { get; set; } = "FLEX1";
        [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Giờ dạng HH:mm"), Display(Name = "Giờ nhận phòng")] public string GioNhanPhong { get; set; } = "14:00";
        [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Giờ dạng HH:mm"), Display(Name = "Giờ trả phòng")] public string GioTraPhong { get; set; } = "12:00";
        [Display(Name = "Bao gồm bữa sáng")] public bool BuaSang { get; set; }
        [Display(Name = "Vĩ độ")] public decimal? ViDo { get; set; }
        [Display(Name = "Kinh độ")] public decimal? KinhDo { get; set; }
        [Display(Name = "Đang kinh doanh")] public bool TrangThai { get; set; } = true;
        public int[] AmenityIds { get; set; } = new int[0];
    }

    [RoleAuthorize(Roles = "Admin")]
    public class HotelsController : AdminController
    {
        void Lookups(HotelEditModel m = null)
        {
            ViewBag.Destinations = CatalogService.Destinations();
            ViewBag.Amenities = HotelSearchService.Amenities();
        }

        public ActionResult Index(string q, string status)
        {
            using (var c = TravelDb.Open())
            {
                ViewBag.Q = q; ViewBag.Status = status; ViewBag.Title = "Chỗ nghỉ";
                return View(c.Query(@"SELECT ks.MaKS, ks.TenKS, dd.TenDiaDiem, ks.SoSao, ks.LoaiHinh, ks.GiaTu, ks.TrangThai,
                    (SELECT COUNT(*) FROM Phong p WHERE p.MaKS=ks.MaKS) Phong, (SELECT COUNT(*) FROM DonDatPhong o WHERE o.MaKS=ks.MaKS) Don, ks.HinhAnhDaiDien
                    FROM KhachSan ks JOIN DiaDiem dd ON dd.MaDiaDiem=ks.MaDiaDiem
                    WHERE (@q IS NULL OR ks.TenKS LIKE @q COLLATE Vietnamese_100_CI_AI) AND (@st IS NULL OR ks.TrangThai=@st) ORDER BY ks.MaKS DESC",
                    new { q = Like(q), st = string.IsNullOrEmpty(status) ? (bool?)null : status == "1" }).ToList());
            }
        }

        public ActionResult Create() { Lookups(); ViewBag.Title = "Thêm chỗ nghỉ"; return View("Edit", new HotelEditModel()); }

        public ActionResult Edit(int id)
        {
            using (var c = TravelDb.Open())
            {
                var r = c.QueryFirstOrDefault(@"SELECT MaKS Id, TenKS, MaDiaDiem, DiaChi, SoSao, ISNULL(GiaTu,0) GiaTu, MoTa, LoaiHinh, SoDienThoai, Email, ChinhSach, ChinhSachHuyLoai, GioNhanPhong, GioTraPhong, BuaSang, ViDo, KinhDo, TrangThai FROM KhachSan WHERE MaKS=@id", new { id });
                if (r == null) return HttpNotFound();
                var m = new HotelEditModel { Id = r.Id, TenKS = r.TenKS, MaDiaDiem = r.MaDiaDiem, DiaChi = r.DiaChi, SoSao = r.SoSao, GiaTu = r.GiaTu, MoTa = r.MoTa, LoaiHinh = r.LoaiHinh, SoDienThoai = r.SoDienThoai, Email = r.Email,
                    ChinhSach = r.ChinhSach, ChinhSachHuyLoai = r.ChinhSachHuyLoai, GioNhanPhong = r.GioNhanPhong, GioTraPhong = r.GioTraPhong, BuaSang = r.BuaSang, ViDo = r.ViDo, KinhDo = r.KinhDo, TrangThai = r.TrangThai };
                m.AmenityIds = c.Query<int>("SELECT MaTienIch FROM KhachSan_TienIch WHERE MaKS=@id", new { id }).ToArray();
                Lookups(); ViewBag.Title = "Sửa: " + m.TenKS;
                ViewBag.Images = c.Query<HinhAnhKhachSan>("SELECT MaHinhAnh, MaKS, DuongDanAnh, MoTa, LaAnhDaiDien, Loai, ThuTu, HienThi FROM HinhAnhKhachSan WHERE MaKS=@id ORDER BY ThuTu, MaHinhAnh", new { id }).ToList();
                ViewBag.Rooms = c.Query("SELECT MaPhong, TenPhong, LoaiPhong, GiaPhong, SoLuong, TrangThai FROM Phong WHERE MaKS=@id", new { id }).ToList();
                ViewBag.Bookings = c.ExecuteScalar<int>("SELECT COUNT(*) FROM DonDatPhong WHERE MaKS=@id", new { id });
                return View(m);
            }
        }

        [HttpPost]
        public ActionResult Save(HotelEditModel m)
        {
            if (!ModelState.IsValid) { Lookups(); ViewBag.Title = m.Id == 0 ? "Thêm chỗ nghỉ" : "Sửa: " + m.TenKS; return View("Edit", m); }
            using (var c = TravelDb.Open())
            using (var tx = c.BeginTransaction())
            {
                var args = new { m.Id, m.TenKS, m.MaDiaDiem, m.DiaChi, m.SoSao, m.GiaTu, m.MoTa, m.LoaiHinh, m.SoDienThoai, m.Email, m.ChinhSach, m.ChinhSachHuyLoai, m.GioNhanPhong, m.GioTraPhong, m.BuaSang, m.ViDo, m.KinhDo, m.TrangThai };
                if (new[] { "FREE24", "FLEX1", "NONREFUND" }.All(x => x != m.ChinhSachHuyLoai)) m.ChinhSachHuyLoai = "FLEX1";
                if (m.Id == 0)
                    m.Id = c.ExecuteScalar<int>(@"INSERT INTO KhachSan (TenKS, MaDiaDiem, DiaChi, SoSao, GiaTu, MoTa, LoaiHinh, SoDienThoai, Email, ChinhSach, ChinhSachHuyLoai, GioNhanPhong, GioTraPhong, BuaSang, ViDo, KinhDo, TrangThai, NgayTao)
                        VALUES (@TenKS,@MaDiaDiem,@DiaChi,@SoSao,@GiaTu,@MoTa,@LoaiHinh,@SoDienThoai,@Email,@ChinhSach,@ChinhSachHuyLoai,@GioNhanPhong,@GioTraPhong,@BuaSang,@ViDo,@KinhDo,@TrangThai,GETDATE()); SELECT CAST(SCOPE_IDENTITY() AS int);", args, tx);
                else
                    c.Execute(@"UPDATE KhachSan SET TenKS=@TenKS, MaDiaDiem=@MaDiaDiem, DiaChi=@DiaChi, SoSao=@SoSao, GiaTu=@GiaTu, MoTa=@MoTa, LoaiHinh=@LoaiHinh, SoDienThoai=@SoDienThoai, Email=@Email, ChinhSach=@ChinhSach,
                        ChinhSachHuyLoai=@ChinhSachHuyLoai, GioNhanPhong=@GioNhanPhong, GioTraPhong=@GioTraPhong, BuaSang=@BuaSang, ViDo=@ViDo, KinhDo=@KinhDo, TrangThai=@TrangThai WHERE MaKS=@Id", args, tx);
                c.Execute("DELETE FROM KhachSan_TienIch WHERE MaKS=@id", new { id = m.Id }, tx);
                foreach (var a in (m.AmenityIds ?? new int[0]).Distinct())
                    c.Execute("INSERT INTO KhachSan_TienIch (MaKS, MaTienIch) VALUES (@k,@a)", new { k = m.Id, a }, tx);
                tx.Commit();
            }
            MenuService.Invalidate(); Audit("HOTEL_SAVE", "KhachSan", m.Id.ToString(), m.TenKS);
            Ok("Đã lưu chỗ nghỉ.");
            return RedirectToAction("Edit", new { id = m.Id });
        }

        /// <summary>Không xóa cứng: ngừng kinh doanh để giữ lịch sử đơn/giao dịch.</summary>
        [HttpPost]
        public ActionResult Deactivate(int id, bool active)
        {
            using (var c = TravelDb.Open()) c.Execute("UPDATE KhachSan SET TrangThai=@a WHERE MaKS=@id", new { a = active, id });
            MenuService.Invalidate(); Audit(active ? "HOTEL_ACTIVATE" : "HOTEL_DEACTIVATE", "KhachSan", id.ToString(), null);
            Ok(active ? "Đã mở bán lại." : "Đã ngừng kinh doanh (dữ liệu lịch sử được giữ nguyên).");
            return RedirectToAction("Index");
        }

        // ---- Ảnh ----
        [HttpPost]
        public ActionResult UploadImage(int id, HttpPostedFileBase file, string caption, string kind)
        {
            string err; var path = UploadHelper.SaveImage(file, "~/Content/images/hotels/uploads", "h" + id, out err);
            if (path == null) { Err(err); return RedirectToAction("Edit", new { id }); }
            using (var c = TravelDb.Open())
            {
                var hasCover = c.ExecuteScalar<int>("SELECT COUNT(*) FROM HinhAnhKhachSan WHERE MaKS=@id AND LaAnhDaiDien=1", new { id }) > 0;
                c.Execute("INSERT INTO HinhAnhKhachSan (MaKS, DuongDanAnh, MoTa, LaAnhDaiDien, Loai, ThuTu, HienThi) VALUES (@id,@p,@c,@cv,@k,(SELECT ISNULL(MAX(ThuTu),0)+1 FROM HinhAnhKhachSan WHERE MaKS=@id),1)",
                    new { id, p = path, c = string.IsNullOrWhiteSpace(caption) ? null : caption.Trim(), cv = !hasCover, k = kind });
            }
            Audit("HOTEL_IMAGE_ADD", "KhachSan", id.ToString(), path); Ok("Đã tải ảnh lên.");
            return RedirectToAction("Edit", new { id });
        }

        [HttpPost]
        public ActionResult ImageAction(int id, int imageId, string op, string caption, int? order)
        {
            using (var c = TravelDb.Open())
            {
                switch (op)
                {
                    case "cover":
                        c.Execute("UPDATE HinhAnhKhachSan SET LaAnhDaiDien = CASE WHEN MaHinhAnh=@i THEN 1 ELSE 0 END WHERE MaKS=@id", new { i = imageId, id });
                        c.Execute("UPDATE KhachSan SET HinhAnhDaiDien=(SELECT DuongDanAnh FROM HinhAnhKhachSan WHERE MaHinhAnh=@i AND MaKS=@id) WHERE MaKS=@id", new { i = imageId, id }); break;
                    case "toggle": c.Execute("UPDATE HinhAnhKhachSan SET HienThi = 1 - HienThi WHERE MaHinhAnh=@i AND MaKS=@id", new { i = imageId, id }); break;
                    case "save": c.Execute("UPDATE HinhAnhKhachSan SET MoTa=@c, ThuTu=ISNULL(@o,ThuTu) WHERE MaHinhAnh=@i AND MaKS=@id", new { c = caption, o = order, i = imageId, id }); break;
                    case "delete":
                        var path = c.ExecuteScalar<string>("SELECT DuongDanAnh FROM HinhAnhKhachSan WHERE MaHinhAnh=@i AND MaKS=@id", new { i = imageId, id });
                        c.Execute("DELETE FROM HinhAnhKhachSan WHERE MaHinhAnh=@i AND MaKS=@id AND LaAnhDaiDien=0", new { i = imageId, id });
                        if (path != null && path.StartsWith("/Content/images/hotels/uploads/") && c.ExecuteScalar<int>("SELECT COUNT(*) FROM HinhAnhKhachSan WHERE DuongDanAnh=@p", new { p = path }) == 0)
                        { try { System.IO.File.Delete(Server.MapPath("~" + path)); } catch { } }
                        break;
                    default: return new HttpStatusCodeResult(400);
                }
            }
            Audit("HOTEL_IMAGE_" + op.ToUpper(), "KhachSan", id.ToString(), "img=" + imageId);
            Ok("Đã cập nhật ảnh.");
            return RedirectToAction("Edit", new { id });
        }
    }

    public class RoomEditModel
    {
        public int Id { get; set; }
        [Required] public int HotelId { get; set; }
        [Required(ErrorMessage = "Nhập tên phòng"), StringLength(100), Display(Name = "Tên loại phòng")] public string TenPhong { get; set; }
        [StringLength(100), Display(Name = "Loại phòng")] public string LoaiPhong { get; set; }
        [Range(1, 20), Display(Name = "Sức chứa tối đa")] public int SoNguoiToiDa { get; set; } = 2;
        [Range(1, 10), Display(Name = "Số giường")] public int? SoGiuong { get; set; } = 1;
        [Display(Name = "Diện tích (m²)")] public decimal? DienTich { get; set; }
        [Range(0, 1000000000), Display(Name = "Giá cơ bản (₫/đêm)")] public decimal GiaPhong { get; set; }
        [Range(0, 500), Display(Name = "Số lượng phòng")] public int SoLuong { get; set; } = 1;
        [StringLength(500), Display(Name = "Mô tả")] public string MoTa { get; set; }
        [Display(Name = "Đang mở bán")] public bool TrangThai { get; set; } = true;
    }

    [RoleAuthorize(Roles = "Admin")]
    public class RoomsController : AdminController
    {
        public ActionResult Index(int? hotelId)
        {
            using (var c = TravelDb.Open())
            {
                ViewBag.Hotels = c.Query("SELECT MaKS, TenKS FROM KhachSan ORDER BY TenKS").ToList(); ViewBag.HotelId = hotelId; ViewBag.Title = "Loại phòng";
                return View(c.Query(@"SELECT p.MaPhong, p.TenPhong, p.LoaiPhong, ks.TenKS, p.MaKS, p.SoNguoiToiDa, p.GiaPhong, p.SoLuong, p.TrangThai,
                        dbo.fn_PhongConLai(p.MaPhong, CAST(GETDATE() AS date), DATEADD(day,1,CAST(GETDATE() AS date))) ConLaiHomNay, (SELECT COUNT(*) FROM HinhAnhPhong h WHERE h.MaPhong=p.MaPhong) Anh
                        FROM Phong p JOIN KhachSan ks ON ks.MaKS=p.MaKS WHERE (@h IS NULL OR p.MaKS=@h) ORDER BY ks.TenKS, p.GiaPhong", new { h = hotelId }).ToList());
            }
        }

        public ActionResult Create(int hotelId) { ViewBag.Title = "Thêm loại phòng"; return View("Edit", new RoomEditModel { HotelId = hotelId }); }

        public ActionResult Edit(int id)
        {
            using (var c = TravelDb.Open())
            {
                var m = c.QueryFirstOrDefault<RoomEditModel>("SELECT MaPhong Id, MaKS HotelId, TenPhong, LoaiPhong, SoNguoiToiDa, SoGiuong, DienTich, GiaPhong, SoLuong, MoTa, TrangThai FROM Phong WHERE MaPhong=@id", new { id });
                if (m == null) return HttpNotFound();
                ViewBag.Title = "Sửa phòng: " + m.TenPhong;
                ViewBag.HotelName = c.ExecuteScalar<string>("SELECT TenKS FROM KhachSan WHERE MaKS=@h", new { h = m.HotelId });
                ViewBag.Images = c.Query<RoomImageItem>("SELECT MaHinhAnhPhong Id, MaPhong RoomId, DuongDanAnh Url, MoTa Caption, Loai Kind, ThuTu [Order], LaAnhDaiDien IsCover, HienThi Visible FROM HinhAnhPhong WHERE MaPhong=@id ORDER BY ThuTu, MaHinhAnhPhong", new { id }).ToList();
                ViewBag.Blocks = c.Query("SELECT MaNgungBan, TuNgay, DenNgay, SoLuong, LyDo FROM PhongNgungBan WHERE MaPhong=@id AND DenNgay >= CAST(GETDATE() AS date) ORDER BY TuNgay", new { id }).ToList();
                ViewBag.Prices = c.Query("SELECT Ngay, Gia FROM GiaPhongTheoNgay WHERE MaPhong=@id AND Ngay >= CAST(GETDATE() AS date) ORDER BY Ngay", new { id }).ToList();
                return View(m);
            }
        }

        [HttpPost]
        public ActionResult Save(RoomEditModel m)
        {
            if (!ModelState.IsValid) { ViewBag.Title = "Sửa phòng"; return View("Edit", m); }
            using (var c = TravelDb.Open())
            using (var tx = c.BeginTransaction())
            {
                if (m.Id != 0)
                {
                    // Không cho giảm số lượng thấp hơn mức đã được đặt ở bất kỳ đêm nào trong tương lai.
                    c.Execute("SELECT 1 FROM Phong WITH (UPDLOCK, ROWLOCK) WHERE MaPhong=@id", new { id = m.Id }, tx);
                    var holds = c.Query<InventoryMath.Hold>(@"SELECT o.NgayNhanPhong CheckIn, o.NgayTraPhong CheckOut, ct.SoLuong Qty FROM ChiTietDatPhong ct JOIN DonDatPhong o ON o.MaDon=ct.MaDon
                        WHERE ct.MaPhong=@id AND o.NgayTraPhong > CAST(GETDATE() AS date) AND o.TrangThai NOT IN (N'Đã hủy', N'Hủy', N'Hết hạn')
                          AND NOT (o.TrangThai=N'Chờ thanh toán' AND o.NgayHetHan < GETDATE())", new { id = m.Id }, tx).ToList();
                    int peak = InventoryMath.PeakUsed(holds, DateTime.Today, DateTime.Today.AddYears(1));
                    if (m.SoLuong < peak) { ModelState.AddModelError("SoLuong", "Đã có đặt phòng chiếm tối đa " + peak + " phòng trong tương lai — không thể giảm xuống " + m.SoLuong + "."); ViewBag.Title = "Sửa phòng"; return View("Edit", m); }
                    c.Execute("UPDATE Phong SET TenPhong=@TenPhong, LoaiPhong=@LoaiPhong, SoNguoiToiDa=@SoNguoiToiDa, SoGiuong=@SoGiuong, DienTich=@DienTich, GiaPhong=@GiaPhong, SoLuong=@SoLuong, MoTa=@MoTa, TrangThai=@TrangThai WHERE MaPhong=@Id", m, tx);
                }
                else
                    m.Id = c.ExecuteScalar<int>("INSERT INTO Phong (MaKS, TenPhong, LoaiPhong, SoNguoiToiDa, SoGiuong, DienTich, GiaPhong, SoLuong, MoTa, TrangThai) VALUES (@HotelId,@TenPhong,@LoaiPhong,@SoNguoiToiDa,@SoGiuong,@DienTich,@GiaPhong,@SoLuong,@MoTa,@TrangThai); SELECT CAST(SCOPE_IDENTITY() AS int);", m, tx);
                // Giá "từ" của chỗ nghỉ luôn bám theo giá thấp nhất đang bán.
                c.Execute("UPDATE KhachSan SET GiaTu = ISNULL((SELECT MIN(GiaPhong) FROM Phong WHERE MaKS=@h AND TrangThai=1), GiaTu) WHERE MaKS=@h", new { h = m.HotelId }, tx);
                tx.Commit();
            }
            Audit("ROOM_SAVE", "Phong", m.Id.ToString(), m.TenPhong); Ok("Đã lưu loại phòng.");
            return RedirectToAction("Edit", new { id = m.Id });
        }

        [HttpPost]
        public ActionResult Block(int id, DateTime from, DateTime to, int qty, string reason)
        {
            if (to.Date <= from.Date || qty < 1) { Err("Khoảng ngày/số lượng không hợp lệ."); return RedirectToAction("Edit", new { id }); }
            using (var c = TravelDb.Open())
            {
                c.Execute("INSERT INTO PhongNgungBan (MaPhong, TuNgay, DenNgay, SoLuong, LyDo) VALUES (@id,@f,@t,@q,@r)", new { id, f = from.Date, t = to.Date, q = qty, r = reason });
                var short_ = c.ExecuteScalar<int>(@"SELECT COUNT(*) FROM (SELECT TOP (DATEDIFF(day,@f,@t)) DATEADD(day,ROW_NUMBER() OVER (ORDER BY (SELECT NULL))-1,@f) d FROM sys.all_columns) x
                    CROSS APPLY (SELECT SUM(ct.SoLuong) n FROM ChiTietDatPhong ct JOIN DonDatPhong o ON o.MaDon=ct.MaDon WHERE ct.MaPhong=@id AND o.NgayNhanPhong<=x.d AND o.NgayTraPhong>x.d
                        AND o.TrangThai NOT IN (N'Đã hủy', N'Hủy', N'Hết hạn')) b
                    JOIN Phong p ON p.MaPhong=@id
                    OUTER APPLY (SELECT SUM(SoLuong) n FROM PhongNgungBan WHERE MaPhong=@id AND TuNgay<=x.d AND DenNgay>x.d) m
                    WHERE ISNULL(b.n,0) + ISNULL(m.n,0) > p.SoLuong", new { id, f = from.Date, t = to.Date });
                if (short_ > 0) TempData["err"] = "Đã chặn phòng, nhưng có " + short_ + " đêm đang bị đặt vượt tồn — hãy kiểm tra các đặt phòng bị ảnh hưởng.";
                else Ok("Đã thiết lập ngừng bán/bảo trì.");
            }
            Audit("ROOM_BLOCK", "Phong", id.ToString(), from.ToString("yyyy-MM-dd") + ".." + to.ToString("yyyy-MM-dd"));
            return RedirectToAction("Edit", new { id });
        }

        [HttpPost]
        public ActionResult Unblock(int id, int blockId)
        {
            using (var c = TravelDb.Open()) c.Execute("DELETE FROM PhongNgungBan WHERE MaNgungBan=@b AND MaPhong=@id", new { b = blockId, id });
            Audit("ROOM_UNBLOCK", "Phong", id.ToString(), "block=" + blockId); Ok("Đã bỏ chặn.");
            return RedirectToAction("Edit", new { id });
        }

        [HttpPost]
        public ActionResult SetPrice(int id, DateTime from, DateTime to, decimal? price)
        {
            if (to.Date < from.Date || (to.Date - from.Date).TotalDays > 366) { Err("Khoảng ngày không hợp lệ."); return RedirectToAction("Edit", new { id }); }
            using (var c = TravelDb.Open())
            using (var tx = c.BeginTransaction())
            {
                for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
                {
                    c.Execute("DELETE FROM GiaPhongTheoNgay WHERE MaPhong=@id AND Ngay=@d", new { id, d }, tx);
                    if (price.HasValue && price.Value >= 0) c.Execute("INSERT INTO GiaPhongTheoNgay (MaPhong, Ngay, Gia) VALUES (@id,@d,@p)", new { id, d, p = price.Value }, tx);
                }
                tx.Commit();
            }
            Audit("ROOM_PRICE", "Phong", id.ToString(), from.ToString("yyyy-MM-dd") + ".." + to.ToString("yyyy-MM-dd") + " = " + price);
            Ok(price.HasValue ? "Đã đặt giá theo ngày." : "Đã xóa giá theo ngày (dùng lại giá cơ bản).");
            return RedirectToAction("Edit", new { id });
        }

        [HttpPost]
        public ActionResult UploadImage(int id, HttpPostedFileBase file, string caption, string kind)
        {
            string err; var path = UploadHelper.SaveImage(file, "~/Content/images/rooms/uploads", "r" + id, out err);
            if (path == null) { Err(err); return RedirectToAction("Edit", new { id }); }
            using (var c = TravelDb.Open())
            {
                var hasCover = c.ExecuteScalar<int>("SELECT COUNT(*) FROM HinhAnhPhong WHERE MaPhong=@id AND LaAnhDaiDien=1", new { id }) > 0;
                c.Execute("INSERT INTO HinhAnhPhong (MaPhong, DuongDanAnh, MoTa, Loai, ThuTu, LaAnhDaiDien, HienThi) VALUES (@id,@p,@c,@k,(SELECT ISNULL(MAX(ThuTu),0)+1 FROM HinhAnhPhong WHERE MaPhong=@id),@cv,1)",
                    new { id, p = path, c = string.IsNullOrWhiteSpace(caption) ? null : caption.Trim(), k = kind, cv = !hasCover });
                if (!hasCover) c.Execute("UPDATE Phong SET HinhAnh=@p WHERE MaPhong=@id", new { p = path, id });
            }
            Audit("ROOM_IMAGE_ADD", "Phong", id.ToString(), path); Ok("Đã tải ảnh lên.");
            return RedirectToAction("Edit", new { id });
        }

        [HttpPost]
        public ActionResult ImageAction(int id, int imageId, string op, string caption, string kind, int? order)
        {
            using (var c = TravelDb.Open())
            {
                switch (op)
                {
                    case "cover":
                        c.Execute("UPDATE HinhAnhPhong SET LaAnhDaiDien = CASE WHEN MaHinhAnhPhong=@i THEN 1 ELSE 0 END WHERE MaPhong=@id", new { i = imageId, id });
                        c.Execute("UPDATE Phong SET HinhAnh=(SELECT DuongDanAnh FROM HinhAnhPhong WHERE MaHinhAnhPhong=@i AND MaPhong=@id) WHERE MaPhong=@id", new { i = imageId, id }); break;
                    case "toggle": c.Execute("UPDATE HinhAnhPhong SET HienThi = 1 - HienThi WHERE MaHinhAnhPhong=@i AND MaPhong=@id", new { i = imageId, id }); break;
                    case "save": c.Execute("UPDATE HinhAnhPhong SET MoTa=@c, Loai=@k, ThuTu=ISNULL(@o,ThuTu) WHERE MaHinhAnhPhong=@i AND MaPhong=@id", new { c = caption, k = kind, o = order, i = imageId, id }); break;
                    case "delete":
                        var path = c.ExecuteScalar<string>("SELECT DuongDanAnh FROM HinhAnhPhong WHERE MaHinhAnhPhong=@i AND MaPhong=@id AND LaAnhDaiDien=0", new { i = imageId, id });
                        c.Execute("DELETE FROM HinhAnhPhong WHERE MaHinhAnhPhong=@i AND MaPhong=@id AND LaAnhDaiDien=0", new { i = imageId, id });
                        if (path != null && path.StartsWith("/Content/images/rooms/uploads/") && c.ExecuteScalar<int>("SELECT COUNT(*) FROM HinhAnhPhong WHERE DuongDanAnh=@p", new { p = path }) == 0)
                        { try { System.IO.File.Delete(Server.MapPath("~" + path)); } catch { } }
                        break;
                    default: return new HttpStatusCodeResult(400);
                }
            }
            Audit("ROOM_IMAGE_" + op.ToUpper(), "Phong", id.ToString(), "img=" + imageId); Ok("Đã cập nhật ảnh.");
            return RedirectToAction("Edit", new { id });
        }
    }
}
