# BOOKING_FLOW

1. **Chọn chỗ nghỉ / loại phòng / ngày** (`/Hotels/Details`): ngày được validate (nhận < trả, không quá khứ, ≤ 30 đêm, ≤ 1 năm); tình trạng còn phòng tính bằng `fn_PhongConLai` trên từng đêm; sức chứa = `SoNguoiToiDa × số phòng ≥ số khách`.
2. **Checkout** (`/Booking/Checkout`, bắt buộc đăng nhập + email đã xác minh): nhập thông tin khách lưu trú, mã khuyến mãi, thấy bảng giá (tiền phòng theo từng đêm/giá theo ngày, giảm giá, phụ phí, VAT) — tính ở server bằng `PricingCalculator`.
3. **Tạo đơn** (`POST /Booking/Create`): trong một transaction khóa dòng `Phong` (`UPDLOCK`) → kiểm tra lại tồn → tạo `DonDatPhong` trạng thái **Chờ thanh toán**, `NgayHetHan = now + 15 phút` (cấu hình), snapshot giá/chính sách, ghi lịch sử. Hai người đặt phòng cuối cùng đồng thời: chỉ một người thành công (có test).
4. **Thanh toán** (`POST /Booking/Pay`): tạo bản ghi `ThanhToan` (mã tham chiếu duy nhất), gia hạn hold đủ thời gian ra cổng (tối đa 60 phút từ lúc tạo), chuyển tới VNPay. Lần thử cũ còn dở bị đánh dấu SUPERSEDED.
5. **Xác nhận**: chỉ khi callback hợp lệ (xem PAYMENT_FLOW) → đơn **Đã thanh toán** (= đã xác nhận), tạo biên nhận, xếp email.
6. **Sau đó**: Admin đánh dấu **Đã nhận phòng** → **Hoàn thành**; khách đánh giá khi đã hết kỳ lưu trú (mỗi đơn một đánh giá).

## State machine (`BookingStatus`)
```
Chờ thanh toán → Đã thanh toán | Đã hủy | Hết hạn
Đã thanh toán  → Đã nhận phòng | Đã hủy | Hoàn thành
Đã nhận phòng  → Hoàn thành
Hết hạn        → Đã thanh toán   (chỉ khi thanh toán đến muộn VÀ còn phòng)
```
Mọi chuyển trạng thái ghi `LichSuTrangThaiDon` (người thực hiện, thời điểm, lý do).

## Giải phóng phòng
Tồn phòng không "trừ" vào bảng — nó được **tính** từ các đơn đang giữ chỗ, nên hết hạn/hủy giải phóng ngay lập tức: đơn `Chờ thanh toán` quá `NgayHetHan` không còn được tính, kể cả khi tác vụ nền chưa đổi trạng thái. Tác vụ nền (30 s) đổi trạng thái sang **Hết hạn** (sau 2 phút ân hạn), trả mã giảm giá và gửi email.

## Hủy
Khách hủy `Chờ thanh toán` (không tiền) hoặc `Đã thanh toán` (tính hoàn theo chính sách → tạo `YeuCauHoanTien`). Chính sách: `FREE24` (hoàn 100% nếu hủy trước 24h so với giờ nhận phòng, sau đó 0), `FLEX1` (như trên nhưng hủy muộn bị giữ 1 đêm đầu), `NONREFUND`. Không có chức năng sửa ngày/loại phòng (chưa triển khai; cần hủy và đặt lại).