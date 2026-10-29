# TravelHub — website đặt phòng lưu trú & kết nối du lịch

ASP.NET MVC 5 (.NET Framework 4.8.1) + ADO.NET/Dapper + SQL Server. Giữ nguyên kiến trúc và database hiện có (xem `docs/PROJECT_ANALYSIS.md`).

## Tính năng đã triển khai (và đã kiểm thử)
Tìm kiếm/lọc từ DB (kiểm tra phòng trống **từng đêm**), chi tiết chỗ nghỉ + gallery, đăng ký/đăng nhập/xác thực email/quên & đặt lại mật khẩu/hồ sơ, yêu thích & so sánh, quy trình đặt phòng (giữ phòng có hạn, chống overbooking), thanh toán VNPay (ký HMAC-SHA512, xác minh callback phía server, idempotent), hủy/hoàn tiền theo chính sách, email HTML (outbox + retry), biên nhận PDF tiếng Việt, đánh giá, điểm đến & cẩm nang, trang Admin (dashboard, chỗ nghỉ/phòng/ảnh/giá theo ngày/bảo trì, booking, thanh toán, hoàn tiền, người dùng, đánh giá, nội dung, cấu hình, nhật ký).

Giao diện premium điện ảnh (hero minh họa SVG nhìn từ trên xuống: máy bay lăn bánh rồi cất cánh, parallax, chuyển cảnh khi tìm, search panel có gợi ý/chọn khách, editorial sections) và kết quả kiểm tra trực quan: `docs/VISUAL_QA_REPORT.md`, ảnh trong `artifacts/visual-qa/`.

## Chưa có / cần credential (nói rõ để không hiểu nhầm)
| Hạng mục | Trạng thái |
|---|---|
| VNPay | Đã tích hợp đủ luồng và kiểm thử bằng chữ ký tự sinh. **Chưa chạy với merchant sandbox thật** vì không có `TmnCode/HashSecret`. Khi chưa cấu hình, trang đặt phòng hiển thị “chưa khả dụng” — không có thanh toán giả lập. |
| MoMo | **Chưa tích hợp** (cổng luôn báo chưa cấu hình). |
| API hoàn tiền của cổng | Chưa gọi tự động. Hoàn tiền: PENDING → Admin duyệt → hoàn thủ công qua cổng/ngân hàng → nhập mã giao dịch hoàn → REFUNDED. |
| SMTP | Cần cấu hình; dev có thể ghi email ra thư mục (`Smtp:PickupDirectory`). |
| Hóa đơn GTGT | Không có; PDF là **biên nhận thanh toán**. |
| Role `Partner` | Dữ liệu/hàm repository cũ giữ nguyên nhưng chưa có giao diện đối tác. |
| Email “cập nhật booking” (đổi ngày/phòng) | Chưa có chức năng sửa booking nên chưa có email này. |

## 1. Yêu cầu
Windows, Visual Studio 2022/2026 (workload ASP.NET) hoặc MSBuild, .NET Framework 4.8.1 Developer Pack, SQL Server/Express, IIS Express, `nuget.exe` (khôi phục gói: `nuget restore TravelHub.slnx` hoặc VS tự khôi phục).

## 2. Database
Chuỗi kết nối: `Web.config` → `connectionStrings/TravelHub` (mặc định `localhost\MSSQLSERVER01`, DB `TravelHubDB`, Windows auth).
Database mới: chạy `Models/TravelHubDatabase_cl.sql` rồi các migration theo thứ tự:
```
sqlcmd -S <server> -d TravelHubDB -E -C -I -b -i db\migrations\001_travelhub_extensions.sql
... 002_availability_function.sql, 003_payment_gateway_columns.sql, 004_images_and_seed.sql, 005_menu_data_expansion.sql, 006_flights_admin.sql
```
Cờ `-I` (QUOTED_IDENTIFIER) là **bắt buộc**. Migration chỉ thêm cột/bảng, idempotent, không xóa dữ liệu. **Luôn BACKUP trước** (`BACKUP DATABASE TravelHubDB TO DISK='...'`). Chi tiết: `docs/DATABASE_DESIGN.md`.

## 3. Cấu hình (không commit secret)
Thứ tự ưu tiên: biến môi trường `TRAVELHUB_<KEY>` (dấu `:` → `__`) → `App_Data\secrets.config` (sao chép từ `secrets.config.example`) → `Web.config`. Xem `.env.example`.
- Email: `Smtp:Host/Port/EnableSsl/User/Password/From` hoặc dev `Smtp:PickupDirectory`.
- VNPay sandbox: `Vnpay:TmnCode`, `Vnpay:HashSecret`, `Vnpay:Url`; đăng ký IPN URL `https://<domain>/Payment/VnPayIpn` và Return URL `https://<domain>/Payment/VnPayReturn` trong cổng merchant. `App:BaseUrl` phải đúng domain công khai (dùng trong link email và return URL).
- Thông số nghiệp vụ (thời gian giữ phòng, VAT, phụ phí, email quản trị, bật/tắt cổng) chỉnh tại Admin → Cấu hình.

## 4. Tạo tài khoản Admin an toàn
Không có mật khẩu mặc định trong mã nguồn. Đặt biến môi trường trước khi khởi động ứng dụng (một lần):
`TRAVELHUB_ADMIN__USER`, `TRAVELHUB_ADMIN__EMAIL`, `TRAVELHUB_ADMIN__PASSWORD` (≥ 10 ký tự). Ứng dụng tạo/cập nhật tài khoản Admin đó khi khởi động; sau đó xóa biến `...PASSWORD`. Đổi vai trò người khác tại Admin → Người dùng (không thể tự đổi quyền của chính mình).

## 5. Chạy website
Mở `TravelHub.slnx` trong Visual Studio, F5 (IIS Express, https://localhost:44317), hoặc:
```
MSBuild TravelHub.csproj /p:Configuration=Debug
"C:\Program Files\IIS Express\iisexpress.exe" /path:C:\TravelHub /port:49688
```
Truy cập `http://localhost:49688`. Admin: `/Admin`.

## 6. Kiểm thử
```
cd Tests\TravelHub.Tests
dotnet test
```
Test tích hợp chạy trên DB **`TravelHubDB_Test`** (xem `Tests/TravelHub.Tests/App.config`): tạo bằng `RESTORE` từ bản backup của `TravelHubDB` (đã chạy migration). Kết quả: `docs/TEST_REPORT.md`.

## 7. Triển khai production
Xem `docs/DEPLOYMENT.md`.

## Cấu trúc
`Controllers/ Areas/Admin/ Models/ (ViewModels, repository cũ) Services/ (booking, payment, email, PDF, search) Infrastructure/ (auth, filter, UI helper) Views/ App_Data/EmailTemplates db/migrations Tests/ docs/`
