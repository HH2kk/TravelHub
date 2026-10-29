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
using TravelHub.Services;

namespace TravelHub.Areas.Admin.Controllers
{
    [RoleAuthorize(Roles = "Admin")]
    public class UsersController : AdminController
    {
        public ActionResult Index(string q, string role, int page = 1)
        {
            if (page < 1) page = 1; const int size = 25;
            using (var c = TravelDb.Open())
            {
                var p = new { q = Like(q), r = string.IsNullOrEmpty(role) ? null : role, skip = (page - 1) * size, take = size };
                const string from = "FROM TaiKhoan tk JOIN VaiTro vt ON vt.MaVaiTro=tk.MaVaiTro LEFT JOIN KhachHang kh ON kh.MaKH=tk.MaKH WHERE (@q IS NULL OR tk.TenDangNhap LIKE @q OR tk.Email LIKE @q OR kh.HoTen LIKE @q COLLATE Vietnamese_100_CI_AI) AND (@r IS NULL OR vt.MaVaiTroCode=@r)";
                ViewBag.Total = c.ExecuteScalar<int>("SELECT COUNT(*) " + from, p);
                ViewBag.Page = page; ViewBag.Size = size; ViewBag.Q = q; ViewBag.Role = role; ViewBag.Title = "Người dùng";
                return View(c.Query("SELECT tk.MaTaiKhoan, tk.TenDangNhap, tk.Email, kh.HoTen, vt.MaVaiTroCode Role, vt.TenVaiTro, tk.TrangThai, tk.EmailXacThuc, tk.NgayTao, tk.LanDangNhapCuoi " + from + " ORDER BY tk.MaTaiKhoan DESC OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY", p).ToList());
            }
        }

        public ActionResult Details(int id)
        {
            using (var c = TravelDb.Open())
            {
                var u = c.QueryFirstOrDefault("SELECT tk.*, vt.MaVaiTroCode Role, vt.TenVaiTro, kh.HoTen, kh.SoDienThoai FROM TaiKhoan tk JOIN VaiTro vt ON vt.MaVaiTro=tk.MaVaiTro LEFT JOIN KhachHang kh ON kh.MaKH=tk.MaKH WHERE tk.MaTaiKhoan=@id", new { id });
                if (u == null) return HttpNotFound();
                ViewBag.User = u; ViewBag.Roles = c.Query("SELECT MaVaiTro, TenVaiTro, MaVaiTroCode FROM VaiTro WHERE TrangThai=1 ORDER BY MaVaiTro").ToList();
                ViewBag.Bookings = u.MaKH == null ? new List<TravelHub.Models.BookingListRow>() : BookingService.ListForCustomer((int)u.MaKH);
                ViewBag.Logs = c.Query("SELECT TOP 20 HanhDong, DoiTuong, MoTa, NgayTao FROM NhatKyHeThong WHERE MaTaiKhoan=@id ORDER BY MaNhatKy DESC", new { id }).ToList();
                ViewBag.Title = "Người dùng: " + u.TenDangNhap;
                return View();
            }
        }

        [HttpPost]
        public ActionResult SetActive(int id, bool active)
        {
            if (id == Me.AccountId) { Err("Không thể tự khóa tài khoản của chính mình."); return RedirectToAction("Details", new { id }); }
            using (var c = TravelDb.Open()) c.Execute("UPDATE TaiKhoan SET TrangThai=@a WHERE MaTaiKhoan=@id", new { a = active, id });
            Audit(active ? "USER_UNLOCK" : "USER_LOCK", "TaiKhoan", id.ToString(), null);
            Ok(active ? "Đã mở khóa tài khoản." : "Đã vô hiệu hóa tài khoản.");
            return RedirectToAction("Details", new { id });
        }

        /// <summary>Đổi role: chỉ Admin; không được đổi role của chính mình (chống tự nâng/hạ quyền qua request thông thường).</summary>
        [HttpPost]
        public ActionResult SetRole(int id, int roleId)
        {
            if (id == Me.AccountId) { Err("Không thể thay đổi quyền của chính mình."); return RedirectToAction("Details", new { id }); }
            using (var c = TravelDb.Open())
            {
                var code = c.ExecuteScalar<string>("SELECT MaVaiTroCode FROM VaiTro WHERE MaVaiTro=@r AND TrangThai=1", new { r = roleId });
                if (code == null) return new HttpStatusCodeResult(400);
                c.Execute("UPDATE TaiKhoan SET MaVaiTro=@r WHERE MaTaiKhoan=@id", new { r = roleId, id });
                Audit("USER_ROLE", "TaiKhoan", id.ToString(), "role=" + code);
            }
            Ok("Đã cập nhật vai trò. Người dùng cần đăng nhập lại để áp dụng.");
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        public ActionResult VerifyEmail(int id)
        {
            using (var c = TravelDb.Open()) c.Execute("UPDATE TaiKhoan SET EmailXacThuc=1, TokenXacThuc=NULL WHERE MaTaiKhoan=@id", new { id });
            Audit("USER_VERIFY_EMAIL", "TaiKhoan", id.ToString(), "xác minh thủ công"); Ok("Đã đánh dấu email đã xác minh.");
            return RedirectToAction("Details", new { id });
        }
    }

    [RoleAuthorize(Roles = "Admin,Staff")]
    public class ReviewsController : AdminController
    {
        public ActionResult Index(int? stars, string status)
        {
            using (var c = TravelDb.Open())
            {
                ViewBag.Stars = stars; ViewBag.Status = status; ViewBag.Title = "Đánh giá";
                return View(c.Query(@"SELECT g.MaDanhGia, g.SoSao, g.NoiDung, g.NgayDanhGia, g.TrangThai, g.LyDoAn, g.LaDuLieuMau, kh.HoTen, ks.TenKS, o.MaDonCode
                    FROM DanhGia g JOIN KhachHang kh ON kh.MaKH=g.MaKH JOIN KhachSan ks ON ks.MaKS=g.MaKS LEFT JOIN DonDatPhong o ON o.MaDon=g.MaDon
                    WHERE (@s IS NULL OR g.SoSao=@s) AND (@t IS NULL OR g.TrangThai=@t) ORDER BY g.NgayDanhGia DESC", new { s = stars, t = string.IsNullOrEmpty(status) ? (bool?)null : status == "1" }).ToList());
            }
        }

        /// <summary>Chỉ ẩn/hiện kèm lý do — không sửa nội dung thay người dùng.</summary>
        [HttpPost]
        public ActionResult SetVisible(int id, bool visible, string reason)
        {
            if (!visible && string.IsNullOrWhiteSpace(reason)) { Err("Cần ghi lý do khi ẩn đánh giá."); return RedirectToAction("Index"); }
            using (var c = TravelDb.Open()) c.Execute("UPDATE DanhGia SET TrangThai=@v, LyDoAn=@r WHERE MaDanhGia=@id", new { v = visible, r = visible ? null : reason, id });
            Audit(visible ? "REVIEW_SHOW" : "REVIEW_HIDE", "DanhGia", id.ToString(), reason);
            Ok(visible ? "Đã hiện đánh giá." : "Đã ẩn đánh giá.");
            return RedirectToAction("Index");
        }
    }

    // ---- Nội dung ----
    public class DestinationEditModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Nhập tên"), StringLength(100), Display(Name = "Tên điểm đến")] public string TenDiaDiem { get; set; }
        [StringLength(100), Display(Name = "Tỉnh/Thành phố")] public string ThanhPho { get; set; }
        [StringLength(500), Display(Name = "Mô tả")] public string MoTa { get; set; }
        [StringLength(50), Display(Name = "Phong cách du lịch")] public string PhongCach { get; set; }
        [Display(Name = "Hiển thị")] public bool TrangThai { get; set; } = true;
        public string HinhAnh { get; set; }
    }

    [RoleAuthorize(Roles = "Admin,ContentManager")]
    public class DestinationsController : AdminController
    {
        public ActionResult Index() { using (var c = TravelDb.Open()) { ViewBag.Title = "Điểm đến"; return View(c.Query("SELECT d.MaDiaDiem, d.TenDiaDiem, d.ThanhPho, d.PhongCach, d.TrangThai, d.HinhAnh, (SELECT COUNT(*) FROM KhachSan k WHERE k.MaDiaDiem=d.MaDiaDiem) KS FROM DiaDiem d ORDER BY d.MaDiaDiem").ToList()); } }
        public ActionResult Create() { ViewBag.Title = "Thêm điểm đến"; return View("Edit", new DestinationEditModel()); }
        public ActionResult Edit(int id)
        {
            using (var c = TravelDb.Open())
            {
                var m = c.QueryFirstOrDefault<DestinationEditModel>("SELECT MaDiaDiem Id, TenDiaDiem, ThanhPho, MoTa, PhongCach, TrangThai, HinhAnh FROM DiaDiem WHERE MaDiaDiem=@id", new { id });
                if (m == null) return HttpNotFound(); ViewBag.Title = "Sửa: " + m.TenDiaDiem; return View(m);
            }
        }

        [HttpPost]
        public ActionResult Save(DestinationEditModel m, HttpPostedFileBase file)
        {
            if (!ModelState.IsValid) { ViewBag.Title = "Điểm đến"; return View("Edit", m); }
            string path = null;
            if (file != null && file.ContentLength > 0) { string err; path = UploadHelper.SaveImage(file, "~/Content/images/destinations/uploads", "d", out err); if (path == null) { ModelState.AddModelError("", err); ViewBag.Title = "Điểm đến"; return View("Edit", m); } }
            using (var c = TravelDb.Open())
            {
                if (m.Id == 0) m.Id = c.ExecuteScalar<int>("INSERT INTO DiaDiem (TenDiaDiem, ThanhPho, QuocGia, MoTa, HinhAnh, PhongCach, TrangThai) VALUES (@TenDiaDiem,@ThanhPho,N'Việt Nam',@MoTa,@img,@PhongCach,@TrangThai); SELECT CAST(SCOPE_IDENTITY() AS int);", new { m.TenDiaDiem, m.ThanhPho, m.MoTa, img = path, m.PhongCach, m.TrangThai });
                else c.Execute("UPDATE DiaDiem SET TenDiaDiem=@TenDiaDiem, ThanhPho=@ThanhPho, MoTa=@MoTa, PhongCach=@PhongCach, TrangThai=@TrangThai, HinhAnh=ISNULL(@img,HinhAnh) WHERE MaDiaDiem=@Id", new { m.TenDiaDiem, m.ThanhPho, m.MoTa, m.PhongCach, m.TrangThai, img = path, m.Id });
            }
            MenuService.Invalidate(); Audit("DESTINATION_SAVE", "DiaDiem", m.Id.ToString(), m.TenDiaDiem); Ok("Đã lưu điểm đến.");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Toggle(int id) { using (var c = TravelDb.Open()) c.Execute("UPDATE DiaDiem SET TrangThai = 1 - TrangThai WHERE MaDiaDiem=@id", new { id }); MenuService.Invalidate(); Audit("DESTINATION_TOGGLE", "DiaDiem", id.ToString(), null); Ok("Đã đổi trạng thái hiển thị."); return RedirectToAction("Index"); }
    }

    public class ArticleEditModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Nhập tiêu đề"), StringLength(200), Display(Name = "Tiêu đề")] public string TieuDe { get; set; }
        [StringLength(500), Display(Name = "Mô tả ngắn")] public string MoTaNgan { get; set; }
        [Display(Name = "Nội dung")] public string NoiDung { get; set; }
        [Display(Name = "Điểm đến")] public int? MaDiaDiem { get; set; }
        [StringLength(100), Display(Name = "Tác giả")] public string TacGia { get; set; }
        [StringLength(100), Display(Name = "Danh mục")] public string DanhMuc { get; set; } = "Kinh nghiệm";
        [StringLength(120), Display(Name = "SEO title")] public string SeoTieuDe { get; set; }
        [StringLength(300), Display(Name = "SEO description")] public string SeoMoTa { get; set; }
        [Display(Name = "Đã xuất bản")] public bool TrangThai { get; set; }
        [Display(Name = "Nổi bật")] public bool NoiBat { get; set; }
        public string HinhAnh { get; set; }
    }

    [RoleAuthorize(Roles = "Admin,ContentManager")]
    public class ArticlesController : AdminController
    {
        public ActionResult Index() { using (var c = TravelDb.Open()) { ViewBag.Title = "Bài viết"; return View(c.Query("SELECT MaBaiViet, TieuDe, DanhMuc, TacGia, NgayDang, TrangThai, NoiBat, LuotXem FROM BaiViet ORDER BY NgayDang DESC").ToList()); } }
        public ActionResult Create() { ViewBag.Title = "Thêm bài viết"; ViewBag.Destinations = CatalogService.Destinations(); return View("Edit", new ArticleEditModel { TacGia = Me.FullName }); }
        public ActionResult Edit(int id)
        {
            using (var c = TravelDb.Open())
            {
                var m = c.QueryFirstOrDefault<ArticleEditModel>("SELECT MaBaiViet Id, TieuDe, MoTaNgan, NoiDung, MaDiaDiem, TacGia, DanhMuc, SeoTieuDe, SeoMoTa, TrangThai, NoiBat, HinhAnh FROM BaiViet WHERE MaBaiViet=@id", new { id });
                if (m == null) return HttpNotFound(); ViewBag.Title = "Sửa bài viết"; ViewBag.Destinations = CatalogService.Destinations(); return View(m);
            }
        }

        [HttpPost, ValidateInput(false)]   // nội dung là văn bản thuần; được HTML-encode khi hiển thị (Razor)
        public ActionResult Save(ArticleEditModel m, HttpPostedFileBase file)
        {
            if (!ModelState.IsValid) { ViewBag.Title = "Bài viết"; ViewBag.Destinations = CatalogService.Destinations(); return View("Edit", m); }
            string path = null;
            if (file != null && file.ContentLength > 0) { string err; path = UploadHelper.SaveImage(file, "~/Content/images/articles/uploads", "a", out err); if (path == null) { ModelState.AddModelError("", err); ViewBag.Title = "Bài viết"; ViewBag.Destinations = CatalogService.Destinations(); return View("Edit", m); } }
            using (var c = TravelDb.Open())
            {
                if (m.Id == 0) m.Id = c.ExecuteScalar<int>(@"INSERT INTO BaiViet (MaDiaDiem, TieuDe, MoTaNgan, NoiDung, HinhAnh, TacGia, NgayDang, LuotXem, NoiBat, TrangThai, DanhMuc, SeoTieuDe, SeoMoTa)
                    VALUES (@MaDiaDiem,@TieuDe,@MoTaNgan,@NoiDung,@img,@TacGia,GETDATE(),0,@NoiBat,@TrangThai,@DanhMuc,@SeoTieuDe,@SeoMoTa); SELECT CAST(SCOPE_IDENTITY() AS int);", new { m.MaDiaDiem, m.TieuDe, m.MoTaNgan, m.NoiDung, img = path, m.TacGia, m.NoiBat, m.TrangThai, m.DanhMuc, m.SeoTieuDe, m.SeoMoTa });
                else c.Execute(@"UPDATE BaiViet SET MaDiaDiem=@MaDiaDiem, TieuDe=@TieuDe, MoTaNgan=@MoTaNgan, NoiDung=@NoiDung, TacGia=@TacGia, NoiBat=@NoiBat, TrangThai=@TrangThai, DanhMuc=@DanhMuc, SeoTieuDe=@SeoTieuDe, SeoMoTa=@SeoMoTa, HinhAnh=ISNULL(@img,HinhAnh) WHERE MaBaiViet=@Id",
                    new { m.MaDiaDiem, m.TieuDe, m.MoTaNgan, m.NoiDung, m.TacGia, m.NoiBat, m.TrangThai, m.DanhMuc, m.SeoTieuDe, m.SeoMoTa, img = path, m.Id });
            }
            MenuService.Invalidate(); Audit("ARTICLE_SAVE", "BaiViet", m.Id.ToString(), m.TieuDe); Ok("Đã lưu bài viết.");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Toggle(int id) { using (var c = TravelDb.Open()) c.Execute("UPDATE BaiViet SET TrangThai = 1 - TrangThai WHERE MaBaiViet=@id", new { id }); MenuService.Invalidate(); Audit("ARTICLE_TOGGLE", "BaiViet", id.ToString(), null); Ok("Đã đổi trạng thái xuất bản."); return RedirectToAction("Index"); }
    }

    [RoleAuthorize(Roles = "Admin")]
    public class SettingsController : AdminController
    {
        // Chỉ các khóa cấu hình KHÔNG nhạy cảm. Secret (SMTP/VNPay) cấu hình qua biến môi trường/secrets.config, không qua DB hay form này.
        static readonly string[] Keys = { "GiuPhongPhut", "ThueVAT", "PhuPhiDichVu", "EmailHoTro", "EmailQuanTri", "BatVNPAY", "BatMoMo" };

        public ActionResult Index()
        {
            using (var c = TravelDb.Open())
            {
                ViewBag.Title = "Cấu hình hệ thống";
                ViewBag.Rows = c.Query("SELECT Khoa, GiaTri, MoTa FROM CauHinhHeThong WHERE Khoa IN @k", new { k = Keys }).ToList();
                ViewBag.Gateways = TravelHub.Services.Payment.Gateways.All.Select(g => new KeyValuePair<string, bool>(g.DisplayName, g.IsConfigured)).ToList();
                ViewBag.Smtp = !string.IsNullOrWhiteSpace(AppConfig.Get("Smtp:Host")) ? "SMTP: " + AppConfig.Get("Smtp:Host") : !string.IsNullOrWhiteSpace(AppConfig.Get("Smtp:PickupDirectory")) ? "Dev: ghi email ra thư mục" : "Chưa cấu hình";
                return View();
            }
        }

        [HttpPost]
        public ActionResult Save(FormCollection f)
        {
            foreach (var k in Keys)
            {
                var v = f[k]; if (v == null) continue;
                if ((k == "GiuPhongPhut") && !(int.TryParse(v, out int n) && n >= 5 && n <= 60)) { Err("Thời gian giữ phòng phải từ 5 đến 60 phút."); return RedirectToAction("Index"); }
                if ((k == "ThueVAT" || k == "PhuPhiDichVu") && !(decimal.TryParse(v, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal d) && d >= 0 && d <= 0.5m)) { Err("Tỉ lệ thuế/phụ phí phải từ 0 đến 0.5 (vd 0.08 = 8%)."); return RedirectToAction("Index"); }
                if ((k == "BatVNPAY" || k == "BatMoMo") && v != "0" && v != "1") { Err("Giá trị bật/tắt không hợp lệ."); return RedirectToAction("Index"); }
                AppConfig.SetSetting(k, v.Trim());
            }
            Audit("SETTINGS_SAVE", "CauHinhHeThong", null, "cập nhật cấu hình"); Ok("Đã lưu cấu hình.");
            return RedirectToAction("Index");
        }
    }

    [RoleAuthorize(Roles = "Admin")]
    public class LogsController : AdminController
    {
        public ActionResult Index(string q, int page = 1)
        {
            if (page < 1) page = 1; const int size = 50;
            using (var c = TravelDb.Open())
            {
                var p = new { q = Like(q), skip = (page - 1) * size, take = size };
                const string from = "FROM NhatKyHeThong n LEFT JOIN TaiKhoan t ON t.MaTaiKhoan=n.MaTaiKhoan WHERE (@q IS NULL OR n.HanhDong LIKE @q OR n.MoTa LIKE @q OR n.DoiTuong LIKE @q OR t.TenDangNhap LIKE @q)";
                ViewBag.Total = c.ExecuteScalar<int>("SELECT COUNT(*) " + from, p); ViewBag.Page = page; ViewBag.Size = size; ViewBag.Q = q; ViewBag.Title = "Nhật ký hoạt động";
                return View(c.Query("SELECT n.MaNhatKy, n.NgayTao, t.TenDangNhap, n.HanhDong, n.DoiTuong, n.DoiTuongId, n.MoTa, n.DiaChiIP " + from + " ORDER BY n.MaNhatKy DESC OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY", p).ToList());
            }
        }
    }

    [RoleAuthorize(Roles = "Admin,Staff")]
    public class InboxController : AdminController
    {
        public ActionResult Index() { using (var c = TravelDb.Open()) { ViewBag.Title = "Liên hệ & hỗ trợ"; return View(c.Query("SELECT MaLienHe, HoTen, Email, TieuDe, NoiDung, TrangThai, NgayTao FROM LienHe ORDER BY MaLienHe DESC").ToList()); } }

        [HttpPost]
        public ActionResult Resolve(int id) { using (var c = TravelDb.Open()) c.Execute("UPDATE LienHe SET TrangThai='DONE' WHERE MaLienHe=@id", new { id }); Ok("Đã đánh dấu đã xử lý."); return RedirectToAction("Index"); }
    }
}
