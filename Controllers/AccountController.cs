using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TravelHub.Infrastructure;
using TravelHub.Models;
using TravelHub.Services;

namespace TravelHub.Controllers
{
    public class AccountController : AppController
    {
        readonly AccountService _svc = new AccountService();

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (Me != null) return SafeRedirect(returnUrl);
            ViewBag.Title = "Đăng nhập";
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost, RateLimit(10, 60)]
        public ActionResult Login(LoginViewModel m)
        {
            ViewBag.Title = "Đăng nhập";
            if (!ModelState.IsValid) return View(m);
            var r = _svc.Login(m.UserName, m.Password);
            if (r.Session == null) { ModelState.AddModelError("", r.Error); return View(m); }
            Auth.SignIn(Response, r.Session, false);
            AppLog.Audit(r.Session.AccountId, "LOGIN", "TaiKhoan", r.Session.AccountId.ToString(), null);
            if (string.IsNullOrEmpty(m.ReturnUrl) && AppRoles.AdminArea.Split(',').Contains(r.Session.Role)) return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            return SafeRedirect(m.ReturnUrl);
        }

        [HttpPost]
        public ActionResult Logout() { Auth.SignOut(); return RedirectToAction("Index", "Home"); }

        [HttpGet]
        public ActionResult Register() { ViewBag.Title = "Đăng ký"; return View(new RegisterViewModel()); }

        [HttpPost, RateLimit(5, 300)]
        public ActionResult Register(RegisterViewModel m)
        {
            ViewBag.Title = "Đăng ký";
            if (!ModelState.IsValid) return View(m);
            string err = null;
            try { err = _svc.Register(m); }
            catch (Exception ex) { AppLog.Error("Register", ex); err = "Không thể tạo tài khoản lúc này. Vui lòng thử lại."; }
            if (err != null) { ModelState.AddModelError("", err); return View(m); }
            return RedirectToAction("RegisterDone", new { email = m.Email });
        }

        public ActionResult RegisterDone(string email) { ViewBag.Title = "Kiểm tra email"; ViewBag.Email = email; return View(); }

        public ActionResult VerifyRequired() { ViewBag.Title = "Cần xác minh email"; return View(); }

        [HttpGet]
        public ActionResult ConfirmEmail(string email, string token)
        {
            ViewBag.Title = "Xác minh email";
            ViewBag.Success = _svc.ConfirmEmail(email, token);
            if ((bool)ViewBag.Success && Me != null) Auth.SignOut();   // buộc đăng nhập lại để cập nhật trạng thái xác minh
            return View();
        }

        [HttpPost, RateLimit(3, 300), RoleAuthorize]
        public ActionResult ResendVerification()
        {
            _svc.ResendVerification(Me.AccountId);
            Ok("Đã gửi lại email xác minh. Vui lòng kiểm tra hộp thư.");
            return Request.UrlReferrer != null && Url.IsLocalUrl(Request.UrlReferrer.PathAndQuery) ? (ActionResult)Redirect(Request.UrlReferrer.PathAndQuery) : RedirectToAction("Index", "Home");
        }

        [HttpGet] public ActionResult ForgotPassword() { ViewBag.Title = "Quên mật khẩu"; return View(new ForgotPasswordViewModel()); }

        [HttpPost, RateLimit(5, 300)]
        public ActionResult ForgotPassword(ForgotPasswordViewModel m)
        {
            ViewBag.Title = "Quên mật khẩu";
            if (!ModelState.IsValid) return View(m);
            try { _svc.StartPasswordReset(m.Email); } catch (Exception ex) { AppLog.Error("Forgot", ex); }
            ViewBag.Sent = true;      // luôn báo đã gửi để không lộ email nào tồn tại
            return View(m);
        }

        [HttpGet]
        public ActionResult ResetPassword(string email, string token)
        {
            ViewBag.Title = "Đặt lại mật khẩu";
            return View(new ResetPasswordViewModel { Email = email, Token = token });
        }

        [HttpPost, RateLimit(10, 300)]
        public ActionResult ResetPassword(ResetPasswordViewModel m)
        {
            ViewBag.Title = "Đặt lại mật khẩu";
            if (!ModelState.IsValid) return View(m);
            if (!_svc.ResetPassword(m.Email, m.Token, m.Password)) { ModelState.AddModelError("", "Liên kết không hợp lệ hoặc đã hết hạn. Vui lòng yêu cầu lại."); return View(m); }
            Ok("Đã đặt lại mật khẩu. Bạn có thể đăng nhập.");
            return RedirectToAction("Login");
        }

        [RoleAuthorize, HttpGet]
        public ActionResult Profile()
        {
            ViewBag.Title = "Hồ sơ của tôi";
            return View(_svc.GetProfile(Me.AccountId));
        }

        [RoleAuthorize, HttpPost]
        public ActionResult Profile(ProfileViewModel m, HttpPostedFileBase avatar)
        {
            ViewBag.Title = "Hồ sơ của tôi";
            var cur = _svc.GetProfile(Me.AccountId);
            m.Email = cur.Email; m.EmailConfirmed = cur.EmailConfirmed; m.AvatarUrl = cur.AvatarUrl;
            if (!ModelState.IsValid) return View(m);
            string path = null;
            if (avatar != null && avatar.ContentLength > 0)
            {
                string err;
                path = UploadHelper.SaveImage(avatar, "~/Content/images/avatars", "u" + Me.AccountId, out err);
                if (path == null) { ModelState.AddModelError("", err); return View(m); }
            }
            _svc.UpdateProfile(Me.AccountId, m, path);
            Ok("Đã cập nhật hồ sơ.");
            return RedirectToAction("Profile");
        }

        [RoleAuthorize, HttpGet]
        public ActionResult ChangePassword() { ViewBag.Title = "Đổi mật khẩu"; return View(new ChangePasswordViewModel()); }

        [RoleAuthorize, HttpPost, RateLimit(10, 300)]
        public ActionResult ChangePassword(ChangePasswordViewModel m)
        {
            ViewBag.Title = "Đổi mật khẩu";
            if (!ModelState.IsValid) return View(m);
            var err = _svc.ChangePassword(Me.AccountId, m.OldPassword, m.NewPassword);
            if (err != null) { ModelState.AddModelError("OldPassword", err); return View(m); }
            AppLog.Audit(Me.AccountId, "CHANGE_PASSWORD", "TaiKhoan", Me.AccountId.ToString(), null);
            Ok("Đã đổi mật khẩu.");
            return RedirectToAction("Profile");
        }
    }

    /// <summary>Upload ảnh an toàn: whitelist đuôi + kiểm tra chữ ký (magic bytes) + giới hạn kích thước; tên file do server sinh.</summary>
    public static class UploadHelper
    {
        public const int MaxBytes = 4 * 1024 * 1024;

        public static string SaveImage(HttpPostedFileBase file, string virtualDir, string prefix, out string error)
        {
            error = null;
            if (file == null || file.ContentLength == 0) { error = "Chưa chọn tệp."; return null; }
            if (file.ContentLength > MaxBytes) { error = "Ảnh vượt quá 4 MB."; return null; }
            var ext = Path.GetExtension(file.FileName ?? "").ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".webp") { error = "Chỉ chấp nhận ảnh .jpg, .png hoặc .webp."; return null; }
            var head = new byte[12];
            file.InputStream.Position = 0;
            if (file.InputStream.Read(head, 0, 12) < 12) { error = "Tệp ảnh không hợp lệ."; return null; }
            file.InputStream.Position = 0;
            bool jpg = head[0] == 0xFF && head[1] == 0xD8 && head[2] == 0xFF;
            bool png = head[0] == 0x89 && head[1] == 0x50 && head[2] == 0x4E && head[3] == 0x47;
            bool webp = head[0] == 'R' && head[1] == 'I' && head[2] == 'F' && head[3] == 'F' && head[8] == 'W' && head[9] == 'E' && head[10] == 'B' && head[11] == 'P';
            if (!(jpg || png || webp)) { error = "Nội dung tệp không phải ảnh hợp lệ."; return null; }
            // Chuẩn hóa đuôi theo nội dung thực tế
            ext = jpg ? ".jpg" : png ? ".png" : ".webp";
            var dir = HttpContext.Current.Server.MapPath(virtualDir);
            Directory.CreateDirectory(dir);
            var name = prefix + "-" + Guid.NewGuid().ToString("N").Substring(0, 12) + ext;
            file.SaveAs(Path.Combine(dir, name));
            return virtualDir.TrimStart('~') + "/" + name;
        }
    }
}
