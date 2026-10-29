# DATABASE_DESIGN

Giữ nguyên các bảng hiện có (tên tiếng Việt). Các thay đổi chỉ là **thêm** (migration `db/migrations/00x`), chạy được nhiều lần.

## Đối chiếu yêu cầu ↔ entity
| Khái niệm | Bảng dùng | Ghi chú |
|---|---|---|
| Chỗ nghỉ | `KhachSan` (+ `ChinhSachHuyLoai, GioNhanPhong, GioTraPhong, BuaSang`) | `TrangThai=0` là ngừng kinh doanh (không xóa cứng) |
| Loại phòng | `Phong` (`SoLuong` = số phòng thuộc loại) | |
| Ảnh | `HinhAnhKhachSan` (+`ThuTu, Loai, HienThi`), **`HinhAnhPhong`** (mới) | ảnh đại diện, thứ tự, ẩn/hiện |
| Tồn phòng theo ngày | hàm `dbo.fn_PhongConLai(@MaPhong,@In,@Out)` | min(SoLuong − đơn đang giữ − bảo trì) trên từng đêm |
| Bảo trì / ngừng bán | **`PhongNgungBan`** | |
| Giá theo ngày | **`GiaPhongTheoNgay`** (ghi đè giá cơ bản) | |
| Đơn đặt | `DonDatPhong` (+ khách lưu trú, `SnapshotGia` JSON, `ChinhSachHuy`, `HuyMienPhiDen`, `RowVer`), `ChiTietDatPhong` | `MaDonCode` unique |
| Lịch sử trạng thái | **`LichSuTrangThaiDon`** | ai/khi nào/lý do |
| Thanh toán | `ThanhToan` (+`MaThamChieuVNPAY` unique, `MaGiaoDichCong`, `Cong`, `TienTe`) | `MaGiaoDich` = mã tham chiếu nội bộ (cột cũ có UNIQUE) |
| Nhật ký callback | **`NhatKyThanhToan`** | không lưu chữ ký |
| Hoàn tiền | **`YeuCauHoanTien`** (unique filtered: 1 yêu cầu mở/đơn) | |
| Email | **`EmailOutbox`** (unique filtered `KhoaTrung` chống gửi trùng) | |
| Biên nhận | **`BienNhan`** (snapshot JSON bất biến, unique theo thanh toán) | |
| Tài khoản | `TaiKhoan` (+`EmailXacThuc`, token băm, khóa đăng nhập), `VaiTro` (+`Staff`,`ContentManager`) | role cũ giữ: Traveler/Partner/Admin |
| Đánh giá | `DanhGia` (+`LyDoAn`, `LaDuLieuMau`; unique theo `MaDon`) | |
| Nội dung | `BaiViet` (+`DanhMuc`, SEO), `DiaDiem`, `DiaDiemDuLich` | |
| Khác | `LienHe`, `DangKyNhanTin`, `CauHinhHeThong`, `NhatKyHeThong` (audit, đã có) | |

Quy ước: tiền `decimal`; thời gian hệ thống mới dùng UTC (`datetime2`), dữ liệu cũ giữ `datetime` local; trạng thái đơn giữ chuỗi tiếng Việt hiện có và ánh xạ qua `BookingStatus` (không đổi dữ liệu cũ).

## Migration
| # | Nội dung | Tác động |
|---|---|---|
| 001 | Cột/bảng mới (xem trên). Tài khoản **đã tồn tại** được đặt `EmailXacThuc=1` để không bị khóa | chỉ thêm |
| 002 | `fn_PhongConLai` | thêm hàm |
| 003 | `MaGiaoDichCong`, `MaPhanHoiCong`, `NgayCapNhat` | chỉ thêm |
| 004 | Thứ tự/loại ảnh, ảnh riêng từng phòng (lấy từ bộ ảnh khách sạn), loại hình/chính sách hủy mặc định, danh mục bài viết, đánh dấu đánh giá mẫu | cập nhật các cột **mới**/trống |
| 005 | Cột `DiaDiem.Vung` (Bắc/Trung/Nam); mở rộng dữ liệu: +7 điểm đến, +15 khách sạn, ~45 phòng, ảnh, 14 điểm tham quan, 8 bài viết, 3 khuyến mãi, đánh giá mẫu (`LaDuLieuMau=1`) | thêm dòng, không sửa dữ liệu cũ; ảnh khách sạn mới dùng lại ảnh minh hoạ có sẵn |
| 006 | Chuyến bay/vé: mã sân bay IATA, loại máy bay, cửa ra, trạng thái chuyến (Scheduled/Delayed/Departed/Arrived/Cancelled), hành lý, giá thương gia; vé: mã vé, hạng ghế, liên hệ, lý do hủy, `LaDuLieuMau`. Thêm 2 hãng bay, 26 chuyến, 20 vé **mẫu** | chỉ thêm cột/dòng; vé mẫu đánh dấu `LaDuLieuMau=1` và hiện nhãn "mẫu" trong Admin |

Backup trước 001 đã tạo: `C:\Temp\thbak\TravelHubDB_pre001.bak` (máy dev).

## Seed
Dữ liệu mẫu có sẵn trong DB (5 điểm đến, 5 chỗ nghỉ, 8 loại phòng, bài viết, đánh giá — đánh giá được đánh dấu `LaDuLieuMau=1`). Không có giao dịch thanh toán giả và không có tài khoản mật khẩu yếu mới.
