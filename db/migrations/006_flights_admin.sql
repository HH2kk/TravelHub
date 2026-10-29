/* 006 - Mở rộng module chuyến bay / vé máy bay phục vụ trang Admin.
   Chỉ THÊM cột/dữ liệu, chạy lại nhiều lần an toàn (chuyến nhận diện theo mã chuyến).
   Dữ liệu vé thêm vào là DỮ LIỆU MẪU (VeMayBay.LaDuLieuMau=1), không phải giao dịch thật. */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;

IF COL_LENGTH('ChuyenBay','MaSanBayDi') IS NULL ALTER TABLE ChuyenBay ADD MaSanBayDi varchar(4) NULL;
IF COL_LENGTH('ChuyenBay','MaSanBayDen') IS NULL ALTER TABLE ChuyenBay ADD MaSanBayDen varchar(4) NULL;
IF COL_LENGTH('ChuyenBay','LoaiMayBay') IS NULL ALTER TABLE ChuyenBay ADD LoaiMayBay nvarchar(60) NULL;
IF COL_LENGTH('ChuyenBay','CuaRa') IS NULL ALTER TABLE ChuyenBay ADD CuaRa nvarchar(10) NULL;
IF COL_LENGTH('ChuyenBay','TrangThaiChuyen') IS NULL ALTER TABLE ChuyenBay ADD TrangThaiChuyen varchar(15) NOT NULL CONSTRAINT DF_ChuyenBay_TTC DEFAULT 'Scheduled';
IF COL_LENGTH('ChuyenBay','HanhLyKg') IS NULL ALTER TABLE ChuyenBay ADD HanhLyKg int NOT NULL CONSTRAINT DF_ChuyenBay_HL DEFAULT 20;
IF COL_LENGTH('ChuyenBay','GiaThuongGia') IS NULL ALTER TABLE ChuyenBay ADD GiaThuongGia decimal(18,2) NULL;
IF COL_LENGTH('ChuyenBay','GhiChu') IS NULL ALTER TABLE ChuyenBay ADD GhiChu nvarchar(300) NULL;

IF COL_LENGTH('VeMayBay','MaVeCode') IS NULL ALTER TABLE VeMayBay ADD MaVeCode varchar(20) NULL;
IF COL_LENGTH('VeMayBay','HangGhe') IS NULL ALTER TABLE VeMayBay ADD HangGhe nvarchar(20) NULL;
IF COL_LENGTH('VeMayBay','EmailLienHe') IS NULL ALTER TABLE VeMayBay ADD EmailLienHe varchar(150) NULL;
IF COL_LENGTH('VeMayBay','SdtLienHe') IS NULL ALTER TABLE VeMayBay ADD SdtLienHe varchar(20) NULL;
IF COL_LENGTH('VeMayBay','HanhLyKg') IS NULL ALTER TABLE VeMayBay ADD HanhLyKg int NULL;
IF COL_LENGTH('VeMayBay','LyDoHuy') IS NULL ALTER TABLE VeMayBay ADD LyDoHuy nvarchar(300) NULL;
IF COL_LENGTH('VeMayBay','NgayCapNhat') IS NULL ALTER TABLE VeMayBay ADD NgayCapNhat datetime NULL;
IF COL_LENGTH('VeMayBay','LaDuLieuMau') IS NULL ALTER TABLE VeMayBay ADD LaDuLieuMau bit NOT NULL CONSTRAINT DF_VeMayBay_Mau DEFAULT 0;
GO

/* mã vé cho vé cũ + IATA cho chuyến cũ */
UPDATE VeMayBay SET MaVeCode = 'TH-F' + RIGHT('000000' + CAST(MaVe AS varchar(10)), 6) WHERE MaVeCode IS NULL;
UPDATE VeMayBay SET HangGhe = N'Phổ thông' WHERE HangGhe IS NULL;
UPDATE VeMayBay SET HanhLyKg = 20 WHERE HanhLyKg IS NULL;
UPDATE ChuyenBay SET MaSanBayDi='SGN' WHERE MaSanBayDi IS NULL AND SanBayDi LIKE N'%Hồ Chí Minh%';
UPDATE ChuyenBay SET MaSanBayDen='DAD' WHERE MaSanBayDen IS NULL AND SanBayDen LIKE N'%Đà Nẵng%';
UPDATE ChuyenBay SET MaSanBayDen='CXR' WHERE MaSanBayDen IS NULL AND SanBayDen LIKE N'%Nha Trang%';
UPDATE ChuyenBay SET MaSanBayDen='PQC' WHERE MaSanBayDen IS NULL AND SanBayDen LIKE N'%Phú Quốc%';
UPDATE ChuyenBay SET LoaiMayBay = N'Airbus A321' WHERE LoaiMayBay IS NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_VeMayBay_MaVeCode')
    CREATE UNIQUE INDEX UX_VeMayBay_MaVeCode ON VeMayBay(MaVeCode) WHERE MaVeCode IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_VeMayBay_KH')
    CREATE INDEX IX_VeMayBay_KH ON VeMayBay(MaKH, NgayDat DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_VeMayBay_CB')
    CREATE INDEX IX_VeMayBay_CB ON VeMayBay(MaChuyenBay);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_ChuyenBay_ThoiGianDi')
    CREATE INDEX IX_ChuyenBay_ThoiGianDi ON ChuyenBay(ThoiGianDi);
GO

/* hãng bay bổ sung */
IF NOT EXISTS (SELECT 1 FROM HangBay WHERE MaIATA='BL') INSERT HangBay(TenHangBay, MaIATA, Logo, TrangThai) VALUES (N'Pacific Airlines','BL',NULL,1);
IF NOT EXISTS (SELECT 1 FROM HangBay WHERE MaIATA='SQ') INSERT HangBay(TenHangBay, MaIATA, Logo, TrangThai) VALUES (N'Singapore Airlines','SQ',NULL,1);
GO

/* chuyến bay: lịch trải từ -12 ngày đến +25 ngày quanh thời điểm chạy migration */
DECLARE @t0 date = CAST(GETDATE() AS date);
DECLARE @f TABLE (Code varchar(20), Air varchar(3), FromCode varchar(4), FromName nvarchar(100), ToCode varchar(4), ToName nvarchar(100), DayOff int, Dep time, Mins int, Price decimal(18,2), Seats int, Sold int, Plane nvarchar(60), Gate nvarchar(10));
INSERT @f VALUES
 ('VN211','VN','HAN',N'Hà Nội','SGN',N'TP. Hồ Chí Minh',-12,'06:15',130,1890000,180,171,N'Airbus A321','A3'),
 ('VJ131','VJ','SGN',N'TP. Hồ Chí Minh','HAN',N'Hà Nội',-9,'09:40',130,1250000,230,226,N'Airbus A330','B7'),
 ('QH201','QH','SGN',N'TP. Hồ Chí Minh','DAD',N'Đà Nẵng',-6,'13:10',80,1090000,170,152,N'Embraer E195','C2'),
 ('VN160','VN','HAN',N'Hà Nội','DAD',N'Đà Nẵng',-4,'07:30',85,1420000,180,164,N'Airbus A321','A5'),
 ('VJ511','VJ','HAN',N'Hà Nội','PQC',N'Phú Quốc',-3,'11:20',140,1790000,180,176,N'Airbus A320','B2'),
 ('VU701','VU','SGN',N'TP. Hồ Chí Minh','PQC',N'Phú Quốc',-2,'15:05',65,790000,150,118,N'ATR 72','D1'),
 ('BL6011','BL','SGN',N'TP. Hồ Chí Minh','HUI',N'Huế',-1,'17:45',95,990000,180,140,N'Airbus A320','C6'),
 ('VN7131','VN','SGN',N'TP. Hồ Chí Minh','DAD',N'Đà Nẵng',1,'06:00',80,1350000,180,96,N'Airbus A321','A1'),
 ('VJ623','VJ','SGN',N'TP. Hồ Chí Minh','DAD',N'Đà Nẵng',1,'12:25',80,890000,230,141,N'Airbus A321','B4'),
 ('QH1501','QH','HAN',N'Hà Nội','SGN',N'TP. Hồ Chí Minh',2,'08:10',130,1650000,170,102,N'Embraer E195','C1'),
 ('VN1553','VN','SGN',N'TP. Hồ Chí Minh','CXR',N'Nha Trang',2,'10:00',60,1050000,180,88,N'Airbus A321','A4'),
 ('VJ821','VJ','SGN',N'TP. Hồ Chí Minh','DLI',N'Đà Lạt',3,'14:30',55,720000,180,64,N'Airbus A320','B3'),
 ('VU741','VU','HAN',N'Hà Nội','DAD',N'Đà Nẵng',3,'16:20',85,1190000,150,71,N'ATR 72','D2'),
 ('VN241','VN','HAN',N'Hà Nội','SGN',N'TP. Hồ Chí Minh',4,'05:55',130,2050000,180,119,N'Airbus A350','A2'),
 ('VJ327','VJ','SGN',N'TP. Hồ Chí Minh','CXR',N'Nha Trang',4,'19:10',60,680000,230,52,N'Airbus A330','B5'),
 ('BL6023','BL','HAN',N'Hà Nội','HUI',N'Huế',5,'09:05',75,950000,180,37,N'Airbus A320','C3'),
 ('QH101','QH','SGN',N'TP. Hồ Chí Minh','HAN',N'Hà Nội',6,'07:20',130,1590000,170,80,N'Embraer E195','C4'),
 ('VN1363','VN','DAD','Đà Nẵng','SGN',N'TP. Hồ Chí Minh',7,'18:00',80,1310000,180,58,N'Airbus A321','A6'),
 ('VJ193','VJ','HAN',N'Hà Nội','SGN',N'TP. Hồ Chí Minh',8,'21:05',130,1150000,230,44,N'Airbus A330','B1'),
 ('SQ185','SQ','SGN',N'TP. Hồ Chí Minh','HAN',N'Hà Nội',9,'11:45',125,2790000,264,81,N'Boeing 787-10','E1'),
 ('VU755','VU','SGN',N'TP. Hồ Chí Minh','PQC',N'Phú Quốc',10,'08:35',65,830000,150,33,N'ATR 72','D3'),
 ('VN6211','VN','HAN',N'Hà Nội','CXR',N'Nha Trang',12,'13:40',110,1780000,180,29,N'Airbus A321','A7'),
 ('VJ451','VJ','DAD','Đà Nẵng','HAN',N'Hà Nội',14,'20:15',85,990000,180,22,N'Airbus A321','B6'),
 ('QH311','QH','SGN',N'TP. Hồ Chí Minh','DLI',N'Đà Lạt',16,'09:50',55,760000,170,18,N'Embraer E195','C5'),
 ('VN1601','VN','HAN',N'Hà Nội','DAD',N'Đà Nẵng',20,'12:00',85,1380000,180,15,N'Airbus A321','A8'),
 ('VJ701','VJ','SGN',N'TP. Hồ Chí Minh','PQC',N'Phú Quốc',25,'16:40',65,720000,180,9,N'Airbus A320','B8');

INSERT ChuyenBay (MaHangBay, MaChuyen, SanBayDi, SanBayDen, ThoiGianDi, ThoiGianDen, GiaVe, SoGhe, SoGheConLai, TrangThai, MaSanBayDi, MaSanBayDen, LoaiMayBay, CuaRa, TrangThaiChuyen, HanhLyKg, GiaThuongGia)
SELECT h.MaHangBay, f.Code, f.FromName, f.ToName,
       DATEADD(minute, DATEDIFF(minute,'00:00',f.Dep), CAST(DATEADD(day,f.DayOff,@t0) AS datetime)),
       DATEADD(minute, f.Mins + DATEDIFF(minute,'00:00',f.Dep), CAST(DATEADD(day,f.DayOff,@t0) AS datetime)),
       f.Price, f.Seats, f.Seats - f.Sold, 1, f.FromCode, f.ToCode, f.Plane, f.Gate,
       CASE WHEN f.DayOff < 0 THEN 'Arrived' WHEN f.Code IN ('VJ327') THEN 'Delayed' ELSE 'Scheduled' END,
       CASE WHEN f.Air IN ('VJ','VU') THEN 7 ELSE 20 END,
       CASE WHEN f.Air IN ('VN','SQ','QH') THEN f.Price * 2.4 END
FROM @f f JOIN HangBay h ON h.MaIATA = f.Air
WHERE NOT EXISTS (SELECT 1 FROM ChuyenBay c WHERE c.MaChuyen = f.Code);

UPDATE ChuyenBay SET TrangThaiChuyen='Cancelled', GhiChu=N'Hủy do điều chỉnh khai thác (dữ liệu mẫu)', TrangThai=1 WHERE MaChuyen='BL6011' AND TrangThaiChuyen<>'Cancelled' AND ThoiGianDi < GETDATE();
GO

/* vé mẫu cho các khách hiện có (đánh dấu LaDuLieuMau=1) */
DECLARE @tix TABLE (Kh int, Code varchar(10), DayOff int, Seat varchar(4), Cls nvarchar(20), Status nvarchar(60), St varchar(20), Reason nvarchar(300));
INSERT @tix VALUES
 (1,'VN211',-12,'8C',N'Phổ thông',N'Đã bay','Completed',NULL),
 (1,'VN7131',1,'3A',N'Thương gia',N'Đã đặt','Booked',NULL),
 (2,'VJ131',-9,'21F',N'Phổ thông',N'Đã bay','Completed',NULL),
 (2,'VJ623',1,'14B',N'Phổ thông',N'Đã đặt','Booked',NULL),
 (3,'QH201',-6,'10D',N'Phổ thông',N'Đã bay','Completed',NULL),
 (3,'VN1553',2,'9E',N'Phổ thông',N'Đã đặt','Booked',NULL),
 (4,'VN160',-4,'5A',N'Thương gia',N'Đã bay','Completed',NULL),
 (4,'VJ511',-3,'33C',N'Phổ thông',N'Đã hủy','Cancelled',N'Khách đổi lịch trình'),
 (5,'VU701',-2,'7B',N'Phổ thông',N'Đã bay','Completed',NULL),
 (5,'QH1501',2,'12A',N'Phổ thông',N'Đã đặt','Booked',NULL),
 (6,'BL6011',-1,'16D',N'Phổ thông',N'Đã hủy','Cancelled',N'Hãng hủy chuyến'),
 (6,'VJ821',3,'20A',N'Phổ thông',N'Đã đặt','Booked',NULL),
 (1,'VN241',4,'2K',N'Thương gia',N'Đã đặt','Booked',NULL),
 (2,'VJ327',4,'25C',N'Phổ thông',N'Đã đặt','Booked',NULL),
 (3,'QH101',6,'11C',N'Phổ thông',N'Đã đặt','Booked',NULL),
 (5,'VN1363',7,'18F',N'Phổ thông',N'Đã đặt','Booked',NULL),
 (6,'SQ185',9,'4A',N'Thương gia',N'Đã đặt','Booked',NULL),
 (4,'VJ193',8,'30E',N'Phổ thông',N'Đã hủy','Cancelled',N'Khách hủy trước 72 giờ'),
 (1,'VU755',10,'6D',N'Phổ thông',N'Đã đặt','Booked',NULL),
 (2,'VN1601',20,'15B',N'Phổ thông',N'Đã đặt','Booked',NULL);

INSERT VeMayBay (MaKH, MaChuyenBay, HoTenHanhKhach, SoGiayTo, SoGhe, GiaVe, TrangThai, NgayDat, TrangThaiCode, MaVeCode, HangGhe, EmailLienHe, SdtLienHe, HanhLyKg, LyDoHuy, NgayCapNhat, LaDuLieuMau)
SELECT t.Kh, c.MaChuyenBay, kh.HoTen, '0792000' + RIGHT('00000' + CAST(ABS(CHECKSUM(t.Kh, t.Code, t.DayOff)) % 100000 AS varchar(10)), 5), t.Seat,
       CASE WHEN t.Cls = N'Thương gia' AND c.GiaThuongGia IS NOT NULL THEN c.GiaThuongGia ELSE c.GiaVe END,
       t.Status, DATEADD(day, CASE WHEN t.DayOff < 0 THEN t.DayOff - 5 ELSE -3 END, GETDATE()), t.St,
       'TH-S' + RIGHT('00000' + CAST(ABS(CHECKSUM(t.Kh, t.Code, t.DayOff)) % 100000 AS varchar(10)), 5),
       t.Cls, kh.Email, kh.SoDienThoai, CASE WHEN t.Cls = N'Thương gia' THEN 30 ELSE c.HanhLyKg END, t.Reason, GETDATE(), 1
FROM @tix t
JOIN ChuyenBay c ON c.MaChuyen = t.Code
JOIN KhachHang kh ON kh.MaKH = t.Kh
WHERE NOT EXISTS (SELECT 1 FROM VeMayBay v WHERE v.MaKH = t.Kh AND v.MaChuyenBay = c.MaChuyenBay);
GO
/* sửa tên thành phố bị mất dấu nếu migration từng chạy với literal không có tiền tố N */
UPDATE ChuyenBay SET SanBayDi = CASE MaSanBayDi WHEN 'SGN' THEN N'TP. Hồ Chí Minh' WHEN 'HAN' THEN N'Hà Nội' WHEN 'DAD' THEN N'Đà Nẵng' WHEN 'CXR' THEN N'Nha Trang' WHEN 'PQC' THEN N'Phú Quốc' WHEN 'DLI' THEN N'Đà Lạt' WHEN 'HUI' THEN N'Huế' ELSE SanBayDi END WHERE SanBayDi LIKE '%?%';
UPDATE ChuyenBay SET SanBayDen = CASE MaSanBayDen WHEN 'SGN' THEN N'TP. Hồ Chí Minh' WHEN 'HAN' THEN N'Hà Nội' WHEN 'DAD' THEN N'Đà Nẵng' WHEN 'CXR' THEN N'Nha Trang' WHEN 'PQC' THEN N'Phú Quốc' WHEN 'DLI' THEN N'Đà Lạt' WHEN 'HUI' THEN N'Huế' ELSE SanBayDen END WHERE SanBayDen LIKE '%?%';
GO
PRINT 'OK 006';
