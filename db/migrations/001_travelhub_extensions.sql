/* =====================================================================
   001_travelhub_extensions.sql
   Migration CỘNG THÊM (additive) cho TravelHubDB. Không DROP / không xóa dữ liệu.
   Idempotent: có thể chạy lại nhiều lần.
   Backup trước khi chạy: db/README (BACKUP DATABASE TravelHubDB ...).
   ===================================================================== */
SET XACT_ABORT ON;
SET NOCOUNT ON;
BEGIN TRAN;

/* ---- TaiKhoan: xác thực email, reset mật khẩu, khóa đăng nhập ---- */
IF COL_LENGTH('dbo.TaiKhoan','EmailXacThuc') IS NULL
BEGIN
    ALTER TABLE dbo.TaiKhoan ADD EmailXacThuc bit NOT NULL CONSTRAINT DF_TaiKhoan_EmailXacThuc DEFAULT 0;
    -- Tài khoản đã tồn tại trước migration được coi là đã xác thực để không bị khóa.
    EXEC('UPDATE dbo.TaiKhoan SET EmailXacThuc = 1');
END
IF COL_LENGTH('dbo.TaiKhoan','TokenXacThuc') IS NULL ALTER TABLE dbo.TaiKhoan ADD TokenXacThuc varchar(100) NULL;
IF COL_LENGTH('dbo.TaiKhoan','TokenXacThucHetHan') IS NULL ALTER TABLE dbo.TaiKhoan ADD TokenXacThucHetHan datetime NULL;
IF COL_LENGTH('dbo.TaiKhoan','TokenDatLaiMK') IS NULL ALTER TABLE dbo.TaiKhoan ADD TokenDatLaiMK varchar(100) NULL;
IF COL_LENGTH('dbo.TaiKhoan','TokenDatLaiMKHetHan') IS NULL ALTER TABLE dbo.TaiKhoan ADD TokenDatLaiMKHetHan datetime NULL;
IF COL_LENGTH('dbo.TaiKhoan','SoLanDangNhapSai') IS NULL ALTER TABLE dbo.TaiKhoan ADD SoLanDangNhapSai int NOT NULL CONSTRAINT DF_TaiKhoan_SaiMK DEFAULT 0;
IF COL_LENGTH('dbo.TaiKhoan','KhoaDen') IS NULL ALTER TABLE dbo.TaiKhoan ADD KhoaDen datetime NULL;

/* ---- Vai trò bổ sung ---- */
IF NOT EXISTS (SELECT 1 FROM dbo.VaiTro WHERE MaVaiTroCode='Staff')
    INSERT dbo.VaiTro (TenVaiTro, MoTa, TrangThai, MaVaiTroCode) VALUES (N'Nhân viên vận hành', N'Xử lý đặt phòng, hỗ trợ khách.', 1, 'Staff');
IF NOT EXISTS (SELECT 1 FROM dbo.VaiTro WHERE MaVaiTroCode='ContentManager')
    INSERT dbo.VaiTro (TenVaiTro, MoTa, TrangThai, MaVaiTroCode) VALUES (N'Quản lý nội dung', N'Quản lý bài viết, điểm đến, banner.', 1, 'ContentManager');

/* ---- ThanhToan: tham chiếu cổng, idempotency ---- */
IF COL_LENGTH('dbo.ThanhToan','MaThamChieuVNPAY') IS NULL ALTER TABLE dbo.ThanhToan ADD MaThamChieuVNPAY varchar(100) NULL;
IF COL_LENGTH('dbo.ThanhToan','Cong') IS NULL ALTER TABLE dbo.ThanhToan ADD Cong varchar(20) NULL;           -- VNPAY | MOMO | MANUAL
IF COL_LENGTH('dbo.ThanhToan','TienTe') IS NULL ALTER TABLE dbo.ThanhToan ADD TienTe char(3) NOT NULL CONSTRAINT DF_ThanhToan_TienTe DEFAULT 'VND';
IF COL_LENGTH('dbo.ThanhToan','NgayTao') IS NULL ALTER TABLE dbo.ThanhToan ADD NgayTao datetime NOT NULL CONSTRAINT DF_ThanhToan_NgayTao DEFAULT GETDATE();
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_ThanhToan_MaThamChieuVNPAY')
    EXEC('CREATE UNIQUE INDEX UX_ThanhToan_MaThamChieuVNPAY ON dbo.ThanhToan(MaThamChieuVNPAY) WHERE MaThamChieuVNPAY IS NOT NULL');

/* ---- DonDatPhong: thông tin khách lưu trú, snapshot giá/chính sách, concurrency ---- */
IF COL_LENGTH('dbo.DonDatPhong','HoTenKhach') IS NULL ALTER TABLE dbo.DonDatPhong ADD HoTenKhach nvarchar(100) NULL;
IF COL_LENGTH('dbo.DonDatPhong','EmailKhach') IS NULL ALTER TABLE dbo.DonDatPhong ADD EmailKhach varchar(100) NULL;
IF COL_LENGTH('dbo.DonDatPhong','SdtKhach') IS NULL ALTER TABLE dbo.DonDatPhong ADD SdtKhach varchar(20) NULL;
IF COL_LENGTH('dbo.DonDatPhong','SnapshotGia') IS NULL ALTER TABLE dbo.DonDatPhong ADD SnapshotGia nvarchar(max) NULL;       -- JSON: giá/đêm, thuế, phụ phí tại thời điểm đặt
IF COL_LENGTH('dbo.DonDatPhong','ChinhSachHuy') IS NULL ALTER TABLE dbo.DonDatPhong ADD ChinhSachHuy nvarchar(500) NULL;      -- snapshot chính sách hủy
IF COL_LENGTH('dbo.DonDatPhong','HuyMienPhiDen') IS NULL ALTER TABLE dbo.DonDatPhong ADD HuyMienPhiDen datetime NULL;
IF COL_LENGTH('dbo.DonDatPhong','GhiChuNoiBo') IS NULL ALTER TABLE dbo.DonDatPhong ADD GhiChuNoiBo nvarchar(1000) NULL;
IF COL_LENGTH('dbo.DonDatPhong','RowVer') IS NULL ALTER TABLE dbo.DonDatPhong ADD RowVer rowversion;
IF COL_LENGTH('dbo.DonDatPhong','NgayCapNhat') IS NULL ALTER TABLE dbo.DonDatPhong ADD NgayCapNhat datetime NULL;

/* ---- Phong: ảnh theo phòng, ngừng bán/bảo trì ---- */
IF OBJECT_ID('dbo.HinhAnhPhong') IS NULL
CREATE TABLE dbo.HinhAnhPhong (
    MaHinhAnhPhong int IDENTITY(1,1) PRIMARY KEY,
    MaPhong int NOT NULL CONSTRAINT FK_HinhAnhPhong_Phong REFERENCES dbo.Phong(MaPhong),
    DuongDanAnh varchar(255) NOT NULL,
    MoTa nvarchar(255) NULL,
    Loai nvarchar(30) NULL,                 -- phòng ngủ | phòng tắm | view | tiện nghi
    ThuTu int NOT NULL DEFAULT 0,
    LaAnhDaiDien bit NOT NULL DEFAULT 0,
    HienThi bit NOT NULL DEFAULT 1
);
IF OBJECT_ID('dbo.PhongNgungBan') IS NULL
CREATE TABLE dbo.PhongNgungBan (
    MaNgungBan int IDENTITY(1,1) PRIMARY KEY,
    MaPhong int NOT NULL CONSTRAINT FK_PhongNgungBan_Phong REFERENCES dbo.Phong(MaPhong),
    TuNgay date NOT NULL,
    DenNgay date NOT NULL,                  -- khoảng [TuNgay, DenNgay)
    SoLuong int NOT NULL DEFAULT 1,         -- số phòng không bán trong khoảng
    LyDo nvarchar(255) NULL,
    CONSTRAINT CK_PhongNgungBan_Ngay CHECK (DenNgay > TuNgay)
);
IF OBJECT_ID('dbo.GiaPhongTheoNgay') IS NULL
CREATE TABLE dbo.GiaPhongTheoNgay (
    MaPhong int NOT NULL CONSTRAINT FK_GiaPhongTheoNgay_Phong REFERENCES dbo.Phong(MaPhong),
    Ngay date NOT NULL,
    Gia decimal(18,2) NOT NULL CHECK (Gia >= 0),
    PRIMARY KEY (MaPhong, Ngay)
);
IF OBJECT_ID('dbo.HinhAnhPhong') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_HinhAnhPhong_MaPhong')
    CREATE INDEX IX_HinhAnhPhong_MaPhong ON dbo.HinhAnhPhong(MaPhong, ThuTu);

/* ---- KhachSan: slug, chính sách chi tiết ---- */
IF COL_LENGTH('dbo.KhachSan','ChinhSachHuyLoai') IS NULL ALTER TABLE dbo.KhachSan ADD ChinhSachHuyLoai varchar(20) NOT NULL CONSTRAINT DF_KhachSan_HuyLoai DEFAULT 'FLEX1'; -- FREE24 | FLEX1 | NONREFUND
IF COL_LENGTH('dbo.KhachSan','GioNhanPhong') IS NULL ALTER TABLE dbo.KhachSan ADD GioNhanPhong varchar(5) NOT NULL CONSTRAINT DF_KhachSan_GioNhan DEFAULT '14:00';
IF COL_LENGTH('dbo.KhachSan','GioTraPhong') IS NULL ALTER TABLE dbo.KhachSan ADD GioTraPhong varchar(5) NOT NULL CONSTRAINT DF_KhachSan_GioTra DEFAULT '12:00';
IF COL_LENGTH('dbo.KhachSan','BuaSang') IS NULL ALTER TABLE dbo.KhachSan ADD BuaSang bit NOT NULL CONSTRAINT DF_KhachSan_BuaSang DEFAULT 0;

/* ---- Lịch sử trạng thái đơn ---- */
IF OBJECT_ID('dbo.LichSuTrangThaiDon') IS NULL
CREATE TABLE dbo.LichSuTrangThaiDon (
    MaLichSu bigint IDENTITY(1,1) PRIMARY KEY,
    MaDon int NOT NULL CONSTRAINT FK_LichSuDon_Don REFERENCES dbo.DonDatPhong(MaDon),
    TuTrangThai nvarchar(30) NULL,
    DenTrangThai nvarchar(30) NOT NULL,
    MaTaiKhoan int NULL,
    LyDo nvarchar(500) NULL,
    NgayTao datetime2 NOT NULL DEFAULT SYSUTCDATETIME()
);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LichSuDon_MaDon') CREATE INDEX IX_LichSuDon_MaDon ON dbo.LichSuTrangThaiDon(MaDon);

/* ---- Nhật ký callback thanh toán (idempotency + đối soát) ---- */
IF OBJECT_ID('dbo.NhatKyThanhToan') IS NULL
CREATE TABLE dbo.NhatKyThanhToan (
    MaNhatKy bigint IDENTITY(1,1) PRIMARY KEY,
    MaThanhToan int NULL,
    Cong varchar(20) NOT NULL,
    Nguon varchar(20) NOT NULL,              -- IPN | RETURN | RECONCILE
    ThamChieu varchar(100) NULL,
    ChuKyHopLe bit NOT NULL,
    MaKetQua varchar(20) NULL,
    DuLieu nvarchar(2000) NULL,              -- đã loại bỏ chữ ký/secret
    NgayTao datetime2 NOT NULL DEFAULT SYSUTCDATETIME()
);

/* ---- Hoàn tiền ---- */
IF OBJECT_ID('dbo.YeuCauHoanTien') IS NULL
CREATE TABLE dbo.YeuCauHoanTien (
    MaYeuCau int IDENTITY(1,1) PRIMARY KEY,
    MaDon int NOT NULL CONSTRAINT FK_HoanTien_Don REFERENCES dbo.DonDatPhong(MaDon),
    MaThanhToan int NULL CONSTRAINT FK_HoanTien_ThanhToan REFERENCES dbo.ThanhToan(MaThanhToan),
    SoTien decimal(18,2) NOT NULL CHECK (SoTien >= 0),
    LyDo nvarchar(500) NULL,
    TrangThai varchar(20) NOT NULL DEFAULT 'PENDING',   -- PENDING | APPROVED | REJECTED | REFUNDED | MANUAL_REQUIRED
    MaGiaoDichHoan varchar(100) NULL,
    NguoiXuLy int NULL,
    LyDoTuChoi nvarchar(500) NULL,
    NgayTao datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
    NgayXuLy datetime2 NULL
);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_HoanTien_MotYeuCauMoiDon')
    EXEC('CREATE UNIQUE INDEX UX_HoanTien_MotYeuCauMoiDon ON dbo.YeuCauHoanTien(MaDon) WHERE TrangThai IN (''PENDING'',''APPROVED'',''REFUNDED'',''MANUAL_REQUIRED'')');

/* ---- Email outbox ---- */
IF OBJECT_ID('dbo.EmailOutbox') IS NULL
CREATE TABLE dbo.EmailOutbox (
    MaEmail bigint IDENTITY(1,1) PRIMARY KEY,
    Loai varchar(40) NOT NULL,
    KhoaTrung varchar(120) NULL,             -- chống gửi trùng (vd BOOKING_CONFIRMED:123)
    Den varchar(150) NOT NULL,
    TieuDe nvarchar(250) NOT NULL,
    NoiDungHtml nvarchar(max) NOT NULL,
    TepDinhKem nvarchar(260) NULL,           -- mã biên nhận, sinh PDF khi gửi
    TrangThai varchar(12) NOT NULL DEFAULT 'PENDING',   -- PENDING | SENT | FAILED
    SoLanThu int NOT NULL DEFAULT 0,
    LoiCuoi nvarchar(500) NULL,
    NgayTao datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
    NgayGui datetime2 NULL
);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_EmailOutbox_KhoaTrung')
    EXEC('CREATE UNIQUE INDEX UX_EmailOutbox_KhoaTrung ON dbo.EmailOutbox(KhoaTrung) WHERE KhoaTrung IS NOT NULL');

/* ---- Biên nhận (snapshot bất biến) ---- */
IF OBJECT_ID('dbo.BienNhan') IS NULL
CREATE TABLE dbo.BienNhan (
    MaBienNhan int IDENTITY(1,1) PRIMARY KEY,
    SoBienNhan varchar(30) NOT NULL UNIQUE,
    MaDon int NOT NULL CONSTRAINT FK_BienNhan_Don REFERENCES dbo.DonDatPhong(MaDon),
    MaThanhToan int NULL CONSTRAINT FK_BienNhan_ThanhToan REFERENCES dbo.ThanhToan(MaThanhToan),
    DuLieuSnapshot nvarchar(max) NOT NULL,   -- JSON
    NgayTao datetime2 NOT NULL DEFAULT SYSUTCDATETIME()
);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_BienNhan_ThanhToan')
    EXEC('CREATE UNIQUE INDEX UX_BienNhan_ThanhToan ON dbo.BienNhan(MaThanhToan) WHERE MaThanhToan IS NOT NULL');

/* ---- Đánh giá: kiểm duyệt, chống trùng ---- */
IF COL_LENGTH('dbo.DanhGia','LyDoAn') IS NULL ALTER TABLE dbo.DanhGia ADD LyDoAn nvarchar(500) NULL;
IF COL_LENGTH('dbo.DanhGia','LaDuLieuMau') IS NULL ALTER TABLE dbo.DanhGia ADD LaDuLieuMau bit NOT NULL CONSTRAINT DF_DanhGia_Mau DEFAULT 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_DanhGia_MotDanhGiaMoiDon')
    EXEC('CREATE UNIQUE INDEX UX_DanhGia_MotDanhGiaMoiDon ON dbo.DanhGia(MaDon) WHERE MaDon IS NOT NULL');

/* ---- Bài viết: danh mục + SEO ---- */
IF COL_LENGTH('dbo.BaiViet','DanhMuc') IS NULL ALTER TABLE dbo.BaiViet ADD DanhMuc nvarchar(100) NULL;
IF COL_LENGTH('dbo.BaiViet','SeoTieuDe') IS NULL ALTER TABLE dbo.BaiViet ADD SeoTieuDe nvarchar(120) NULL;
IF COL_LENGTH('dbo.BaiViet','SeoMoTa') IS NULL ALTER TABLE dbo.BaiViet ADD SeoMoTa nvarchar(300) NULL;

/* ---- Liên hệ / hỗ trợ, bản tin, cấu hình ---- */
IF OBJECT_ID('dbo.LienHe') IS NULL
CREATE TABLE dbo.LienHe (
    MaLienHe int IDENTITY(1,1) PRIMARY KEY,
    HoTen nvarchar(100) NOT NULL, Email varchar(100) NOT NULL,
    TieuDe nvarchar(200) NOT NULL, NoiDung nvarchar(2000) NOT NULL,
    MaDon int NULL, TrangThai varchar(12) NOT NULL DEFAULT 'NEW',
    NgayTao datetime2 NOT NULL DEFAULT SYSUTCDATETIME()
);
IF OBJECT_ID('dbo.DangKyNhanTin') IS NULL
CREATE TABLE dbo.DangKyNhanTin (
    Email varchar(100) NOT NULL PRIMARY KEY, NgayTao datetime2 NOT NULL DEFAULT SYSUTCDATETIME()
);
IF OBJECT_ID('dbo.CauHinhHeThong') IS NULL
CREATE TABLE dbo.CauHinhHeThong (
    Khoa varchar(60) NOT NULL PRIMARY KEY, GiaTri nvarchar(1000) NOT NULL, MoTa nvarchar(250) NULL
);
MERGE dbo.CauHinhHeThong AS t USING (VALUES
    ('GiuPhongPhut','15',N'Thời gian giữ phòng chờ thanh toán (phút)'),
    ('ThueVAT','0.08',N'Thuế VAT áp dụng trên tiền phòng'),
    ('PhuPhiDichVu','0',N'Phụ phí dịch vụ (tỉ lệ)'),
    ('EmailHoTro',N'support@travelhub.vn',N'Email hỗ trợ'),
    ('EmailQuanTri',N'',N'Email nhận thông báo booking mới'),
    ('BatVNPAY','1',N'Bật phương thức VNPay'),
    ('BatMoMo','0',N'Bật phương thức MoMo (cần credential)')
) AS s(Khoa,GiaTri,MoTa) ON t.Khoa=s.Khoa
WHEN NOT MATCHED THEN INSERT (Khoa,GiaTri,MoTa) VALUES (s.Khoa,s.GiaTri,s.MoTa);

/* ---- Index tìm kiếm ---- */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_ChiTietDatPhong_MaPhong') CREATE INDEX IX_ChiTietDatPhong_MaPhong ON dbo.ChiTietDatPhong(MaPhong) INCLUDE (MaDon, SoLuong);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_KhachSan_MaDiaDiem') CREATE INDEX IX_KhachSan_MaDiaDiem ON dbo.KhachSan(MaDiaDiem, TrangThai);

COMMIT;
PRINT N'001 OK';
