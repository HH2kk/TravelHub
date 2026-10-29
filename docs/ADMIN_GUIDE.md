# ADMIN_GUIDE

Đăng nhập tại `/Account/Login` bằng tài khoản Admin (xem README mục 4) → `/Admin`.

| Role | Quyền |
|---|---|
| `Admin` | Tất cả |
| `Staff` | Booking, thanh toán, hoàn tiền, đánh giá, liên hệ, dashboard |
| `ContentManager` | Điểm đến, bài viết, dashboard |

| Mục | Chức năng |
|---|---|
| Tổng quan | KPI thật từ DB. **Doanh thu thực thu = tiền đã thu thành công − đã hoàn xong**; booking chưa thanh toán nằm ở "Chờ thanh toán", không tính vào doanh thu. Biểu đồ theo ngày, theo trạng thái, booking mới |
| Chỗ nghỉ | Thêm/sửa, tiện nghi, chính sách, giờ nhận/trả phòng, **Ngừng kinh doanh** (không xóa cứng), ảnh: upload, mô tả/alt, thứ tự, đại diện, ẩn/hiện, xóa |
| Loại phòng | Sửa thông tin; **không cho giảm số lượng** dưới mức đã được đặt trong tương lai; ngừng bán/bảo trì theo khoảng ngày (cảnh báo nếu ảnh hưởng đơn có sẵn); giá theo ngày; ảnh phòng riêng |
| Đặt phòng | Lọc theo mã/khách/trạng thái/ngày, xuất CSV, chi tiết, lịch sử, đánh dấu nhận phòng/hoàn thành, hủy (bắt buộc lý do; Admin có thể hoàn 100%), ghi chú nội bộ, gửi lại email, tải biên nhận |
| Thanh toán | Danh sách + nhật ký callback; **không có thao tác sửa trạng thái giao dịch thủ công** |
| Hoàn tiền | Duyệt → nhập mã giao dịch hoàn thực tế → hoàn tất; từ chối kèm lý do; danh sách giao dịch cần đối soát |
| Người dùng | Tìm, xem booking/hoạt động, khóa/mở khóa, xác minh email thủ công, đổi vai trò (không áp dụng cho chính mình) |
| Đánh giá | Ẩn/hiện kèm lý do; không sửa nội dung |
| Điểm đến / Bài viết | CRUD, ảnh, SEO title/description, nháp/xuất bản |
| Cấu hình | Thời gian giữ phòng, VAT, phụ phí, email hỗ trợ/quản trị, bật/tắt cổng. Secret **không** cấu hình ở đây |
| Nhật ký | Audit: ai, khi nào, hành động, IP |

Chưa có: quản lý banner trang chủ, danh mục bài viết riêng (đang là trường văn bản), mã giảm giá CRUD (đọc từ bảng `KhuyenMai` có sẵn; quản trị qua SQL), giao diện đối tác (Partner).
## Huy hiệu & menu
Sidebar Admin hiện số việc cần xử lý (đặt phòng chờ, cần đối soát, hoàn tiền mở, liên hệ mới). Mega-menu phía khách đọc từ DB: sau khi lưu/bật-tắt khách sạn, điểm đến, bài viết, menu tự làm mới. Điểm đến có trường **Vùng** (Bắc/Trung/Nam) dùng cho bộ lọc.

## Admin v2: dashboard, chuyến bay, vé, hồ sơ khách 360°
- **Tổng quan**: KPI có so sánh kỳ trước (doanh thu thực thu, đặt phòng, vé bay, khách mới), biểu đồ theo ngày, phân bố trạng thái, chuyến bay sắp bay + tải ghế, tuyến/hãng/chỗ nghỉ/điểm đến nổi bật, khách giá trị cao, danh sách "cần xử lý hôm nay". Doanh thu chỉ tính giao dịch thanh toán thành công trừ hoàn tiền; giá trị vé máy bay hiển thị riêng vì vé chưa có cổng thanh toán.
- **Chuyến bay** (`/Admin/Flights`): lọc, tải ghế, danh sách hành khách theo chuyến, đổi tình trạng (chậm/hủy…). Hủy chuyến sẽ hủy luôn các vé còn hiệu lực (cần liên hệ khách hoàn/đổi vé thủ công). Chỉ Admin thêm/sửa/ngừng bán chuyến. Trạng thái chuyến và vé tự chuyển sang "Đã hạ cánh/Đã bay" sau giờ đến (tối đa 1 lần/phút khi có người mở trang).
- **Vé máy bay** (`/Admin/Tickets`): tìm theo mã vé/hành khách/giấy tờ/email/chuyến, thẻ lên máy bay chi tiết, làm thủ tục, hủy vé (trả lại ghế nếu chuyến chưa bay), xuất CSV.
- **Hồ sơ khách** (`/Admin/Customers`): 360° gồm đặt phòng (khách sạn, phòng, ngày, số khách, mã giảm giá, thanh toán), vé máy bay, giao dịch, yêu thích, đánh giá, hành vi duyệt web và dòng thời gian. Mở từ Tài khoản, đặt phòng, vé hoặc ô tìm kiếm.
- **Tìm kiếm nhanh** (phím `/`): mã đặt phòng, mã vé, khách, chuyến bay, chỗ nghỉ.
- Dữ liệu vé trong seed là **mẫu** (nhãn "mẫu"); số ghế đã bán của chuyến là tồn kho của hãng nên không nhất thiết bằng số vé ghi trong hệ thống.
