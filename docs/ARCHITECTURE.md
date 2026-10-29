# ARCHITECTURE

**Stack:** ASP.NET MVC 5 / .NET Framework 4.8.1, Razor, Bootstrap 5.2 + `Content/site.css`, ADO.NET + Dapper, SQL Server, BCrypt, PDFsharp (GDI+), FormsAuthentication. (Quyết định giữ nguyên nền tảng hiện có — không chuyển sang ASP.NET Core; các tương đương: Identity → bảng `TaiKhoan` + BCrypt + Forms ticket; BackgroundService → `IRegisteredObject` + Timer; Rate limit → `RateLimitAttribute`.)

| Lớp | Thư mục | Vai trò |
|---|---|---|
| Controllers | `Controllers/`, `Areas/Admin/Controllers/` | Nhận request, validate, gọi service; không chứa nghiệp vụ |
| Services | `Services/` | `BookingService` (giữ phòng, hủy, hết hạn), `PaymentService` + `Payment/Gateways` (VNPay), `ReceiptService`, `EmailService`/`BookingNotifier`, `HotelSearchService`, `CatalogService`, `AccountService`, `AdminSeeder` |
| Domain thuần (test được) | `Services/Domain.cs` | `BookingStatus` (state machine), `PricingCalculator`, `CancellationPolicy`, `InventoryMath` |
| Data | `Models/TravelRepository.cs` (cũ, các hàm đọc/đối tác), `Models/TravelDb.cs` | Kết nối DB; các hàm đặt phòng/thanh toán/đăng nhập cũ đã được thay thế bằng Services |
| Infrastructure | `Infrastructure/` | `Auth` (ticket + principal), filters (CSRF toàn cục, role, verified-email, rate-limit), `Ui` (icon, định dạng) |
| Views | `Views/`, `Areas/Admin/Views/` | Razor; email ở `App_Data/EmailTemplates` |
| Tác vụ nền | `Services/BackgroundWorkers.cs` | Mỗi 30 giây: hết hạn giữ phòng + gửi email trong outbox |

## Bảo mật
- Mật khẩu BCrypt; **đã bỏ** nhánh so sánh plaintext; khóa 15 phút sau 5 lần sai; token xác thực/đặt lại lưu dạng SHA-256, một lần dùng, có hạn.
- CSRF: filter toàn cục `AutoValidateAntiForgeryFilter` cho mọi POST (trừ IPN có `[IgnoreAntiForgery]`, bảo vệ bằng chữ ký).
- Phân quyền server-side: `[RoleAuthorize(Roles=...)]` trên từng controller/area; role nằm trong ticket do server mã hóa.
- Chống IDOR: mọi truy vấn booking/biên nhận lọc theo `MaKH` của phiên (nhân viên mới được bỏ lọc).
- Upload: whitelist đuôi + magic bytes + ≤ 4 MB + tên file do server sinh, chỉ lưu dưới `Content/images/**/uploads`.
- Không lưu secret trong DB/log; callback log loại bỏ chữ ký; CSV xuất chống formula injection; header bảo mật, cookie HttpOnly.
- Lỗi: trang 403/404/500 riêng; không lộ stack trace khi `customErrors=RemoteOnly` (production đặt `debug="false"`, `mode="On"`).