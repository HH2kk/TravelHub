# EMAIL_AND_RECEIPT

## Email
- Template HTML: `App_Data/EmailTemplates/*.html` (`_layout` + từng loại). `{{Key}}` được HTML-encode, `{{KeyHtml}}` giữ nguyên. Controller không chứa HTML email.
- Gửi qua **outbox** (`EmailOutbox`): `EmailService.Enqueue` → tác vụ nền gửi mỗi 30 s (và ngay khi có email mới), tối đa 5 lần/email, ghi `LoiCuoi`. `KhoaTrung` (unique) chống gửi trùng khi callback lặp. Lỗi email không làm mất booking/giao dịch.
- Đã có: xác minh email, gửi lại xác minh, đặt lại mật khẩu, xác nhận đặt phòng, biên nhận thanh toán (đính kèm PDF), thanh toán thất bại, hủy, hết hạn giữ phòng, hoàn tiền xong, hoàn tiền bị từ chối, thông báo admin booking mới (nếu cấu hình `EmailQuanTri`), yêu cầu hỗ trợ. Gửi lại email xác nhận từ trang chi tiết đơn (khách/Admin).
- Chưa có: email "cập nhật booking" (không có chức năng sửa booking), email "yêu cầu hoàn tiền" riêng (nội dung nằm trong email hủy).
- Cấu hình: xem README mục 3. Dev: `Smtp:PickupDirectory` ghi file `.eml` (base64) để kiểm tra.

## Biên nhận PDF
- Sinh bằng PDFsharp, font Arial nhúng Unicode (tiếng Việt chuẩn).
- Dữ liệu lấy từ **snapshot JSON** lưu lúc thanh toán (`BienNhan.DuLieuSnapshot`) → đổi giá/tên khách sạn sau này không làm đổi biên nhận cũ (có test). Mã `BNyyMMdd-00001`, một biên nhận cho mỗi giao dịch thành công.
- Tải tại `/Booking/Receipt?no=...`: chỉ chủ đơn hoặc nhân viên; người khác nhận 404.
- Nội dung: mã booking, mã giao dịch (nội bộ + của cổng), người đặt, chỗ nghỉ, phòng, ngày, giá từng đêm, giảm giá/phụ phí/VAT, tổng, đã thanh toán, còn phải trả, phương thức, trạng thái, chính sách hủy.
- **Đây là biên nhận thanh toán, không phải hóa đơn GTGT điện tử** (chưa tích hợp hệ thống hóa đơn) — đã ghi chú trên PDF.