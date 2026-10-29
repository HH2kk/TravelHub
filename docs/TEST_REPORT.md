# TEST_REPORT

Ngày chạy: 2026-10-29. Môi trường: Windows 11, MSBuild VS 18, .NET Framework 4.8.1, SQL Server `localhost\MSSQLSERVER01`.

## Build
`MSBuild TravelHub.csproj /p:MvcBuildViews=true` → **0 lỗi** (biên dịch cả toàn bộ Razor view, gồm Area Admin).

## Test tự động — `dotnet test` (xUnit, 61 test): **61 passed, 0 failed** (chạy 2 lần liên tiếp đều đạt)
| Nhóm | Kiểm tra |
|---|---|
| Giá | tổng theo từng đêm × số phòng + VAT; giảm giá không vượt tiền phòng; mã khuyến mãi (%, số tiền, trần, tối thiểu) |
| Chính sách hủy | hủy sớm 100%; FLEX1 giữ 1 đêm; FREE24/NONREFUND không hoàn; mốc 24h |
| Tồn phòng | hết phòng ở đêm giữa kỳ chặn cả kỳ; ngày trả = ngày nhận của người sau; không cộng dồn sai |
| Tìm kiếm & ngày | ngày không hợp lệ (bằng, ngược, quá khứ); khách sạn chỉ xuất hiện khi có phòng đủ sức chứa và trống mọi đêm |
| Chống overbooking | **2 người đặt phòng cuối cùng đồng thời → 1 thành công**; 12 request đồng thời/3 phòng → đúng 3; hold hết hạn trả phòng; hủy trả phòng; phòng bảo trì |
| Quyền sở hữu | khách khác không đọc/hủy/thanh toán đơn của người khác (IDOR) |
| Thanh toán | callback hợp lệ xác nhận 1 lần + biên nhận + email; callback lặp idempotent; **5 callback đồng thời → đúng 1 xác nhận**; sai chữ ký (`97`); sai số tiền (`04`); mã lạ (`01`); thất bại giữ hold & thử lại được; thanh toán đến muộn còn phòng → khôi phục; đến muộn hết phòng → không đặt trùng, tạo yêu cầu hoàn; giao dịch cũ thành công muộn → không xác nhận 2 lần, xếp hoàn |
| Chữ ký VNPay | chữ ký đúng/sai secret/sửa số tiền/thiếu hash; URL thanh toán có `vnp_Amount = VND×100` |
| Hoàn tiền | hủy đơn đã thanh toán tạo đúng 1 yêu cầu; hủy lặp lỗi; đơn hoàn thành không hủy được |
| Email & PDF | outbox chống trùng khóa + gửi tới thư mục pickup; template encode input; PDF hợp lệ (`%PDF`), snapshot bất biến khi giá đổi; 1 biên nhận/giao dịch |
| Tài khoản | đăng ký → xác minh bằng token từ email (một lần) → đăng nhập; trùng tài khoản; khóa sau 5 lần sai; plaintext không được chấp nhận |
| Upload | từ chối `.aspx/.exe/.cshtml`, `.jpg` giả nội dung thực thi, file > 4 MB, file rỗng |

Test tích hợp chạy trên DB `TravelHubDB_Test` (bản sao của `TravelHubDB`).

## Kiểm thử thủ công qua HTTP (IIS Express, DB thật `TravelHubDB`)
- Trang công khai (/, Hotels, Details, Destinations, Articles, Contact, Login) → 200; hero là ảnh máy bay cất cánh; giao diện tiếng Việt hiển thị đúng dấu.
- Ẩn danh vào `/Admin`, `/Booking/My` → 302 đăng nhập; khách thường vào `/Admin` → **403**; POST thiếu antiforgery bị từ chối.
- Admin đăng nhập: toàn bộ 22 trang quản trị trả 200.
- Luồng khách: đăng ký → email xác minh (file .eml) → chưa xác minh không vào được Checkout (302) → xác minh → đăng nhập → Checkout → tạo đơn (giữ phòng) → bấm thanh toán → chuyển hướng tới URL VNPay có chữ ký, số tiền đúng → IPN ký đúng secret test: `00` (xác nhận), IPN lặp: `02`, IPN sai chữ ký: `97` → đơn **Đã thanh toán**, biên nhận `BN…` tạo, email xác nhận gửi, **PDF tải được (application/pdf, font Unicode nhúng)**.
- Dữ liệu thử nghiệm đã được xóa khỏi `TravelHubDB`; tài khoản Admin dùng để thử đã bị vô hiệu hóa.

## Lỗi đã phát hiện và sửa trong quá trình kiểm thử
1. Thiếu binding redirect `System.Memory` (BCrypt) → đăng nhập lỗi 500 khi chạy trên IIS.
2. View mã hóa sai tiếng Việt (thiếu `fileEncoding="utf-8"`).
3. Model binding sai tiền tố ở `Booking/Create` (form dùng `Request.*`).
4. Địa chỉ IP `::1` gửi cho VNPay → chuẩn hóa `127.0.0.1`.
5. Cú pháp Razor (`@if` trong khối code) ở 3 view.

## Chưa kiểm thử / giới hạn
- **Không có** merchant VNPay sandbox thật: chưa thử với trang thanh toán và IPN thật của VNPay (chỉ xác minh với chữ ký tự sinh theo tài liệu). Cần chạy thử bằng thẻ test của VNPay.
- SMTP thật chưa thử (chỉ pickup directory).
- Chưa kiểm thử trên nhiều trình duyệt/thiết bị thật; responsive mới được kiểm tra bằng CSS/xem trình duyệt desktop.
- Hiển thị PDF được xác nhận ở mức cấu trúc (font nhúng Unicode/ToUnicode), chưa xem trực quan từng trang.
- Chưa có test tự động cho giao diện Admin (CRUD qua HTTP chỉ kiểm tra trang render 200).
- Không có test tải (load test) cho truy vấn tìm kiếm.
## Cập nhật: menu + dữ liệu (66/66 đạt)
Thêm lớp `MenuAndDataTests` (5 test): mega-menu đọc dữ liệu thật, chỉ khuyến mãi còn hiệu lực, chip chờ thanh toán hiện rồi ẩn sau khi huỷ, chuyến sắp tới chỉ tính đơn đã thanh toán, mọi khách sạn đang mở đều có phòng/ảnh/ảnh bìa và file ảnh tồn tại. Build `MvcBuildViews=true` sạch.

## Cập nhật: Admin v2 (77/77 đạt, chạy 2 lần)
Thêm `AdminFlightTests` (hủy vé trả ghế đúng 1 lần, không trả ghế khi chuyến đã bay, vé đã bay không hủy được, hủy chuyến hủy vé còn hiệu lực, trạng thái không hợp lệ bị từ chối, đồng bộ vòng đời, lọc vé), `CustomerAdminTests` (tổng hợp 360° đúng khách, danh sách/sắp xếp, ký tự đặc biệt LIKE) và `DashboardTests` (chuỗi ngày đủ, số liệu nhất quán). Kiểm tra HTTP: tạo chuyến hợp lệ → 302, giờ đến < giờ đi hoặc trùng mã → form báo lỗi, thiếu CSRF → 400, tài khoản khách → 403 cho mọi trang admin mới. Một test so sánh doanh thu trước/sau đã bị bỏ vì chạy song song với test thanh toán nên không ổn định.
