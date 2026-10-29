# PROJECT_ANALYSIS — TravelHub (khảo sát ban đầu)

Ngày khảo sát: 2026-10-29. Chỉ đọc, chưa sửa mã nguồn.

## 1. Kiến trúc & công nghệ hiện có

| Hạng mục | Hiện trạng |
|---|---|
| Solution | `TravelHub.slnx` (1 project: `TravelHub.csproj`, định dạng csproj cũ) |
| Framework | **ASP.NET MVC 5.2.9 trên .NET Framework 4.8.1** (Global.asax, Web.config, packages.config) — *không phải ASP.NET Core* |
| Truy cập dữ liệu | **ADO.NET thuần** (`SqlConnection`/`SqlCommand`) qua `Models/TravelRepository.cs` — *không có EF/EF Core, không có Identity* |
| Database | SQL Server, script `Models/TravelHubDatabase_cl.sql` (tạo `TravelHubDB`), schema đặt tên tiếng Việt |
| UI | Bootstrap 5.2.3, jQuery 3.7.0, Razor — view mới chỉ là template mặc định (Home/Index, About, Contact) |
| Auth | Tự viết: `TaiKhoan`, `VaiTro`, `Quyen`, `VaiTro_Quyen`, `TaiKhoan_Quyen`; `PasswordHasher` dùng BCrypt (có fallback so sánh plaintext) |
| Máy | .NET SDK 10.0.401 (không build được csproj cũ bằng `dotnet build`); MSBuild có tại VS 18 Community; Targeting Pack 4.8.1 có; SQL Server: `MSSQL$MSSQLSERVER01`, `MSSQL$MSSQLSERVER02` đang chạy |

## 2. Schema hiện có (22+ bảng)

`BaiViet`, `ChiTietDatPhong`, `ChuyenBay`, `DanhGia`, `DiaDiem`, `DiaDiemDuLich`, `DonDatPhong`, `HangBay`, `HinhAnhKhachSan`, `KhachHang`, `KhachSan`, `KhachSan_TienIch`, `KhuyenMai`, `Phong`, `Quyen`, `TaiKhoan`, `ThanhToan`, `TienIch`, `VaiTro`, `VaiTro_Quyen`, `VeMayBay`, `YeuThich`, `TaiKhoan_Quyen`.

Đối chiếu nhanh với yêu cầu:

| Yêu cầu | Đã có | Thiếu / cần mở rộng |
|---|---|---|
| Chỗ nghỉ, điểm đến, tiện nghi, bài viết, yêu thích, đánh giá | `KhachSan`, `DiaDiem`, `TienIch`+`KhachSan_TienIch`, `BaiViet`, `YeuThich`, `DanhGia` | slug/SEO, trạng thái kiểm duyệt đánh giá, ảnh theo phòng |
| Phòng & tồn phòng | `Phong` (có `SoLuong`, `SoNguoiToiDa`) — **loại phòng, không có tồn theo ngày** | `RoomInventory` theo ngày, giá theo ngày, bảo trì |
| Đặt phòng | `DonDatPhong`, `ChiTietDatPhong` (tồn phòng tính bằng `SUM` overlap, **không có khóa/transaction chống overbooking**) | mã booking duy nhất, hạn giữ chỗ, snapshot giá/chính sách, lịch sử trạng thái |
| Thanh toán | `ThanhToan`, có `CreateVnpayTransaction`/`ProcessVnpayIpn` | idempotency, log callback, hoàn tiền (`RefundRequest`) |
| Email / PDF / Audit / Support / Banner / Cấu hình | — | toàn bộ cần bổ sung (`EmailOutbox`, `Receipt`, `AuditLog`, …) |
| Xác thực email, reset mật khẩu | — | cột/bảng token, `EmailConfirmed` |

## 3. Vấn đề phát hiện trong source hiện tại

1. **Repository không biên dịch được:** `Models/*.cs` **không được liệt kê trong `<Compile>` của csproj**; `TravelRepository` dùng các kiểu không tồn tại trong repo (`DestinationItem`, `HotelCard`, `RoomItem`, `BookingItem`, `UserSession`, `RegisterViewModel`, `PartnerHotelCreateViewModel`, `HinhAnhKhachSan`, …) → thiếu thư mục ViewModels/Models.
2. **Thiếu NuGet:** `BCrypt.Net` được dùng nhưng không có trong `packages.config`; không có driver/ thư viện PDF/email.
3. **Thiếu connection string** `TravelHub` trong `Web.config` (`TravelDb.Open()` sẽ ném lỗi).
4. **Encoding:** đã kiểm tra — các file `.cs` là UTF-8 hợp lệ (không có BOM); hiện tượng "mojibake" ban đầu chỉ do console Windows PowerShell đọc UTF-8 như ANSI. Không cần chuyển đổi. Lưu ý: luôn đọc/ghi file với `-Encoding UTF8`.
5. **Ảnh:** `Content/images/` **không tồn tại** → mọi đường dẫn trong `Media.cs` đều là ảnh hỏng; chưa có ảnh hero.
6. **Bảo mật:** `PasswordHasher.Verify` chấp nhận mật khẩu plaintext (`string.Equals(stored, input)`) khi hash không bắt đầu `$2`; không có chống CSRF/rate-limit; không có kiểm tra quyền server-side vì chưa có controller.
7. **Controller/View:** chỉ có `HomeController` mặc định; chưa có trang nghiệp vụ nào.
8. Bảng `DonDatPhong.TrangThai` là chuỗi tiếng Việt tự do (`N'Đã hủy'`, `N'Hủy'`, `Cancelled`…) — chưa có state machine.

## 3b. Database thực tế (đọc-chỉ, `localhost\MSSQLSERVER01`, DB `TravelHubDB`, Windows auth)

- Có 26 bảng, **nhiều hơn file .sql** (thêm `KhuyenMai_SuDung`, `NhatKyHeThong`, `UserBehavior`, `UserPreferenceSnapshot`). Dữ liệu thật: 5 khách sạn, 8 phòng, 7 đơn, 1 thanh toán, 8 tài khoản, 5 điểm đến, 6 bài viết. Vai trò hiện có: `Traveler`, `Partner`, `Admin` (khác `Customer/Admin/Staff/ContentManager` trong yêu cầu → ánh xạ, không đổi tên).
- DB thực tế **đã có**: `DonDatPhong.MaDonCode/TrangThaiCode/NgayHetHan/LyDoHuy`, `KhachSan.ChinhSach/LoaiHinh/ViDo/KinhDo`, `ThanhToan.TrangThaiCode/LoaiGiaoDich`.
- **Lệch giữa code và DB:** `TravelRepository` dùng `ThanhToan.MaThamChieuVNPAY` nhưng cột này **không có** trong DB → các hàm VNPay hiện sẽ lỗi SQL. Cột `TaiKhoan` chưa có `EmailConfirmed`/token.
- Trạng thái đơn đang là chuỗi tiếng Việt (`Chờ thanh toán`, `Đã thanh toán`, `Đã hủy`, …) và code đã tin vào các giá trị này → state machine sẽ được xây **trên** các giá trị này (map sang enum), không đổi dữ liệu cũ.

## 4. Quyết định kiến trúc (đề xuất, chờ xác nhận)

Theo nguyên tắc "tôn trọng kiến trúc hiện tại, không nâng cấp framework khi chưa kiểm tra":

- **Giữ ASP.NET MVC 5 / .NET Framework 4.8.1 + ADO.NET + schema tiếng Việt hiện có**, mở rộng bằng script migration SQL có đánh số (`db/migrations/00x_*.sql`, chỉ `ADD`, không `DROP`).
- Thay thế các phần mà prompt gợi ý cho .NET Core bằng tương đương trên .NET Framework: Identity → giữ bảng `TaiKhoan` + BCrypt (bỏ fallback plaintext); xUnit → xUnit/NUnit cho .NET Framework; BackgroundService → `HostedService` qua `HostingEnvironment.QueueBackgroundWorkItem`/timer; Rate limiting → filter tự viết.
- Nếu muốn dùng ASP.NET Core + EF Core + Identity thì đó là **viết lại toàn bộ** (trái nguyên tắc 1.2) — cần bạn xác nhận rõ.

## 5. Kế hoạch giai đoạn

A (xong, tài liệu này) → B kiến trúc → C migration & model → D UI/ảnh → E đặt phòng/tồn phòng → F auth/email/PDF → G thanh toán → H admin → I test/tài liệu.

## 6. Giả định & rủi ro

- Không có credential VNPay/MoMo/SMTP → chỉ dựng cấu trúc + sandbox; ghi rõ trạng thái "chưa kiểm chứng với nhà cung cấp".
- Ảnh hero (máy bay cất cánh) và ảnh phòng: cần tải từ nguồn có giấy phép (Unsplash/Pexels/Wikimedia) — cần truy cập mạng; nguồn sẽ ghi trong `docs/ASSET_SOURCES.md`.
- Chưa kết nối được SQL Server để kiểm tra dữ liệu thực (instance nào chứa `TravelHubDB` chưa xác định).

## 7. Cập nhật sau khi triển khai (2026-10-29)
- Đã xác nhận giữ MVC5/.NET Framework + ADO.NET (Dapper). Các vấn đề mục 3 đã được xử lý: csproj biên dịch toàn bộ thư mục; thêm ViewModels, BCrypt, Dapper, PDFsharp; có connection string; bỏ nhánh plaintext; ảnh đã tải về (`docs/ASSET_SOURCES.md`); các hàm đặt phòng/thanh toán/đăng nhập cũ trong `TravelRepository` được thay bằng `Services` (cột `MaThamChieuVNPAY` đã được thêm bằng migration 001).
- Xem `README.md` (tính năng, hạn chế), `docs/TEST_REPORT.md`.
