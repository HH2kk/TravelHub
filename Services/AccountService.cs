using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using TravelHub.Data;
using TravelHub.Models;

namespace TravelHub.Services
{
    public class AccountService
    {
        const int MaxFailures = 5;
        static readonly TimeSpan LockFor = TimeSpan.FromMinutes(15);

        public static string NewToken()
        {
            var b = new byte[32];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(b);
            return Convert.ToBase64String(b).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        public static string HashToken(string token)
        {
            using (var sha = SHA256.Create())
                return string.Concat(sha.ComputeHash(Encoding.UTF8.GetBytes(token ?? "")).Select(x => x.ToString("x2")));
        }

        /// <summary>Trả về null nếu thành công, hoặc thông báo lỗi.</summary>
        public string Register(RegisterViewModel m)
        {
            string token = NewToken();
            using (var c = TravelDb.Open())
            using (var tx = c.BeginTransaction())
            {
                var dup = c.ExecuteScalar<int>("SELECT COUNT(*) FROM TaiKhoan WHERE TenDangNhap=@u OR Email=@e",
                    new { u = m.UserName.Trim(), e = m.Email.Trim() }, tx);
                if (dup > 0) return "Tên đăng nhập hoặc email đã được sử dụng.";
                var kh = c.ExecuteScalar<int>(@"INSERT INTO KhachHang (HoTen, Email, SoDienThoai, TrangThai, NgayTao)
                    VALUES (@n,@e,@p,1,GETDATE()); SELECT CAST(SCOPE_IDENTITY() AS int);",
                    new { n = m.FullName.Trim(), e = m.Email.Trim(), p = string.IsNullOrWhiteSpace(m.Phone) ? null : m.Phone.Trim() }, tx);
                var roleId = c.ExecuteScalar<int>("SELECT MaVaiTro FROM VaiTro WHERE MaVaiTroCode='Traveler'", null, tx);
                c.Execute(@"INSERT INTO TaiKhoan (TenDangNhap, Email, MatKhau, MaVaiTro, MaKH, TrangThai, NgayTao, EmailXacThuc, TokenXacThuc, TokenXacThucHetHan)
                    VALUES (@u,@e,@pw,@r,@kh,1,GETDATE(),0,@t,DATEADD(hour,24,GETDATE()))",
                    new { u = m.UserName.Trim(), e = m.Email.Trim(), pw = PasswordHasher.Hash(m.Password), r = roleId, kh, t = HashToken(token) }, tx);
                tx.Commit();
            }
            SendVerification(m.Email.Trim(), m.FullName.Trim(), token);
            return null;
        }

        void SendVerification(string email, string name, string token)
        {
            var url = AppConfig.BaseUrl + "/Account/ConfirmEmail?email=" + Uri.EscapeDataString(email) + "&token=" + Uri.EscapeDataString(token);
            EmailService.Enqueue("VERIFY_EMAIL", null, email, "Xác minh email TravelHub",
                EmailTemplates.Render("verify-email", new System.Collections.Generic.Dictionary<string, string> {
                    { "Name", name }, { "ButtonUrl", url } }));
        }

        public class LoginResult { public UserSession Session; public string Error; }

        public LoginResult Login(string userName, string password)
        {
            using (var c = TravelDb.Open())
            {
                var row = c.QueryFirstOrDefault(@"
SELECT tk.MaTaiKhoan, tk.TenDangNhap, tk.Email, tk.MatKhau, tk.MaKH, tk.TrangThai, tk.EmailXacThuc, tk.SoLanDangNhapSai, tk.KhoaDen,
       vt.MaVaiTroCode, kh.HoTen
FROM TaiKhoan tk INNER JOIN VaiTro vt ON vt.MaVaiTro = tk.MaVaiTro LEFT JOIN KhachHang kh ON kh.MaKH = tk.MaKH
WHERE tk.TenDangNhap = @u", new { u = (userName ?? "").Trim() });
                const string bad = "Tên đăng nhập hoặc mật khẩu không đúng.";
                if (row == null) { PasswordHasher.Hash("dummy"); return new LoginResult { Error = bad }; }
                if (row.KhoaDen != null && (DateTime)row.KhoaDen > DateTime.Now)
                    return new LoginResult { Error = "Tài khoản tạm khóa do đăng nhập sai nhiều lần. Vui lòng thử lại sau ít phút." };
                if (!PasswordHasher.Verify((string)row.MatKhau, password))
                {
                    c.Execute(@"UPDATE TaiKhoan SET SoLanDangNhapSai = SoLanDangNhapSai + 1,
                                KhoaDen = CASE WHEN SoLanDangNhapSai + 1 >= @max THEN DATEADD(minute,@mins,GETDATE()) ELSE KhoaDen END
                                WHERE MaTaiKhoan=@id", new { max = MaxFailures, mins = (int)LockFor.TotalMinutes, id = (int)row.MaTaiKhoan });
                    return new LoginResult { Error = bad };
                }
                if (!(bool)row.TrangThai) return new LoginResult { Error = "Tài khoản đã bị vô hiệu hóa. Vui lòng liên hệ hỗ trợ." };
                c.Execute("UPDATE TaiKhoan SET SoLanDangNhapSai=0, KhoaDen=NULL, LanDangNhapCuoi=GETDATE() WHERE MaTaiKhoan=@id", new { id = (int)row.MaTaiKhoan });
                var s = new UserSession
                {
                    AccountId = row.MaTaiKhoan,
                    CustomerId = row.MaKH == null ? (int?)null : (int)row.MaKH,
                    UserName = row.TenDangNhap,
                    Email = row.Email,
                    FullName = string.IsNullOrEmpty((string)row.HoTen) ? (string)row.TenDangNhap : (string)row.HoTen,
                    Role = row.MaVaiTroCode ?? AppRoles.Customer,
                    EmailConfirmed = (bool)row.EmailXacThuc
                };
                return new LoginResult { Session = s };
            }
        }

        public bool ConfirmEmail(string email, string token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token)) return false;
            using (var c = TravelDb.Open())
                return c.Execute(@"UPDATE TaiKhoan SET EmailXacThuc=1, TokenXacThuc=NULL, TokenXacThucHetHan=NULL
                    WHERE Email=@e AND TokenXacThuc=@t AND TokenXacThucHetHan > GETDATE()", new { e = email.Trim(), t = HashToken(token) }) == 1;
        }

        public void ResendVerification(int accountId)
        {
            var token = NewToken();
            using (var c = TravelDb.Open())
            {
                var row = c.QueryFirstOrDefault(@"SELECT tk.Email, kh.HoTen, tk.EmailXacThuc FROM TaiKhoan tk LEFT JOIN KhachHang kh ON kh.MaKH=tk.MaKH WHERE tk.MaTaiKhoan=@id", new { id = accountId });
                if (row == null || (bool)row.EmailXacThuc) return;
                c.Execute("UPDATE TaiKhoan SET TokenXacThuc=@t, TokenXacThucHetHan=DATEADD(hour,24,GETDATE()) WHERE MaTaiKhoan=@id", new { t = HashToken(token), id = accountId });
                SendVerification((string)row.Email, (string)(row.HoTen ?? row.Email), token);
            }
        }

        /// <summary>Luôn trả về như thành công để không lộ email nào tồn tại.</summary>
        public void StartPasswordReset(string email)
        {
            var token = NewToken();
            using (var c = TravelDb.Open())
            {
                var n = c.Execute(@"UPDATE TaiKhoan SET TokenDatLaiMK=@t, TokenDatLaiMKHetHan=DATEADD(hour,1,GETDATE()) WHERE Email=@e AND TrangThai=1",
                    new { t = HashToken(token), e = (email ?? "").Trim() });
                if (n == 0) return;
                var name = c.ExecuteScalar<string>("SELECT ISNULL(kh.HoTen, tk.TenDangNhap) FROM TaiKhoan tk LEFT JOIN KhachHang kh ON kh.MaKH=tk.MaKH WHERE tk.Email=@e", new { e = email.Trim() });
                var url = AppConfig.BaseUrl + "/Account/ResetPassword?email=" + Uri.EscapeDataString(email.Trim()) + "&token=" + Uri.EscapeDataString(token);
                EmailService.Enqueue("RESET_PASSWORD", null, email.Trim(), "Đặt lại mật khẩu TravelHub",
                    EmailTemplates.Render("reset-password", new System.Collections.Generic.Dictionary<string, string> { { "Name", name }, { "ButtonUrl", url } }));
            }
        }

        public bool ResetPassword(string email, string token, string newPassword)
        {
            using (var c = TravelDb.Open())
                return c.Execute(@"UPDATE TaiKhoan SET MatKhau=@pw, TokenDatLaiMK=NULL, TokenDatLaiMKHetHan=NULL, SoLanDangNhapSai=0, KhoaDen=NULL
                    WHERE Email=@e AND TokenDatLaiMK=@t AND TokenDatLaiMKHetHan > GETDATE()",
                    new { pw = PasswordHasher.Hash(newPassword), e = (email ?? "").Trim(), t = HashToken(token) }) == 1;
        }

        public string ChangePassword(int accountId, string oldPw, string newPw)
        {
            using (var c = TravelDb.Open())
            {
                var hash = c.ExecuteScalar<string>("SELECT MatKhau FROM TaiKhoan WHERE MaTaiKhoan=@id", new { id = accountId });
                if (!PasswordHasher.Verify(hash, oldPw)) return "Mật khẩu hiện tại không đúng.";
                c.Execute("UPDATE TaiKhoan SET MatKhau=@pw WHERE MaTaiKhoan=@id", new { pw = PasswordHasher.Hash(newPw), id = accountId });
                return null;
            }
        }

        public ProfileViewModel GetProfile(int accountId)
        {
            using (var c = TravelDb.Open())
                return c.QueryFirstOrDefault<ProfileViewModel>(@"
SELECT kh.HoTen AS FullName, kh.SoDienThoai AS Phone, kh.NgaySinh AS BirthDate, kh.GioiTinh AS Gender, tk.Email, kh.HinhAnh AS AvatarUrl, tk.EmailXacThuc AS EmailConfirmed
FROM TaiKhoan tk INNER JOIN KhachHang kh ON kh.MaKH=tk.MaKH WHERE tk.MaTaiKhoan=@id", new { id = accountId });
        }

        public void UpdateProfile(int accountId, ProfileViewModel m, string avatarPath)
        {
            using (var c = TravelDb.Open())
                c.Execute(@"UPDATE kh SET HoTen=@n, SoDienThoai=@p, NgaySinh=@b, GioiTinh=@g, HinhAnh=ISNULL(@a, kh.HinhAnh)
                    FROM KhachHang kh INNER JOIN TaiKhoan tk ON tk.MaKH=kh.MaKH WHERE tk.MaTaiKhoan=@id",
                    new { n = m.FullName.Trim(), p = string.IsNullOrWhiteSpace(m.Phone) ? null : m.Phone.Trim(), b = m.BirthDate, g = m.Gender, a = avatarPath, id = accountId });
        }
    }
}
