/* 004 - Sắp xếp/ẩn ảnh khách sạn + ảnh riêng cho từng phòng + dữ liệu mẫu bổ sung (đánh dấu là seed).
   Idempotent. Chỉ thêm dữ liệu, không sửa/xóa dữ liệu hiện có (trừ cập nhật các cột MỚI thêm). */
SET NOCOUNT ON;

IF COL_LENGTH('dbo.HinhAnhKhachSan','ThuTu') IS NULL ALTER TABLE dbo.HinhAnhKhachSan ADD ThuTu int NOT NULL CONSTRAINT DF_HAKS_ThuTu DEFAULT 0;
IF COL_LENGTH('dbo.HinhAnhKhachSan','Loai') IS NULL ALTER TABLE dbo.HinhAnhKhachSan ADD Loai nvarchar(30) NULL;
IF COL_LENGTH('dbo.HinhAnhKhachSan','HienThi') IS NULL ALTER TABLE dbo.HinhAnhKhachSan ADD HienThi bit NOT NULL CONSTRAINT DF_HAKS_HienThi DEFAULT 1;
EXEC('UPDATE dbo.HinhAnhKhachSan SET ThuTu = MaHinhAnh WHERE ThuTu = 0');

GO
-- Phân loại ảnh khách sạn theo mô tả hiện có
EXEC('UPDATE dbo.HinhAnhKhachSan SET Loai = CASE WHEN MoTa LIKE N''%Mặt tiền%'' THEN N''Mặt tiền'' WHEN MoTa LIKE N''%Phòng%'' THEN N''Phòng ngủ'' WHEN MoTa LIKE N''%bơi%'' THEN N''Tiện nghi'' WHEN MoTa LIKE N''%view%'' OR MoTa LIKE N''%biển%'' THEN N''View'' ELSE N''Khu vực chung'' END WHERE Loai IS NULL');

-- Loại hình / chính sách hủy cho dữ liệu hiện có (chỉ điền khi còn trống)
EXEC('UPDATE dbo.KhachSan SET LoaiHinh = N''Khách sạn'' WHERE LoaiHinh IS NULL');
EXEC('UPDATE dbo.KhachSan SET LoaiHinh = N''Resort'' WHERE MaKS = 4 AND LoaiHinh = N''Khách sạn''');
EXEC('UPDATE dbo.KhachSan SET BuaSang = 1 WHERE MaKS IN (1,3,4)');
EXEC('UPDATE dbo.KhachSan SET ChinhSachHuyLoai = ''FREE24'' WHERE MaKS = 5 AND ChinhSachHuyLoai = ''FLEX1''');
EXEC('UPDATE dbo.KhachSan SET ChinhSachHuyLoai = ''NONREFUND'' WHERE MaKS = 2 AND ChinhSachHuyLoai = ''FLEX1''');

-- Ảnh riêng cho từng phòng: ảnh chính của phòng + ảnh bổ sung (không dùng chung 1 ảnh cho mọi phòng)
IF NOT EXISTS (SELECT 1 FROM dbo.HinhAnhPhong)
BEGIN
    INSERT dbo.HinhAnhPhong (MaPhong, DuongDanAnh, MoTa, Loai, ThuTu, LaAnhDaiDien)
    SELECT MaPhong, HinhAnh, TenPhong, N'Phòng ngủ', 1, 1 FROM dbo.Phong WHERE HinhAnh IS NOT NULL;

    -- Ảnh bổ sung lấy từ bộ ảnh của chính khách sạn (phòng nghỉ / view / tiện nghi)
    INSERT dbo.HinhAnhPhong (MaPhong, DuongDanAnh, MoTa, Loai, ThuTu, LaAnhDaiDien)
    SELECT p.MaPhong, h.DuongDanAnh, h.MoTa, ISNULL(h.Loai, N'Khu vực chung'),
           1 + ROW_NUMBER() OVER (PARTITION BY p.MaPhong ORDER BY h.ThuTu), 0
    FROM dbo.Phong p
    JOIN dbo.HinhAnhKhachSan h ON h.MaKS = p.MaKS
    WHERE h.Loai IN (N'Phòng ngủ', N'View', N'Tiện nghi') AND h.DuongDanAnh <> ISNULL(p.HinhAnh, '');
END

-- Ngày sinh/giới tính không bắt buộc; đánh dấu đánh giá mẫu hiện có
EXEC('UPDATE dbo.DanhGia SET LaDuLieuMau = 1 WHERE LaDuLieuMau = 0');

-- Danh mục bài viết (điền khi còn trống)
EXEC('UPDATE dbo.BaiViet SET DanhMuc = CASE WHEN TieuDe LIKE N''%Ăn gì%'' THEN N''Ẩm thực'' WHEN TieuDe LIKE N''%Kinh nghiệm%'' OR TieuDe LIKE N''%Cuối tuần%'' THEN N''Kinh nghiệm'' ELSE N''Điểm đến'' END WHERE DanhMuc IS NULL');

PRINT N'004 OK';
