using System;
using Dapper;
using TravelHub.Data;

namespace TravelHub.Services
{
    /// <summary>Tạo/cập nhật tài khoản Admin từ cấu hình triển khai (không hardcode mật khẩu, không tự nâng quyền người dùng đầu tiên).
    /// Chỉ chạy khi có đủ TRAVELHUB_ADMIN_USER, TRAVELHUB_ADMIN_EMAIL, TRAVELHUB_ADMIN_PASSWORD (hoặc appSettings Admin:User/Email/Password).</summary>
    public static class AdminSeeder
    {
        public static void Run()
        {
            var user = AppConfig.Get("Admin:User"); var email = AppConfig.Get("Admin:Email"); var pw = AppConfig.Get("Admin:Password");
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pw)) return;
            if (pw.Length < 10) { AppLog.Error("AdminSeeder", new Exception("Admin:Password phải có ít nhất 10 ký tự — bỏ qua.")); return; }
            using (var c = TravelDb.Open())
            using (var tx = c.BeginTransaction())
            {
                var role = c.ExecuteScalar<int>("SELECT MaVaiTro FROM VaiTro WHERE MaVaiTroCode='Admin'", null, tx);
                var existing = c.ExecuteScalar<int?>("SELECT MaTaiKhoan FROM TaiKhoan WHERE TenDangNhap=@u", new { u = user }, tx);
                var hash = PasswordHasher.Hash(pw);
                if (existing.HasValue)
                    c.Execute("UPDATE TaiKhoan SET MatKhau=@h, MaVaiTro=@r, TrangThai=1, EmailXacThuc=1, SoLanDangNhapSai=0, KhoaDen=NULL WHERE MaTaiKhoan=@id", new { h = hash, r = role, id = existing.Value }, tx);
                else
                {
                    var kh = c.ExecuteScalar<int>("INSERT INTO KhachHang (HoTen, Email, TrangThai, NgayTao) VALUES (N'Quản trị viên', @e, 1, GETDATE()); SELECT CAST(SCOPE_IDENTITY() AS int);", new { e = email }, tx);
                    c.Execute("INSERT INTO TaiKhoan (TenDangNhap, Email, MatKhau, MaVaiTro, MaKH, TrangThai, NgayTao, EmailXacThuc) VALUES (@u,@e,@h,@r,@k,1,GETDATE(),1)", new { u = user, e = email, h = hash, r = role, k = kh }, tx);
                }
                tx.Commit();
            }
            AppLog.Audit(null, "ADMIN_SEED", "TaiKhoan", user, "Admin được tạo/cập nhật từ cấu hình triển khai");
        }
    }
}
