/* 005 - Mở rộng dữ liệu mẫu (đánh dấu LaDuLieuMau cho đánh giá) + cột Vung cho điểm đến phục vụ menu/lọc.
   Idempotent: mỗi khối chỉ chèn khi chưa tồn tại (so theo tên/mã). Không sửa/xóa dữ liệu cũ.
   LƯU Ý: ảnh của chỗ nghỉ/phòng mới dùng lại ảnh minh họa đã có (placeholder) — thay bằng ảnh thật qua Admin. */
SET NOCOUNT ON;
IF COL_LENGTH('dbo.DiaDiem','Vung') IS NULL ALTER TABLE dbo.DiaDiem ADD Vung nvarchar(20) NULL;   -- Bắc | Trung | Nam
GO
UPDATE dbo.DiaDiem SET Vung = CASE TenDiaDiem WHEN N'Đà Nẵng' THEN N'Trung' WHEN N'Đà Lạt' THEN N'Trung' WHEN N'Nha Trang' THEN N'Trung' WHEN N'Phú Quốc' THEN N'Nam' WHEN N'Vũng Tàu' THEN N'Nam' ELSE Vung END WHERE Vung IS NULL;

/* ---------- Điểm đến mới ---------- */
INSERT dbo.DiaDiem (TenDiaDiem, ThanhPho, QuocGia, MoTa, HinhAnh, PhongCach, TrangThai, ViDo, KinhDo, Vung)
SELECT v.* FROM (VALUES
 (N'Hà Nội', N'Hà Nội', N'Việt Nam', N'Thủ đô nghìn năm văn hiến với phố cổ, hồ Hoàn Kiếm và ẩm thực đường phố nổi tiếng.', '/Content/images/destinations/hanoi.jpg', N'Thành phố', 1, 21.028500, 105.854200, N'Bắc'),
 (N'TP. Hồ Chí Minh', N'TP. Hồ Chí Minh', N'Việt Nam', N'Đô thị năng động bậc nhất cả nước: mua sắm, ẩm thực, cuộc sống về đêm và các điểm di tích.', '/Content/images/destinations/hcm.jpg', N'Thành phố', 1, 10.823100, 106.629700, N'Nam'),
 (N'Hội An', N'Quảng Nam', N'Việt Nam', N'Phố cổ đèn lồng được UNESCO công nhận, làng nghề, biển An Bàng và ẩm thực đặc sắc.', '/Content/images/destinations/hoian.jpg', N'Văn hóa', 1, 15.880100, 108.338000, N'Trung'),
 (N'Huế', N'Thừa Thiên Huế', N'Việt Nam', N'Cố đô với Đại Nội, lăng tẩm, sông Hương và nhã nhạc cung đình.', '/Content/images/destinations/hue.jpg', N'Văn hóa', 1, 16.463700, 107.590900, N'Trung'),
 (N'Sa Pa', N'Lào Cai', N'Việt Nam', N'Thị trấn trong mây với ruộng bậc thang, bản làng người dân tộc và đỉnh Fansipan.', '/Content/images/destinations/sapa.jpg', N'Núi', 1, 22.336400, 103.843800, N'Bắc'),
 (N'Hạ Long', N'Quảng Ninh', N'Việt Nam', N'Vịnh di sản thiên nhiên thế giới với hàng nghìn đảo đá vôi và du thuyền ngắm cảnh.', '/Content/images/destinations/halong1.jpg', N'Biển', 1, 20.950100, 107.073400, N'Bắc'),
 (N'Ninh Bình', N'Ninh Bình', N'Việt Nam', N'Tràng An, Tam Cốc, Bái Đính — cảnh quan núi đá vôi và dòng sông thơ mộng.', '/Content/images/destinations/ninhbinh.jpg', N'Thiên nhiên', 1, 20.250600, 105.974500, N'Bắc')
) v(TenDiaDiem, ThanhPho, QuocGia, MoTa, HinhAnh, PhongCach, TrangThai, ViDo, KinhDo, Vung)
WHERE NOT EXISTS (SELECT 1 FROM dbo.DiaDiem d WHERE d.TenDiaDiem = v.TenDiaDiem);

/* ---------- Chỗ nghỉ mới ---------- */
CREATE TABLE #H (TenKS nvarchar(150), Dest nvarchar(100), DiaChi nvarchar(255), Sao int, Gia decimal(18,2), Loai nvarchar(50), MoTa nvarchar(max), Sang bit, Huy varchar(20), Tel varchar(20), ChinhSach nvarchar(max));
INSERT #H VALUES
 (N'Hanoi Old Quarter Boutique', N'Hà Nội', N'28 Hàng Gai, Hoàn Kiếm, Hà Nội', 4, 1100000, N'Khách sạn', N'Khách sạn boutique giữa phố cổ, đi bộ tới hồ Hoàn Kiếm và chợ đêm.', 1, 'FLEX1', '02439000111', N'Nhận phòng 14:00, trả phòng 12:00. Không hút thuốc trong phòng.'),
 (N'West Lake Residence', N'Hà Nội', N'12 Quảng An, Tây Hồ, Hà Nội', 5, 2400000, N'Căn hộ', N'Căn hộ dịch vụ cao cấp view Hồ Tây, đầy đủ bếp và phòng khách.', 1, 'FREE24', '02439000222', N'Nhận phòng 15:00, trả phòng 12:00. Cho phép thú cưng nhỏ khi báo trước.'),
 (N'Saigon Riverside Hotel', N'TP. Hồ Chí Minh', N'3 Tôn Đức Thắng, Quận 1, TP. Hồ Chí Minh', 5, 2200000, N'Khách sạn', N'Khách sạn bên sông Sài Gòn, hồ bơi vô cực và nhà hàng trên tầng thượng.', 1, 'FLEX1', '02838000111', N'Nhận phòng 14:00, trả phòng 12:00.'),
 (N'District 1 Central Inn', N'TP. Hồ Chí Minh', N'45 Lê Thánh Tôn, Quận 1, TP. Hồ Chí Minh', 3, 750000, N'Khách sạn', N'Khách sạn gọn gàng ngay trung tâm, thuận tiện đi chợ Bến Thành và phố đi bộ.', 0, 'FREE24', '02838000222', N'Nhận phòng 14:00, trả phòng 12:00.'),
 (N'Hoi An Lantern Villa', N'Hội An', N'88 Cẩm Châu, Hội An, Quảng Nam', 4, 1600000, N'Biệt thự', N'Biệt thự có hồ bơi riêng cách phố cổ 10 phút đi xe đạp.', 1, 'FLEX1', '02353000111', N'Nhận phòng 14:00, trả phòng 11:00. Miễn phí xe đạp cho khách.'),
 (N'An Bang Beach Homestay', N'Hội An', N'120 Nguyễn Phan Vinh, An Bàng, Hội An', 3, 650000, N'Homestay', N'Homestay thân thiện cách biển An Bàng vài bước chân.', 1, 'FREE24', '02353000222', N'Nhận phòng 13:00, trả phòng 11:00.'),
 (N'Hue Imperial Hotel', N'Huế', N'15 Lê Lợi, Huế', 4, 1050000, N'Khách sạn', N'Khách sạn bên sông Hương, gần Đại Nội và cầu Trường Tiền.', 1, 'FLEX1', '02343000111', N'Nhận phòng 14:00, trả phòng 12:00.'),
 (N'Perfume River Homestay', N'Huế', N'7 Kiệt 24 Phạm Hồng Thái, Huế', 3, 600000, N'Homestay', N'Homestay yên tĩnh trong khu vườn nhỏ, chủ nhà hướng dẫn ẩm thực Huế.', 1, 'NONREFUND', '02343000222', N'Nhận phòng 13:00, trả phòng 11:00. Giá ưu đãi không hoàn hủy.'),
 (N'Sapa Cloud View Lodge', N'Sa Pa', N'33 Mường Hoa, Sa Pa, Lào Cai', 4, 1350000, N'Resort', N'Lodge nhìn ra thung lũng Mường Hoa, lò sưởi và nhà hàng đặc sản vùng cao.', 1, 'FLEX1', '02143000111', N'Nhận phòng 14:00, trả phòng 12:00. Có sưởi trong phòng.'),
 (N'Fansipan Mountain Homestay', N'Sa Pa', N'9 Cát Cát, Sa Pa, Lào Cai', 3, 700000, N'Homestay', N'Homestay của người dân bản địa, bữa tối cùng gia đình chủ nhà.', 0, 'FREE24', '02143000222', N'Nhận phòng 13:00, trả phòng 11:00.'),
 (N'Halong Bay Panorama Hotel', N'Hạ Long', N'18 Hạ Long, Bãi Cháy, Quảng Ninh', 5, 2000000, N'Khách sạn', N'Khách sạn 5 sao nhìn toàn cảnh vịnh Hạ Long, hồ bơi và spa.', 1, 'FLEX1', '02033000111', N'Nhận phòng 14:00, trả phòng 12:00.'),
 (N'Bai Chay Beach Hotel', N'Hạ Long', N'60 Vườn Đào, Bãi Cháy, Quảng Ninh', 3, 800000, N'Khách sạn', N'Khách sạn giá tốt gần bãi tắm Bãi Cháy và cảng du thuyền.', 0, 'FREE24', '02033000222', N'Nhận phòng 14:00, trả phòng 12:00.'),
 (N'Trang An Eco Resort', N'Ninh Bình', N'Khu du lịch Tràng An, Hoa Lư, Ninh Bình', 4, 1500000, N'Resort', N'Resort sinh thái giữa cánh đồng và núi đá vôi, gần Tràng An và Tam Cốc.', 1, 'FLEX1', '02293000111', N'Nhận phòng 14:00, trả phòng 12:00.'),
 (N'Son Tra Peninsula Resort', N'Đà Nẵng', N'Bãi Bụt, Sơn Trà, Đà Nẵng', 5, 3200000, N'Resort', N'Resort biển riêng trên bán đảo Sơn Trà, villa hướng biển và spa.', 1, 'FREE24', '02363000111', N'Nhận phòng 15:00, trả phòng 12:00.'),
 (N'Nha Trang Bay Hotel', N'Nha Trang', N'88 Trần Phú, Nha Trang, Khánh Hòa', 4, 1250000, N'Khách sạn', N'Khách sạn view vịnh Nha Trang, gần chợ đêm và quảng trường biển.', 1, 'FLEX1', '02583000111', N'Nhận phòng 14:00, trả phòng 12:00.');

INSERT dbo.KhachSan (TenKS, DiaChi, MaDiaDiem, MoTa, SoSao, GiaTu, SoDienThoai, TrangThai, NgayTao, LoaiHinh, ChinhSach, ChinhSachHuyLoai, BuaSang, ViDo, KinhDo)
SELECT h.TenKS, h.DiaChi, d.MaDiaDiem, h.MoTa, h.Sao, h.Gia, h.Tel, 1, GETDATE(), h.Loai, h.ChinhSach, h.Huy, h.Sang, d.ViDo, d.KinhDo
FROM #H h JOIN dbo.DiaDiem d ON d.TenDiaDiem = h.Dest
WHERE NOT EXISTS (SELECT 1 FROM dbo.KhachSan k WHERE k.TenKS = h.TenKS);

/* ---------- Phòng (chỉ cho chỗ nghỉ chưa có phòng) ---------- */
CREATE TABLE #R (Ten nvarchar(100), Loai nvarchar(100), Khach int, Giuong int, Heso decimal(4,2), SoLuong int, Anh varchar(255), DienTich decimal(6,1), MoTa nvarchar(500));
INSERT #R VALUES
 (N'Phòng Tiêu chuẩn', N'Phòng đôi', 2, 1, 1.00, 10, '/Content/images/rooms/standard-room.jpg', 24, N'Phòng gọn gàng, đầy đủ tiện nghi cơ bản.'),
 (N'Phòng Cao cấp', N'Phòng đôi/twin', 2, 1, 1.35, 8, '/Content/images/rooms/deluxe-double.jpg', 32, N'Phòng rộng hơn, view đẹp, bàn làm việc và khu vực tiếp khách nhỏ.'),
 (N'Phòng Gia đình', N'Phòng gia đình', 4, 2, 1.90, 4, '/Content/images/rooms/family-room.jpg', 48, N'Hai giường lớn, phù hợp gia đình 3–4 người.');
INSERT dbo.Phong (MaKS, TenPhong, LoaiPhong, SoNguoiToiDa, SoGiuong, DienTich, GiaPhong, SoLuong, MoTa, HinhAnh, TrangThai)
SELECT k.MaKS, r.Ten, r.Loai, r.Khach, r.Giuong, r.DienTich, ROUND(k.GiaTu * r.Heso / 10000, 0) * 10000, r.SoLuong, r.MoTa, r.Anh, 1
FROM dbo.KhachSan k CROSS JOIN #R r
WHERE k.TenKS IN (SELECT TenKS FROM #H) AND NOT EXISTS (SELECT 1 FROM dbo.Phong p WHERE p.MaKS = k.MaKS);

/* ---------- Tiện nghi ---------- */
INSERT dbo.KhachSan_TienIch (MaKS, MaTienIch)
SELECT k.MaKS, t.MaTienIch FROM dbo.KhachSan k CROSS JOIN dbo.TienIch t
WHERE k.TenKS IN (SELECT TenKS FROM #H) AND t.TrangThai = 1
  AND (t.MaTienIch IN (1, 7) OR (k.MaKS * 3 + t.MaTienIch) % 3 <> 0 OR (t.MaTienIch = 2 AND k.SoSao >= 4))
  AND NOT EXISTS (SELECT 1 FROM dbo.KhachSan_TienIch x WHERE x.MaKS = k.MaKS AND x.MaTienIch = t.MaTienIch);

/* ---------- Ảnh chỗ nghỉ (tái dùng ảnh minh họa) ---------- */
CREATE TABLE #Pool (Kind int, N int, Path varchar(255), Loai nvarchar(30), MoTa nvarchar(255));
INSERT #Pool VALUES
 (1,1,'/Content/images/hotels/danang-1.jpg',N'Mặt tiền',N'Mặt tiền'),(1,2,'/Content/images/hotels/dalat-1.jpg',N'Mặt tiền',N'Sảnh và mặt tiền'),(1,3,'/Content/images/hotels/nhatrang-1.jpg',N'Mặt tiền',N'Mặt tiền'),(1,4,'/Content/images/hotels/vungtau-1.jpg',N'Mặt tiền',N'Mặt tiền'),(1,5,'/Content/images/hotels/phuquoc-1.jpg',N'Mặt tiền',N'Khu resort'),
 (2,1,'/Content/images/hotels/danang-2.jpg',N'Phòng ngủ',N'Phòng nghỉ'),(2,2,'/Content/images/hotels/dalat-2.jpg',N'Phòng ngủ',N'Phòng nghỉ'),(2,3,'/Content/images/hotels/nhatrang-2.jpg',N'Phòng ngủ',N'Phòng nghỉ'),(2,4,'/Content/images/hotels/vungtau-2.jpg',N'Phòng ngủ',N'Phòng nghỉ'),(2,5,'/Content/images/hotels/phuquoc-2.jpg',N'Phòng ngủ',N'Phòng nghỉ'),
 (3,1,'/Content/images/hotels/danang-3.jpg',N'Tiện nghi',N'Hồ bơi / sân vườn'),(3,2,'/Content/images/hotels/phuquoc-3.jpg',N'Tiện nghi',N'Hồ bơi'),(3,3,'/Content/images/hotels/dalat-3.jpg',N'Khu vực chung',N'Khu vườn'),(3,4,'/Content/images/hotels/nhatrang-3.jpg',N'Khu vực chung',N'Khu vực chung'),
 (4,1,'/Content/images/hotels/danang-4.jpg',N'View',N'View từ ban công'),(4,2,'/Content/images/hotels/phuquoc-sunset.jpg',N'View',N'Hoàng hôn'),(4,3,'/Content/images/hotels/vungtau-beach.jpg',N'View',N'View biển');
;WITH nh AS (SELECT k.MaKS, ROW_NUMBER() OVER (ORDER BY k.MaKS) rn FROM dbo.KhachSan k WHERE k.TenKS IN (SELECT TenKS FROM #H) AND NOT EXISTS (SELECT 1 FROM dbo.HinhAnhKhachSan i WHERE i.MaKS = k.MaKS))
INSERT dbo.HinhAnhKhachSan (MaKS, DuongDanAnh, MoTa, LaAnhDaiDien, Loai, ThuTu, HienThi)
SELECT nh.MaKS, p.Path, p.MoTa, CASE WHEN p.Kind = 1 THEN 1 ELSE 0 END, p.Loai, p.Kind, 1
FROM nh JOIN (SELECT Kind, COUNT(*) c FROM #Pool GROUP BY Kind) cnt ON 1=1
JOIN #Pool p ON p.Kind = cnt.Kind AND p.N = ((nh.rn - 1) % cnt.c) + 1;
UPDATE k SET HinhAnhDaiDien = i.DuongDanAnh FROM dbo.KhachSan k JOIN dbo.HinhAnhKhachSan i ON i.MaKS = k.MaKS AND i.LaAnhDaiDien = 1 WHERE k.TenKS IN (SELECT TenKS FROM #H) AND k.HinhAnhDaiDien IS NULL;
-- Ảnh riêng cho phòng mới: ảnh chính + 2 ảnh từ bộ ảnh của chính chỗ nghỉ
INSERT dbo.HinhAnhPhong (MaPhong, DuongDanAnh, MoTa, Loai, ThuTu, LaAnhDaiDien)
SELECT p.MaPhong, p.HinhAnh, p.TenPhong, N'Phòng ngủ', 1, 1 FROM dbo.Phong p JOIN dbo.KhachSan k ON k.MaKS = p.MaKS
WHERE k.TenKS IN (SELECT TenKS FROM #H) AND p.HinhAnh IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.HinhAnhPhong x WHERE x.MaPhong = p.MaPhong);
INSERT dbo.HinhAnhPhong (MaPhong, DuongDanAnh, MoTa, Loai, ThuTu, LaAnhDaiDien)
SELECT p.MaPhong, i.DuongDanAnh, i.MoTa, ISNULL(i.Loai, N'Khu vực chung'), 1 + ROW_NUMBER() OVER (PARTITION BY p.MaPhong ORDER BY i.ThuTu), 0
FROM dbo.Phong p JOIN dbo.KhachSan k ON k.MaKS = p.MaKS JOIN dbo.HinhAnhKhachSan i ON i.MaKS = k.MaKS AND i.Loai IN (N'Phòng ngủ', N'View', N'Tiện nghi')
WHERE k.TenKS IN (SELECT TenKS FROM #H) AND i.DuongDanAnh <> p.HinhAnh AND (SELECT COUNT(*) FROM dbo.HinhAnhPhong x WHERE x.MaPhong = p.MaPhong) = 1;

/* ---------- Đánh giá mẫu (LaDuLieuMau = 1) ---------- */
CREATE TABLE #V (N int, Sao int, ND nvarchar(500));
INSERT #V VALUES (1,5,N'Vị trí thuận tiện, nhân viên thân thiện, phòng sạch sẽ. Sẽ quay lại.'),(2,4,N'Phòng đẹp, bữa sáng ổn. Chỉ hơi ồn vào buổi tối.'),(3,5,N'Dịch vụ rất tốt, check-in nhanh, view tuyệt vời.'),(4,4,N'Giá hợp lý so với chất lượng, phù hợp đi gia đình.'),(5,5,N'Kỳ nghỉ tuyệt vời, mọi thứ đúng như mô tả.'),(6,3,N'Phòng ổn nhưng wifi hơi yếu ở tầng cao.');
;WITH nh AS (SELECT k.MaKS, ROW_NUMBER() OVER (ORDER BY k.MaKS) rn FROM dbo.KhachSan k WHERE k.TenKS IN (SELECT TenKS FROM #H) AND NOT EXISTS (SELECT 1 FROM dbo.DanhGia g WHERE g.MaKS = k.MaKS)),
cus AS (SELECT MaKH, ROW_NUMBER() OVER (ORDER BY MaKH) cn FROM dbo.KhachHang)
INSERT dbo.DanhGia (MaKH, MaKS, SoSao, NoiDung, NgayDanhGia, TrangThai, LaDuLieuMau)
SELECT c.MaKH, nh.MaKS, v.Sao, v.ND, DATEADD(day, -((nh.rn * 7 + v.N * 5) % 120), GETDATE()), 1, 1
FROM nh JOIN #V v ON v.N IN (((nh.rn - 1) % 6) + 1, ((nh.rn + 2) % 6) + 1)
JOIN cus c ON c.cn = ((nh.rn + v.N) % (SELECT COUNT(*) FROM dbo.KhachHang)) + 1;

/* ---------- Địa điểm tham quan ---------- */
INSERT dbo.DiaDiemDuLich (MaDiaDiem, TenDiaDiem, LoaiDiaDiem, DiaChi, MoTa, HinhAnh, GiaVe, NoiBat, TrangThai)
SELECT d.MaDiaDiem, a.Ten, a.Loai, a.DiaChi, a.MoTa, d.HinhAnh, NULL, 1, 1
FROM (VALUES
 (N'Hà Nội', N'Hồ Hoàn Kiếm', N'Hồ', N'Trung tâm Hà Nội', N'Hồ nước giữa lòng thủ đô, đền Ngọc Sơn và Tháp Rùa.'),
 (N'Hà Nội', N'Văn Miếu – Quốc Tử Giám', N'Di tích', N'58 Quốc Tử Giám, Đống Đa', N'Trường đại học đầu tiên của Việt Nam.'),
 (N'TP. Hồ Chí Minh', N'Dinh Độc Lập', N'Di tích', N'135 Nam Kỳ Khởi Nghĩa, Quận 1', N'Công trình lịch sử gắn với ngày thống nhất đất nước.'),
 (N'TP. Hồ Chí Minh', N'Chợ Bến Thành', N'Chợ', N'Lê Lợi, Quận 1', N'Biểu tượng mua sắm và ẩm thực của thành phố.'),
 (N'Hội An', N'Phố cổ Hội An', N'Di tích', N'Trần Phú, Hội An', N'Khu phố cổ đèn lồng, di sản thế giới.'),
 (N'Hội An', N'Biển An Bàng', N'Bãi biển', N'An Bàng, Hội An', N'Bãi biển yên tĩnh cách phố cổ khoảng 5 km.'),
 (N'Huế', N'Đại Nội Huế', N'Di tích', N'Phú Hậu, Huế', N'Hoàng thành triều Nguyễn.'),
 (N'Huế', N'Chùa Thiên Mụ', N'Di tích', N'Hương Long, Huế', N'Ngôi chùa cổ bên sông Hương.'),
 (N'Sa Pa', N'Đỉnh Fansipan', N'Thiên nhiên', N'Sa Pa, Lào Cai', N'“Nóc nhà Đông Dương”, đi cáp treo hoặc trekking.'),
 (N'Sa Pa', N'Bản Cát Cát', N'Làng nghề', N'Sa Pa, Lào Cai', N'Bản làng người H’Mông với nghề dệt và thác nước.'),
 (N'Hạ Long', N'Vịnh Hạ Long', N'Thiên nhiên', N'Hạ Long, Quảng Ninh', N'Du thuyền ngắm đảo đá vôi và hang động.'),
 (N'Hạ Long', N'Hang Sửng Sốt', N'Hang động', N'Vịnh Hạ Long', N'Một trong những hang động đẹp nhất vịnh.'),
 (N'Ninh Bình', N'Khu du lịch Tràng An', N'Thiên nhiên', N'Hoa Lư, Ninh Bình', N'Đi thuyền qua các hang động và núi đá vôi.'),
 (N'Ninh Bình', N'Chùa Bái Đính', N'Di tích', N'Gia Sinh, Ninh Bình', N'Quần thể chùa lớn bậc nhất Việt Nam.')
) a(Dest, Ten, Loai, DiaChi, MoTa) JOIN dbo.DiaDiem d ON d.TenDiaDiem = a.Dest
WHERE NOT EXISTS (SELECT 1 FROM dbo.DiaDiemDuLich x WHERE x.TenDiaDiem = a.Ten);

/* ---------- Bài viết ---------- */
INSERT dbo.BaiViet (MaDiaDiem, TieuDe, MoTaNgan, NoiDung, HinhAnh, TacGia, NgayDang, LuotXem, NoiBat, TrangThai, DanhMuc)
SELECT d.MaDiaDiem, a.Tieu, a.Ngan, a.ND, d.HinhAnh, N'Biên tập TravelHub', DATEADD(day, -a.Ago, GETDATE()), 0, a.Nb, 1, a.DM
FROM (VALUES
 (N'Hà Nội', N'Một ngày khám phá phố cổ Hà Nội', N'Gợi ý lộ trình đi bộ quanh hồ Hoàn Kiếm và 36 phố phường.', N'Bắt đầu buổi sáng bằng một tô phở nóng rồi đi dạo quanh hồ Hoàn Kiếm.' + CHAR(13)+CHAR(10)+CHAR(13)+CHAR(10) + N'Buổi chiều ghé Văn Miếu – Quốc Tử Giám, tối thưởng thức bún chả và cà phê trứng.', 3, 1, N'Kinh nghiệm'),
 (N'Hà Nội', N'Ăn gì ở Hà Nội: 8 món nhất định phải thử', N'Phở, bún chả, chả cá, bánh cuốn… và nơi thưởng thức.', N'Ẩm thực Hà Nội nổi tiếng với hương vị thanh, tinh tế.' + CHAR(13)+CHAR(10)+CHAR(13)+CHAR(10) + N'Hãy thử phở bò, bún chả, bánh cuốn Thanh Trì và chè Thái.', 6, 0, N'Ẩm thực'),
 (N'TP. Hồ Chí Minh', N'Sài Gòn 48 giờ: lịch trình cho người đi lần đầu', N'Từ Dinh Độc Lập đến chợ Bến Thành và phố đi bộ Nguyễn Huệ.', N'Ngày đầu tham quan khu trung tâm: Dinh Độc Lập, Nhà thờ Đức Bà, Bưu điện thành phố.' + CHAR(13)+CHAR(10)+CHAR(13)+CHAR(10) + N'Ngày hai ăn sáng bánh mì, đi chợ Bến Thành và thư giãn trên sông Sài Gòn.', 9, 1, N'Kinh nghiệm'),
 (N'Hội An', N'Hội An mùa đèn lồng: nên đi vào lúc nào?', N'Thời điểm đẹp, mẹo tránh đông và chỗ ăn ngon.', N'Phố cổ đẹp nhất lúc chiều muộn khi đèn lồng bắt đầu thắp sáng.' + CHAR(13)+CHAR(10)+CHAR(13)+CHAR(10) + N'Nên đến vào ngày thường và tránh các dịp lễ lớn để đỡ đông.', 12, 1, N'Điểm đến'),
 (N'Huế', N'Một vòng cố đô Huế qua Đại Nội và lăng tẩm', N'Cách sắp xếp thời gian để tham quan trọn vẹn.', N'Dành nửa ngày cho Đại Nội, nửa ngày cho lăng Khải Định hoặc Tự Đức.' + CHAR(13)+CHAR(10)+CHAR(13)+CHAR(10) + N'Buổi tối đi thuyền trên sông Hương nghe ca Huế.', 15, 0, N'Điểm đến'),
 (N'Sa Pa', N'Sa Pa nên đi mùa nào để săn mây và ruộng bậc thang?', N'Lịch mùa vụ và trang phục cần chuẩn bị.', N'Mùa lúa chín (tháng 9–10) cho ruộng bậc thang vàng rực, mùa đông có thể có băng giá.' + CHAR(13)+CHAR(10)+CHAR(13)+CHAR(10) + N'Hãy mang áo ấm và giày chống trượt.', 18, 1, N'Mẹo du lịch'),
 (N'Hạ Long', N'Du thuyền Hạ Long 2 ngày 1 đêm: có gì đáng giá?', N'Lịch trình, chi phí tham khảo và lưu ý khi đặt.', N'Hầu hết hành trình gồm tham quan hang, chèo kayak và nghỉ đêm trên vịnh.' + CHAR(13)+CHAR(10)+CHAR(13)+CHAR(10) + N'Hãy kiểm tra kỹ chính sách hủy và thời tiết trước khi đặt.', 21, 0, N'Kinh nghiệm'),
 (N'Ninh Bình', N'Ninh Bình trong một ngày: Tràng An – Bái Đính – Hang Múa', N'Lộ trình gợi ý cho chuyến đi từ Hà Nội.', N'Xuất phát sớm để tránh đông, đi thuyền Tràng An buổi sáng, chùa Bái Đính buổi chiều.' + CHAR(13)+CHAR(10)+CHAR(13)+CHAR(10) + N'Leo Hang Múa lúc hoàng hôn để ngắm toàn cảnh.', 24, 0, N'Điểm đến')
) a(Dest, Tieu, Ngan, ND, Ago, Nb, DM) JOIN dbo.DiaDiem d ON d.TenDiaDiem = a.Dest
WHERE NOT EXISTS (SELECT 1 FROM dbo.BaiViet b WHERE b.TieuDe = a.Tieu);

/* ---------- Khuyến mãi ---------- */
INSERT dbo.KhuyenMai (TenKM, MaCode, PhanTramGiam, SoTienGiam, GiaTriToiThieu, NgayBatDau, NgayKetThuc, SoLuong, MoTa, TrangThai, SoLuongDaDung, SoLanToiDaMoiKH, GiamToiDa)
SELECT * FROM (VALUES
 (N'Khởi đầu năm mới', 'NEWYEAR27', 12.00, NULL, 1000000.00, '2026-12-01', '2027-03-31', 500, N'Giảm 12% cho đơn từ 1.000.000đ (tối đa 400.000đ).', 1, 0, 1, 400000.00),
 (N'Hè rực rỡ', 'HE2027', 15.00, NULL, 1500000.00, '2027-04-01', '2027-08-31', 400, N'Giảm 15% cho đơn từ 1.500.000đ (tối đa 600.000đ).', 1, 0, 2, 600000.00),
 (N'Cuối tuần thư giãn', 'WEEKEND100', NULL, 100000.00, 800000.00, '2026-11-01', '2027-06-30', 800, N'Giảm 100.000đ cho đơn từ 800.000đ.', 1, 0, 3, 100000.00)
) v(TenKM, MaCode, Pct, Amt, Min, Bd, Kt, Sl, MoTa, TT, Dd, Mk, Max)
WHERE NOT EXISTS (SELECT 1 FROM dbo.KhuyenMai k WHERE k.MaCode = v.MaCode);

DROP TABLE #H; DROP TABLE #R; DROP TABLE #Pool; DROP TABLE #V;
PRINT N'005 OK';
