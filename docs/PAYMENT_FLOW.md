# PAYMENT_FLOW

**Cổng:** `IPaymentGateway` (`Services/Payment/Gateways.cs`). `VnPayGateway` (VNPay 2.1.0, HMAC-SHA512) đã triển khai; `MoMoGateway` chỉ là chỗ giữ chỗ (luôn "chưa cấu hình"). Không có thanh toán giả lập: nếu chưa có `Vnpay:TmnCode/HashSecret` hoặc cổng bị tắt, nút thanh toán không khả dụng.

## Luồng
1. `StartPayment`: khóa đơn, kiểm tra chủ đơn/đang chờ/hold còn hạn, **số tiền lấy từ DB** (`ThanhTien`), tạo `ThanhToan` PENDING với `vnp_TxnRef` duy nhất, ký URL.
2. Người dùng thanh toán tại VNPay → trình duyệt về `/Payment/VnPayReturn`; VNPay gọi server-to-server `/Payment/VnPayIpn`. **Cả hai dùng chung `HandleCallback`** (idempotent) nên đơn được xác nhận dù IPN không tới (ví dụ localhost) hoặc người dùng không quay lại.
3. `HandleCallback` (transaction SERIALIZABLE, khóa dòng thanh toán + đơn):
   - sai chữ ký → `97` (không đổi gì); không thấy mã tham chiếu → `01`; sai số tiền → `04`;
   - đã xử lý → `02` (không xử lý lại);
   - thất bại → `ThanhToan=Thất bại`, đơn vẫn giữ nếu còn hạn (thử lại được);
   - thành công + đơn còn giữ → `ThanhToan=Thành công`, đơn → **Đã thanh toán**;
   - thành công nhưng đơn đã **Đã thanh toán** (giao dịch trùng) → `Cần đối soát` + tự tạo yêu cầu hoàn;
   - thành công nhưng hold đã hết/đơn **Hết hạn** → chỉ xác nhận nếu `fn_PhongConLai` còn đủ phòng (khóa dòng `Phong`); nếu không → `Cần đối soát` + yêu cầu hoàn 100%. Đơn **Đã hủy** không bao giờ được khôi phục.
4. Mọi callback được ghi `NhatKyThanhToan` (nguồn, chữ ký hợp lệ?, mã kết quả; không lưu chữ ký).
5. Sau commit: tạo biên nhận (unique theo thanh toán) + email (khóa chống trùng). Lỗi email/PDF không ảnh hưởng giao dịch đã xác nhận.

Trình duyệt không bao giờ quyết định trạng thái: trang kết quả đọc trạng thái từ DB sau khi server xác minh chữ ký.

## Hoàn tiền (`YeuCauHoanTien`)
`PENDING` (tạo tự động khi hủy đơn đã thu tiền / giao dịch trùng / thanh toán muộn hết phòng) → Admin **Duyệt** → `MANUAL_REQUIRED` → hoàn qua cổng/ngân hàng → Admin nhập **mã giao dịch hoàn thực tế** → `REFUNDED` (+ email khách). Có thể **Từ chối** (bắt buộc lý do). Unique index đảm bảo mỗi đơn chỉ có một yêu cầu hoàn đang mở (chống hoàn lặp). Khách chỉ thấy "đã hoàn" sau bước nhập mã. *Gọi API hoàn tiền tự động của VNPay chưa được triển khai.*

## Cần từ nhà cung cấp
VNPay merchant sandbox: `TmnCode`, `HashSecret`, đăng ký IPN/Return URL. Sau khi có, kiểm thử bằng thẻ test của VNPay trước khi lên production (đổi `Vnpay:Url`).

## Đối soát
Admin → Hoàn tiền hiển thị giao dịch `Cần đối soát`; Admin → Thanh toán → Nhật ký xem từng callback. Chưa có job truy vấn chủ động trạng thái giao dịch từ cổng (VNPay `querydr`): nếu cổng không bao giờ gửi callback (cả IPN lẫn return), giao dịch ở trạng thái "Đang xử lý" và phải đối chiếu thủ công trên cổng merchant. Callback đến chậm vẫn được xử lý đúng khi đến (có test).