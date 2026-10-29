USE [master]
GO

CREATE DATABASE [TravelHubDB]
GO

USE [TravelHubDB]
GO

/****** Object:  Table [dbo].[BaiViet]    Script Date: 05/10/2026 11:17:37 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BaiViet](
	[MaBaiViet] [int] IDENTITY(1,1) NOT NULL,
	[MaDiaDiem] [int] NULL,
	[TieuDe] [nvarchar](200) NOT NULL,
	[MoTaNgan] [nvarchar](500) NULL,
	[NoiDung] [nvarchar](max) NULL,
	[HinhAnh] [varchar](255) NULL,
	[TacGia] [nvarchar](100) NULL,
	[NgayDang] [datetime] NOT NULL,
	[LuotXem] [int] NOT NULL,
	[NoiBat] [bit] NOT NULL,
	[TrangThai] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[MaBaiViet] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ChiTietDatPhong]    Script Date: 05/10/2026 11:17:38 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ChiTietDatPhong](
	[MaChiTiet] [int] IDENTITY(1,1) NOT NULL,
	[MaDon] [int] NOT NULL,
	[MaPhong] [int] NOT NULL,
	[SoLuong] [int] NOT NULL,
	[DonGia] [decimal](18, 2) NOT NULL,
	[SoDem] [int] NULL,
	[ThanhTien]  AS ([SoLuong]*[DonGia]),
PRIMARY KEY CLUSTERED 
(
	[MaChiTiet] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ChuyenBay]    Script Date: 05/10/2026 11:17:38 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ChuyenBay](
	[MaChuyenBay] [int] IDENTITY(1,1) NOT NULL,
	[MaHangBay] [int] NOT NULL,
	[MaChuyen] [varchar](20) NOT NULL,
	[SanBayDi] [nvarchar](100) NOT NULL,
	[SanBayDen] [nvarchar](100) NOT NULL,
	[ThoiGianDi] [datetime] NOT NULL,
	[ThoiGianDen] [datetime] NOT NULL,
	[GiaVe] [decimal](18, 2) NOT NULL,
	[SoGhe] [int] NOT NULL,
	[SoGheConLai] [int] NOT NULL,
	[TrangThai] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[MaChuyenBay] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DanhGia]    Script Date: 05/10/2026 11:17:38 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DanhGia](
	[MaDanhGia] [int] IDENTITY(1,1) NOT NULL,
	[MaKH] [int] NOT NULL,
	[MaKS] [int] NOT NULL,
	[SoSao] [int] NOT NULL,
	[NoiDung] [nvarchar](1000) NULL,
	[NgayDanhGia] [datetime] NOT NULL,
	[TrangThai] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[MaDanhGia] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DiaDiem]    Script Date: 05/10/2026 11:17:38 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DiaDiem](
	[MaDiaDiem] [int] IDENTITY(1,1) NOT NULL,
	[TenDiaDiem] [nvarchar](100) NOT NULL,
	[ThanhPho] [nvarchar](100) NULL,
	[QuocGia] [nvarchar](100) NOT NULL,
	[MoTa] [nvarchar](500) NULL,
	[HinhAnh] [varchar](255) NULL,
	[PhongCach] [nvarchar](100) NULL,
	[TrangThai] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[MaDiaDiem] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DiaDiemDuLich]    Script Date: 05/10/2026 11:17:38 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DiaDiemDuLich](
	[MaDDDL] [int] IDENTITY(1,1) NOT NULL,
	[MaDiaDiem] [int] NOT NULL,
	[TenDiaDiem] [nvarchar](150) NOT NULL,
	[LoaiDiaDiem] [nvarchar](50) NULL,
	[DiaChi] [nvarchar](255) NULL,
	[MoTa] [nvarchar](max) NULL,
	[HinhAnh] [varchar](255) NULL,
	[GiaVe] [decimal](18, 2) NULL,
	[GioMoCua] [nvarchar](100) NULL,
	[NoiBat] [bit] NOT NULL,
	[TrangThai] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[MaDDDL] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DonDatPhong]    Script Date: 05/10/2026 11:17:38 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DonDatPhong](
	[MaDon] [int] IDENTITY(1,1) NOT NULL,
	[MaDonCode] [varchar](50) NULL,
	[MaKH] [int] NOT NULL,
	[MaKS] [int] NOT NULL,
	[MaKM] [int] NULL,
	[NgayNhanPhong] [date] NOT NULL,
	[NgayTraPhong] [date] NOT NULL,
	[SoNguoiLon] [int] NOT NULL,
	[SoTreEm] [int] NOT NULL,
	[TongTien] [decimal](18, 2) NOT NULL,
	[GiamGia] [decimal](18, 2) NOT NULL,
	[ThanhTien] [decimal](18, 2) NOT NULL,
	[TrangThai] [nvarchar](30) NOT NULL,
	[TrangThaiCode] [varchar](30) NULL,
	[PhuongThucThanhToan] [nvarchar](50) NULL,
	[GhiChu] [nvarchar](500) NULL,
	[NgayHetHan] [datetime] NULL,
	[NgayDat] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[MaDon] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HangBay]    Script Date: 05/10/2026 11:17:38 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HangBay](
	[MaHangBay] [int] IDENTITY(1,1) NOT NULL,
	[TenHangBay] [nvarchar](100) NOT NULL,
	[MaIATA] [varchar](10) NULL,
	[Logo] [varchar](255) NULL,
	[TrangThai] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[MaHangBay] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HinhAnhKhachSan]    Script Date: 05/10/2026 11:17:38 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HinhAnhKhachSan](
	[MaHinhAnh] [int] IDENTITY(1,1) NOT NULL,
	[MaKS] [int] NOT NULL,
	[DuongDanAnh] [varchar](255) NOT NULL,
	[MoTa] [nvarchar](255) NULL,
	[LaAnhDaiDien] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[MaHinhAnh] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[KhachHang]    Script Date: 05/10/2026 11:17:38 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[KhachHang](
	[MaKH] [int] IDENTITY(1,1) NOT NULL,
	[HoTen] [nvarchar](100) NOT NULL,
	[Email] [varchar](100) NOT NULL,
	[SoDienThoai] [varchar](20) NULL,
	[NgaySinh] [date] NULL,
	[GioiTinh] [nvarchar](10) NULL,
	[HinhAnh] [varchar](255) NULL,
	[TrangThai] [bit] NOT NULL,
	[NgayTao] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[MaKH] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[KhachSan]    Script Date: 05/10/2026 11:17:38 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[KhachSan](
	[MaKS] [int] IDENTITY(1,1) NOT NULL,
	[TenKS] [nvarchar](150) NOT NULL,
	[DiaChi] [nvarchar](255) NOT NULL,
	[MaDiaDiem] [int] NOT NULL,
	[MaTaiKhoanDoiTac] [int] NULL,
	[MoTa] [nvarchar](max) NULL,
	[SoSao] [int] NOT NULL,
	[GiaTu] [decimal](18, 2) NULL,
	[HinhAnhDaiDien] [varchar](255) NULL,
	[LoaiHinh] [nvarchar](100) NULL,
	[SoDienThoai] [varchar](20) NULL,
	[Email] [varchar](100) NULL,
	[ChinhSach] [nvarchar](1000) NULL,
	[TrangThai] [bit] NOT NULL,
	[NgayTao] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[MaKS] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[KhachSan_TienIch]    Script Date: 05/10/2026 11:17:38 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[KhachSan_TienIch](
	[MaKS] [int] NOT NULL,
	[MaTienIch] [int] NOT NULL,
 CONSTRAINT [PK_KhachSan_TienIch] PRIMARY KEY CLUSTERED 
(
	[MaKS] ASC,
	[MaTienIch] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[KhuyenMai]    Script Date: 05/10/2026 11:17:38 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[KhuyenMai](
	[MaKM] [int] IDENTITY(1,1) NOT NULL,
	[TenKM] [nvarchar](150) NOT NULL,
	[MaCode] [varchar](50) NULL,
	[PhanTramGiam] [decimal](5, 2) NULL,
	[SoTienGiam] [decimal](18, 2) NULL,
	[GiaTriToiThieu] [decimal](18, 2) NULL,
	[NgayBatDau] [datetime] NULL,
	[NgayKetThuc] [datetime] NULL,
	[SoLuong] [int] NULL,
	[MoTa] [nvarchar](500) NULL,
	[TrangThai] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[MaKM] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Phong]    Script Date: 05/10/2026 11:17:38 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Phong](
	[MaPhong] [int] IDENTITY(1,1) NOT NULL,
	[MaKS] [int] NOT NULL,
	[TenPhong] [nvarchar](100) NOT NULL,
	[LoaiPhong] [nvarchar](100) NULL,
	[SoNguoiToiDa] [int] NOT NULL,
	[SoGiuong] [int] NULL,
	[DienTich] [decimal](10, 2) NULL,
	[GiaPhong] [decimal](18, 2) NOT NULL,
	[SoLuong] [int] NOT NULL,
	[MoTa] [nvarchar](500) NULL,
	[HinhAnh] [varchar](255) NULL,
	[TrangThai] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[MaPhong] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Quyen]    Script Date: 05/10/2026 11:17:38 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Quyen](
	[MaQuyen] [int] IDENTITY(1,1) NOT NULL,
	[TenQuyen] [nvarchar](100) NOT NULL,
	[MaQuyenCode] [varchar](100) NOT NULL,
	[MoTa] [nvarchar](255) NULL,
	[TrangThai] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[MaQuyen] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TaiKhoan]    Script Date: 05/10/2026 11:17:38 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TaiKhoan](
	[MaTaiKhoan] [int] IDENTITY(1,1) NOT NULL,
	[TenDangNhap] [varchar](100) NOT NULL,
	[Email] [varchar](100) NOT NULL,
	[MatKhau] [varchar](255) NOT NULL,
	[MaVaiTro] [int] NOT NULL,
	[MaKH] [int] NULL,
	[TrangThai] [bit] NOT NULL,
	[NgayTao] [datetime] NOT NULL,
	[LanDangNhapCuoi] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[MaTaiKhoan] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ThanhToan]    Script Date: 05/10/2026 11:17:38 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ThanhToan](
	[MaThanhToan] [int] IDENTITY(1,1) NOT NULL,
	[MaDon] [int] NOT NULL,
	[SoTien] [decimal](18, 2) NOT NULL,
	[PhuongThuc] [nvarchar](50) NULL,
	[MaGiaoDich] [varchar](100) NULL,
	[TrangThai] [nvarchar](30) NOT NULL,
	[TrangThaiCode] [varchar](30) NULL,
	[LoaiGiaoDich] [varchar](30) NULL,
	[NgayThanhToan] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[MaThanhToan] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TienIch]    Script Date: 05/10/2026 11:17:38 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TienIch](
	[MaTienIch] [int] IDENTITY(1,1) NOT NULL,
	[TenTienIch] [nvarchar](100) NOT NULL,
	[Icon] [varchar](100) NULL,
	[TrangThai] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[MaTienIch] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VaiTro]    Script Date: 05/10/2026 11:17:38 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VaiTro](
	[MaVaiTro] [int] IDENTITY(1,1) NOT NULL,
	[TenVaiTro] [nvarchar](50) NOT NULL,
	[MoTa] [nvarchar](255) NULL,
	[TrangThai] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[MaVaiTro] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VaiTro_Quyen]    Script Date: 05/10/2026 11:17:38 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VaiTro_Quyen](
	[MaVaiTro] [int] NOT NULL,
	[MaQuyen] [int] NOT NULL,
 CONSTRAINT [PK_VaiTro_Quyen] PRIMARY KEY CLUSTERED 
(
	[MaVaiTro] ASC,
	[MaQuyen] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VeMayBay]    Script Date: 05/10/2026 11:17:38 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VeMayBay](
	[MaVe] [int] IDENTITY(1,1) NOT NULL,
	[MaKH] [int] NOT NULL,
	[MaChuyenBay] [int] NOT NULL,
	[HoTenHanhKhach] [nvarchar](100) NOT NULL,
	[SoGiayTo] [varchar](30) NULL,
	[SoGhe] [varchar](10) NULL,
	[GiaVe] [decimal](18, 2) NULL,
	[TrangThai] [nvarchar](30) NOT NULL,
	[NgayDat] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[MaVe] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[YeuThich]    Script Date: 05/10/2026 11:17:38 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[YeuThich](
	[MaKH] [int] NOT NULL,
	[MaKS] [int] NOT NULL,
	[NgayThem] [datetime] NOT NULL,
 CONSTRAINT [PK_YeuThich] PRIMARY KEY CLUSTERED 
(
	[MaKH] ASC,
	[MaKS] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[BaiViet] ON 

INSERT [dbo].[BaiViet] ([MaBaiViet], [MaDiaDiem], [TieuDe], [MoTaNgan], [NoiDung], [HinhAnh], [TacGia], [NgayDang], [LuotXem], [NoiBat], [TrangThai]) VALUES (1, 1, N'10 địa điểm không thể bỏ qua khi đến Đà Nẵng', N'Khám phá những địa điểm nổi tiếng nhất tại Đà Nẵng.', N'Đà Nẵng là một trong những điểm đến hấp dẫn nhất Việt Nam với biển Mỹ Khê, Bà Nà Hills, Cầu Rồng và nhiều địa điểm thú vị khác.', N'/Content/images/articles/danang.jpg', N'TravelHub', CAST(N'2026-10-01T22:02:50.527' AS DateTime), 1250, 1, 1)
INSERT [dbo].[BaiViet] ([MaBaiViet], [MaDiaDiem], [TieuDe], [MoTaNgan], [NoiDung], [HinhAnh], [TacGia], [NgayDang], [LuotXem], [NoiBat], [TrangThai]) VALUES (2, 2, N'Kinh nghiệm du lịch Đà Lạt tự túc', N'Những kinh nghiệm hữu ích cho chuyến đi Đà Lạt.', N'Đà Lạt có khí hậu mát mẻ, cảnh quan thơ mộng và rất nhiều địa điểm check-in.', N'/Content/images/articles/dalat.jpg', N'TravelHub', CAST(N'2026-10-01T22:02:50.527' AS DateTime), 980, 1, 1)
INSERT [dbo].[BaiViet] ([MaBaiViet], [MaDiaDiem], [TieuDe], [MoTaNgan], [NoiDung], [HinhAnh], [TacGia], [NgayDang], [LuotXem], [NoiBat], [TrangThai]) VALUES (3, 3, N'Khám phá biển Nha Trang', N'Những trải nghiệm đáng nhớ tại Nha Trang.', N'Nha Trang nổi tiếng với biển xanh, đảo đẹp và nhiều hoạt động vui chơi giải trí.', N'/Content/images/articles/nhatrang.jpg', N'TravelHub', CAST(N'2026-10-01T22:02:50.527' AS DateTime), 870, 0, 1)
INSERT [dbo].[BaiViet] ([MaBaiViet], [MaDiaDiem], [TieuDe], [MoTaNgan], [NoiDung], [HinhAnh], [TacGia], [NgayDang], [LuotXem], [NoiBat], [TrangThai]) VALUES (4, 4, N'Phú Quốc - thiên đường nghỉ dưỡng', N'Khám phá vẻ đẹp của đảo ngọc Phú Quốc.', N'Phú Quốc sở hữu nhiều bãi biển đẹp, resort cao cấp và các hoạt động trải nghiệm hấp dẫn.', N'/Content/images/articles/phuquoc.jpg', N'TravelHub', CAST(N'2026-10-01T22:02:50.527' AS DateTime), 1430, 1, 1)
INSERT [dbo].[BaiViet] ([MaBaiViet], [MaDiaDiem], [TieuDe], [MoTaNgan], [NoiDung], [HinhAnh], [TacGia], [NgayDang], [LuotXem], [NoiBat], [TrangThai]) VALUES (5, 5, N'Cuối tuần đi Vũng Tàu', N'Lịch trình Vũng Tàu 2 ngày 1 đêm.', N'Vũng Tàu là điểm đến phù hợp cho những chuyến đi ngắn ngày từ TP.HCM.', N'/Content/images/articles/vungtau.jpg', N'TravelHub', CAST(N'2026-10-01T22:02:50.527' AS DateTime), 760, 0, 1)
INSERT [dbo].[BaiViet] ([MaBaiViet], [MaDiaDiem], [TieuDe], [MoTaNgan], [NoiDung], [HinhAnh], [TacGia], [NgayDang], [LuotXem], [NoiBat], [TrangThai]) VALUES (6, 1, N'Ăn gì khi đi Đà Nẵng?', N'Gợi ý các món ăn nổi tiếng tại Đà Nẵng.', N'Mì Quảng, bánh tráng cuốn thịt heo, bún chả cá và nhiều món ăn hấp dẫn đang chờ bạn khám phá.', N'/Content/images/articles/food-danang.jpg', N'TravelHub', CAST(N'2026-10-01T22:02:50.527' AS DateTime), 650, 0, 1)
INSERT [dbo].[BaiViet] ([MaBaiViet], [MaDiaDiem], [TieuDe], [MoTaNgan], [NoiDung], [HinhAnh], [TacGia], [NgayDang], [LuotXem], [NoiBat], [TrangThai]) VALUES (7, 6, N'Hà Nội: gợi ý lịch trình 2 ngày 1 đêm', N'Phố cổ, hồ Hoàn Kiếm, Văn Miếu và Hoàng thành Thăng Long trong một lịch trình gọn.', N'Hà Nội phù hợp với chuyến đi ngắn nhờ các điểm tham quan nằm gần nhau. Bạn có thể dành buổi sáng đi bộ quanh hồ Hoàn Kiếm và phố cổ, buổi chiều ghé Văn Miếu - Quốc Tử Giám hoặc Hoàng thành Thăng Long, buổi tối thưởng thức ẩm thực đường phố.', N'/Content/images/articles/hohoankiem.jpg', N'TravelHub', CAST(N'2026-10-04T17:12:33.387' AS DateTime), 0, 0, 1)
INSERT [dbo].[BaiViet] ([MaBaiViet], [MaDiaDiem], [TieuDe], [MoTaNgan], [NoiDung], [HinhAnh], [TacGia], [NgayDang], [LuotXem], [NoiBat], [TrangThai]) VALUES (8, 7, N'Một ngày dạo quanh trung tâm Sài Gòn', N'Từ Nhà thờ Đức Bà, Bưu điện Trung tâm đến chợ Bến Thành.', N'Khu vực Quận 1 tập trung nhiều điểm tham quan có thể đi bộ: Nhà thờ Đức Bà, Bưu điện Trung tâm Sài Gòn, Dinh Độc Lập, phố đi bộ Nguyễn Huệ và chợ Bến Thành. Buổi tối bạn có thể thử các quán ăn đường phố hoặc đi dạo bên bờ sông Sài Gòn.', N'/Content/images/articles/nhathoducba.jpg', N'TravelHub', CAST(N'2026-10-04T17:12:33.387' AS DateTime), 0, 0, 1)
INSERT [dbo].[BaiViet] ([MaBaiViet], [MaDiaDiem], [TieuDe], [MoTaNgan], [NoiDung], [HinhAnh], [TacGia], [NgayDang], [LuotXem], [NoiBat], [TrangThai]) VALUES (9, 8, N'Hội An: đi đâu, ăn gì trong phố cổ', N'Gợi ý cách tham quan phố cổ Hội An, Di sản văn hóa thế giới.', N'Phố cổ Hội An được UNESCO công nhận là Di sản văn hóa thế giới. Du khách thường kết hợp tham quan Chùa Cầu, các nhà cổ và hội quán, sau đó đi thuyền trên sông Thu Bồn vào buổi tối. Nên mua vé tham quan phố cổ trước khi vào các điểm có kiểm soát vé.', N'/Content/images/articles/phocohoian.jpg', N'TravelHub', CAST(N'2026-10-04T17:12:33.387' AS DateTime), 0, 0, 1)
INSERT [dbo].[BaiViet] ([MaBaiViet], [MaDiaDiem], [TieuDe], [MoTaNgan], [NoiDung], [HinhAnh], [TacGia], [NgayDang], [LuotXem], [NoiBat], [TrangThai]) VALUES (10, 9, N'Khám phá cố đô Huế qua các di tích triều Nguyễn', N'Đại Nội, chùa Thiên Mụ và các lăng tẩm vua Nguyễn.', N'Quần thể di tích cố đô Huế được UNESCO công nhận là Di sản văn hóa thế giới. Hành trình thường gồm Đại Nội, chùa Thiên Mụ bên sông Hương và các lăng Khải Định, Minh Mạng, Tự Đức. Nên sắp xếp một ngày riêng cho các lăng vì chúng nằm cách xa trung tâm.', N'/Content/images/articles/chuathienmu.jpg', N'TravelHub', CAST(N'2026-10-04T17:12:33.387' AS DateTime), 0, 0, 1)
INSERT [dbo].[BaiViet] ([MaBaiViet], [MaDiaDiem], [TieuDe], [MoTaNgan], [NoiDung], [HinhAnh], [TacGia], [NgayDang], [LuotXem], [NoiBat], [TrangThai]) VALUES (11, 10, N'Sa Pa: ruộng bậc thang, bản làng và Fansipan', N'Gợi ý những điểm đáng ghé khi lên Sa Pa.', N'Sa Pa nổi tiếng với ruộng bậc thang, các bản làng như Cát Cát và đỉnh Fansipan cao 3.143 m. Thời tiết vùng núi thay đổi nhanh nên bạn nên mang theo áo ấm và áo mưa nhẹ.', N'/Content/images/articles/bancatcat.jpg', N'TravelHub', CAST(N'2026-10-04T17:12:33.387' AS DateTime), 0, 0, 1)
INSERT [dbo].[BaiViet] ([MaBaiViet], [MaDiaDiem], [TieuDe], [MoTaNgan], [NoiDung], [HinhAnh], [TacGia], [NgayDang], [LuotXem], [NoiBat], [TrangThai]) VALUES (12, 11, N'Tham quan vịnh Hạ Long: nên chọn tuyến nào?', N'Những điều cần biết trước khi đặt tour tham quan vịnh.', N'Vịnh Hạ Long là Di sản thiên nhiên thế giới do UNESCO công nhận. Tour trong ngày thường ghé Hang Sửng Sốt và các đảo nhỏ; nếu muốn trải nghiệm kỹ hơn bạn có thể chọn du thuyền ngủ đêm. Hãy kiểm tra lịch trình và giá vé với đơn vị tổ chức trước khi đặt.', N'/Content/images/articles/hangsungsot.jpg', N'TravelHub', CAST(N'2026-10-04T17:12:33.387' AS DateTime), 0, 0, 1)
INSERT [dbo].[BaiViet] ([MaBaiViet], [MaDiaDiem], [TieuDe], [MoTaNgan], [NoiDung], [HinhAnh], [TacGia], [NgayDang], [LuotXem], [NoiBat], [TrangThai]) VALUES (13, 12, N'Ninh Bình: Tràng An, Tam Cốc và Hang Múa', N'Một chuyến đi ngắn ngày từ Hà Nội đến vùng non nước Ninh Bình.', N'Quần thể danh thắng Tràng An được UNESCO công nhận là Di sản thế giới. Bạn có thể đi thuyền ở Tràng An hoặc Tam Cốc, leo bậc thang ở Hang Múa để ngắm toàn cảnh và ghé chùa Bái Đình.', N'/Content/images/articles/hangmua.jpg', N'TravelHub', CAST(N'2026-10-04T17:12:33.387' AS DateTime), 0, 0, 1)
SET IDENTITY_INSERT [dbo].[BaiViet] OFF
GO
SET IDENTITY_INSERT [dbo].[ChiTietDatPhong] ON 

INSERT [dbo].[ChiTietDatPhong] ([MaChiTiet], [MaDon], [MaPhong], [SoLuong], [DonGia], [SoDem]) VALUES (1, 1, 1, 2, CAST(900000.00 AS Decimal(18, 2)), 2)
INSERT [dbo].[ChiTietDatPhong] ([MaChiTiet], [MaDon], [MaPhong], [SoLuong], [DonGia], [SoDem]) VALUES (2, 2, 4, 2, CAST(800000.00 AS Decimal(18, 2)), 2)
INSERT [dbo].[ChiTietDatPhong] ([MaChiTiet], [MaDon], [MaPhong], [SoLuong], [DonGia], [SoDem]) VALUES (3, 3, 1, 1, CAST(900000.00 AS Decimal(18, 2)), 2)
INSERT [dbo].[ChiTietDatPhong] ([MaChiTiet], [MaDon], [MaPhong], [SoLuong], [DonGia], [SoDem]) VALUES (4, 4, 1, 1, CAST(900000.00 AS Decimal(18, 2)), 2)
INSERT [dbo].[ChiTietDatPhong] ([MaChiTiet], [MaDon], [MaPhong], [SoLuong], [DonGia], [SoDem]) VALUES (5, 5, 1, 1, CAST(900000.00 AS Decimal(18, 2)), 3)
INSERT [dbo].[ChiTietDatPhong] ([MaChiTiet], [MaDon], [MaPhong], [SoLuong], [DonGia], [SoDem]) VALUES (6, 6, 1, 1, CAST(900000.00 AS Decimal(18, 2)), 2)
INSERT [dbo].[ChiTietDatPhong] ([MaChiTiet], [MaDon], [MaPhong], [SoLuong], [DonGia], [SoDem]) VALUES (7, 7, 1, 1, CAST(900000.00 AS Decimal(18, 2)), 2)
INSERT [dbo].[ChiTietDatPhong] ([MaChiTiet], [MaDon], [MaPhong], [SoLuong], [DonGia], [SoDem]) VALUES (8, 8, 1, 1, CAST(900000.00 AS Decimal(18, 2)), 2)
SET IDENTITY_INSERT [dbo].[ChiTietDatPhong] OFF
GO
SET IDENTITY_INSERT [dbo].[ChuyenBay] ON 

INSERT [dbo].[ChuyenBay] ([MaChuyenBay], [MaHangBay], [MaChuyen], [SanBayDi], [SanBayDen], [ThoiGianDi], [ThoiGianDen], [GiaVe], [SoGhe], [SoGheConLai], [TrangThai]) VALUES (1, 1, N'VN123', N'TP. Hồ Chí Minh', N'Đà Nẵng', CAST(N'2026-10-05T07:00:00.000' AS DateTime), CAST(N'2026-10-05T08:20:00.000' AS DateTime), CAST(1200000.00 AS Decimal(18, 2)), 180, 150, 1)
INSERT [dbo].[ChuyenBay] ([MaChuyenBay], [MaHangBay], [MaChuyen], [SanBayDi], [SanBayDen], [ThoiGianDi], [ThoiGianDen], [GiaVe], [SoGhe], [SoGheConLai], [TrangThai]) VALUES (2, 2, N'VJ621', N'TP. Hồ Chí Minh', N'Đà Nẵng', CAST(N'2026-10-05T09:00:00.000' AS DateTime), CAST(N'2026-10-05T10:20:00.000' AS DateTime), CAST(950000.00 AS Decimal(18, 2)), 180, 130, 1)
INSERT [dbo].[ChuyenBay] ([MaChuyenBay], [MaHangBay], [MaChuyen], [SanBayDi], [SanBayDen], [ThoiGianDi], [ThoiGianDen], [GiaVe], [SoGhe], [SoGheConLai], [TrangThai]) VALUES (3, 1, N'VN156', N'TP. Hồ Chí Minh', N'Nha Trang', CAST(N'2026-10-06T08:00:00.000' AS DateTime), CAST(N'2026-10-06T09:00:00.000' AS DateTime), CAST(1100000.00 AS Decimal(18, 2)), 180, 140, 1)
INSERT [dbo].[ChuyenBay] ([MaChuyenBay], [MaHangBay], [MaChuyen], [SanBayDi], [SanBayDen], [ThoiGianDi], [ThoiGianDen], [GiaVe], [SoGhe], [SoGheConLai], [TrangThai]) VALUES (4, 2, N'VJ325', N'TP. Hồ Chí Minh', N'Phú Quốc', CAST(N'2026-10-07T10:00:00.000' AS DateTime), CAST(N'2026-10-07T11:00:00.000' AS DateTime), CAST(850000.00 AS Decimal(18, 2)), 180, 120, 1)
INSERT [dbo].[ChuyenBay] ([MaChuyenBay], [MaHangBay], [MaChuyen], [SanBayDi], [SanBayDen], [ThoiGianDi], [ThoiGianDen], [GiaVe], [SoGhe], [SoGheConLai], [TrangThai]) VALUES (5, 1, N'VN210', N'Hà Nội', N'TP. Hồ Chí Minh', CAST(N'2026-10-08T06:00:00.000' AS DateTime), CAST(N'2026-10-08T08:10:00.000' AS DateTime), CAST(1850000.00 AS Decimal(18, 2)), 180, 160, 1)
INSERT [dbo].[ChuyenBay] ([MaChuyenBay], [MaHangBay], [MaChuyen], [SanBayDi], [SanBayDen], [ThoiGianDi], [ThoiGianDen], [GiaVe], [SoGhe], [SoGheConLai], [TrangThai]) VALUES (6, 1, N'VN241', N'TP. Hồ Chí Minh', N'Hà Nội', CAST(N'2026-10-08T14:00:00.000' AS DateTime), CAST(N'2026-10-08T16:10:00.000' AS DateTime), CAST(1900000.00 AS Decimal(18, 2)), 180, 140, 1)
INSERT [dbo].[ChuyenBay] ([MaChuyenBay], [MaHangBay], [MaChuyen], [SanBayDi], [SanBayDen], [ThoiGianDi], [ThoiGianDen], [GiaVe], [SoGhe], [SoGheConLai], [TrangThai]) VALUES (7, 1, N'VN160', N'Hà Nội', N'Đà Nẵng', CAST(N'2026-10-09T07:15:00.000' AS DateTime), CAST(N'2026-10-09T08:35:00.000' AS DateTime), CAST(1300000.00 AS Decimal(18, 2)), 180, 145, 1)
INSERT [dbo].[ChuyenBay] ([MaChuyenBay], [MaHangBay], [MaChuyen], [SanBayDi], [SanBayDen], [ThoiGianDi], [ThoiGianDen], [GiaVe], [SoGhe], [SoGheConLai], [TrangThai]) VALUES (8, 1, N'VN1356', N'TP. Hồ Chí Minh', N'Đà Lạt', CAST(N'2026-10-09T08:00:00.000' AS DateTime), CAST(N'2026-10-09T09:00:00.000' AS DateTime), CAST(1000000.00 AS Decimal(18, 2)), 180, 150, 1)
INSERT [dbo].[ChuyenBay] ([MaChuyenBay], [MaHangBay], [MaChuyen], [SanBayDi], [SanBayDen], [ThoiGianDi], [ThoiGianDen], [GiaVe], [SoGhe], [SoGheConLai], [TrangThai]) VALUES (9, 1, N'VN1822', N'TP. Hồ Chí Minh', N'Phú Quốc', CAST(N'2026-10-10T12:00:00.000' AS DateTime), CAST(N'2026-10-10T13:00:00.000' AS DateTime), CAST(1150000.00 AS Decimal(18, 2)), 180, 140, 1)
INSERT [dbo].[ChuyenBay] ([MaChuyenBay], [MaHangBay], [MaChuyen], [SanBayDi], [SanBayDen], [ThoiGianDi], [ThoiGianDen], [GiaVe], [SoGhe], [SoGheConLai], [TrangThai]) VALUES (10, 1, N'VN1370', N'TP. Hồ Chí Minh', N'Huế', CAST(N'2026-10-11T10:30:00.000' AS DateTime), CAST(N'2026-10-11T11:50:00.000' AS DateTime), CAST(1250000.00 AS Decimal(18, 2)), 180, 150, 1)
INSERT [dbo].[ChuyenBay] ([MaChuyenBay], [MaHangBay], [MaChuyen], [SanBayDi], [SanBayDen], [ThoiGianDi], [ThoiGianDen], [GiaVe], [SoGhe], [SoGheConLai], [TrangThai]) VALUES (11, 1, N'VN1347', N'Hà Nội', N'Nha Trang', CAST(N'2026-10-12T08:30:00.000' AS DateTime), CAST(N'2026-10-12T10:25:00.000' AS DateTime), CAST(1700000.00 AS Decimal(18, 2)), 180, 140, 1)
INSERT [dbo].[ChuyenBay] ([MaChuyenBay], [MaHangBay], [MaChuyen], [SanBayDi], [SanBayDen], [ThoiGianDi], [ThoiGianDen], [GiaVe], [SoGhe], [SoGheConLai], [TrangThai]) VALUES (12, 1, N'VN167', N'Đà Nẵng', N'Hà Nội', CAST(N'2026-10-12T19:00:00.000' AS DateTime), CAST(N'2026-10-12T20:20:00.000' AS DateTime), CAST(1250000.00 AS Decimal(18, 2)), 180, 145, 1)
INSERT [dbo].[ChuyenBay] ([MaChuyenBay], [MaHangBay], [MaChuyen], [SanBayDi], [SanBayDen], [ThoiGianDi], [ThoiGianDen], [GiaVe], [SoGhe], [SoGheConLai], [TrangThai]) VALUES (13, 2, N'VJ130', N'Hà Nội', N'TP. Hồ Chí Minh', CAST(N'2026-10-08T09:30:00.000' AS DateTime), CAST(N'2026-10-08T11:40:00.000' AS DateTime), CAST(1350000.00 AS Decimal(18, 2)), 180, 150, 1)
INSERT [dbo].[ChuyenBay] ([MaChuyenBay], [MaHangBay], [MaChuyen], [SanBayDi], [SanBayDen], [ThoiGianDi], [ThoiGianDen], [GiaVe], [SoGhe], [SoGheConLai], [TrangThai]) VALUES (14, 2, N'VJ180', N'TP. Hồ Chí Minh', N'Hà Nội', CAST(N'2026-10-08T18:20:00.000' AS DateTime), CAST(N'2026-10-08T20:30:00.000' AS DateTime), CAST(1400000.00 AS Decimal(18, 2)), 180, 155, 1)
INSERT [dbo].[ChuyenBay] ([MaChuyenBay], [MaHangBay], [MaChuyen], [SanBayDi], [SanBayDen], [ThoiGianDi], [ThoiGianDen], [GiaVe], [SoGhe], [SoGheConLai], [TrangThai]) VALUES (15, 2, N'VJ524', N'Hà Nội', N'Đà Nẵng', CAST(N'2026-10-09T13:40:00.000' AS DateTime), CAST(N'2026-10-09T15:00:00.000' AS DateTime), CAST(900000.00 AS Decimal(18, 2)), 180, 120, 1)
INSERT [dbo].[ChuyenBay] ([MaChuyenBay], [MaHangBay], [MaChuyen], [SanBayDi], [SanBayDen], [ThoiGianDi], [ThoiGianDen], [GiaVe], [SoGhe], [SoGheConLai], [TrangThai]) VALUES (16, 2, N'VJ410', N'TP. Hồ Chí Minh', N'Đà Lạt', CAST(N'2026-10-09T11:00:00.000' AS DateTime), CAST(N'2026-10-09T12:00:00.000' AS DateTime), CAST(700000.00 AS Decimal(18, 2)), 180, 135, 1)
INSERT [dbo].[ChuyenBay] ([MaChuyenBay], [MaHangBay], [MaChuyen], [SanBayDi], [SanBayDen], [ThoiGianDi], [ThoiGianDen], [GiaVe], [SoGhe], [SoGheConLai], [TrangThai]) VALUES (17, 2, N'VJ634', N'TP. Hồ Chí Minh', N'Nha Trang', CAST(N'2026-10-10T07:30:00.000' AS DateTime), CAST(N'2026-10-10T08:30:00.000' AS DateTime), CAST(750000.00 AS Decimal(18, 2)), 180, 130, 1)
INSERT [dbo].[ChuyenBay] ([MaChuyenBay], [MaHangBay], [MaChuyen], [SanBayDi], [SanBayDen], [ThoiGianDi], [ThoiGianDen], [GiaVe], [SoGhe], [SoGheConLai], [TrangThai]) VALUES (18, 2, N'VJ553', N'Hà Nội', N'Phú Quốc', CAST(N'2026-10-11T09:00:00.000' AS DateTime), CAST(N'2026-10-11T11:10:00.000' AS DateTime), CAST(1900000.00 AS Decimal(18, 2)), 180, 125, 1)
INSERT [dbo].[ChuyenBay] ([MaChuyenBay], [MaHangBay], [MaChuyen], [SanBayDi], [SanBayDen], [ThoiGianDi], [ThoiGianDen], [GiaVe], [SoGhe], [SoGheConLai], [TrangThai]) VALUES (19, 2, N'VJ638', N'Đà Nẵng', N'TP. Hồ Chí Minh', CAST(N'2026-10-12T16:00:00.000' AS DateTime), CAST(N'2026-10-12T17:20:00.000' AS DateTime), CAST(880000.00 AS Decimal(18, 2)), 180, 130, 1)
SET IDENTITY_INSERT [dbo].[ChuyenBay] OFF
GO
SET IDENTITY_INSERT [dbo].[DanhGia] ON 

INSERT [dbo].[DanhGia] ([MaDanhGia], [MaKH], [MaKS], [SoSao], [NoiDung], [NgayDanhGia], [TrangThai]) VALUES (1, 1, 1, 5, N'Khách sạn rất đẹp, phòng sạch và nhân viên nhiệt tình.', CAST(N'2026-10-01T22:02:50.387' AS DateTime), 1)
INSERT [dbo].[DanhGia] ([MaDanhGia], [MaKH], [MaKS], [SoSao], [NoiDung], [NgayDanhGia], [TrangThai]) VALUES (2, 2, 1, 4, N'Vị trí gần biển, phòng khá rộng và sạch.', CAST(N'2026-10-01T22:02:50.387' AS DateTime), 1)
INSERT [dbo].[DanhGia] ([MaDanhGia], [MaKH], [MaKS], [SoSao], [NoiDung], [NgayDanhGia], [TrangThai]) VALUES (3, 3, 2, 5, N'Không gian rất đẹp, phù hợp nghỉ dưỡng.', CAST(N'2026-10-01T22:02:50.387' AS DateTime), 1)
INSERT [dbo].[DanhGia] ([MaDanhGia], [MaKH], [MaKS], [SoSao], [NoiDung], [NgayDanhGia], [TrangThai]) VALUES (4, 1, 3, 5, N'Khách sạn cao cấp, view biển rất đẹp.', CAST(N'2026-10-01T22:02:50.387' AS DateTime), 1)
INSERT [dbo].[DanhGia] ([MaDanhGia], [MaKH], [MaKS], [SoSao], [NoiDung], [NgayDanhGia], [TrangThai]) VALUES (5, 2, 4, 4, N'Resort đẹp, dịch vụ tốt.', CAST(N'2026-10-01T22:02:50.387' AS DateTime), 1)
SET IDENTITY_INSERT [dbo].[DanhGia] OFF
GO
SET IDENTITY_INSERT [dbo].[DiaDiem] ON 

INSERT [dbo].[DiaDiem] ([MaDiaDiem], [TenDiaDiem], [ThanhPho], [QuocGia], [MoTa], [HinhAnh], [PhongCach], [TrangThai]) VALUES (1, N'Đà Nẵng', N'Đà Nẵng', N'Việt Nam', N'Thành phố biển năng động, hiện đại và nhiều điểm du lịch nổi tiếng.', N'/Content/images/destinations/danang1.jpg', N'Biển & nghỉ dưỡng', 1)
INSERT [dbo].[DiaDiem] ([MaDiaDiem], [TenDiaDiem], [ThanhPho], [QuocGia], [MoTa], [HinhAnh], [PhongCach], [TrangThai]) VALUES (2, N'Đà Lạt', N'Lâm Đồng', N'Việt Nam', N'Thành phố ngàn hoa với khí hậu mát mẻ quanh năm.', N'/Content/images/destinations/dalat1.jpg', N'Nghỉ dưỡng & thiên nhiên', 1)
INSERT [dbo].[DiaDiem] ([MaDiaDiem], [TenDiaDiem], [ThanhPho], [QuocGia], [MoTa], [HinhAnh], [PhongCach], [TrangThai]) VALUES (3, N'Nha Trang', N'Khánh Hòa', N'Việt Nam', N'Thành phố biển nổi tiếng với bãi biển đẹp và nhiều khu vui chơi.', N'/Content/images/destinations/nhatrang1.jpg', N'Biển & thành phố', 1)
INSERT [dbo].[DiaDiem] ([MaDiaDiem], [TenDiaDiem], [ThanhPho], [QuocGia], [MoTa], [HinhAnh], [PhongCach], [TrangThai]) VALUES (4, N'Phú Quốc', N'Kiên Giang', N'Việt Nam', N'Đảo ngọc với biển xanh, cát trắng và nhiều khu nghỉ dưỡng.', N'/Content/images/destinations/phuquoc1.jpg', N'Biển & nghỉ dưỡng', 1)
INSERT [dbo].[DiaDiem] ([MaDiaDiem], [TenDiaDiem], [ThanhPho], [QuocGia], [MoTa], [HinhAnh], [PhongCach], [TrangThai]) VALUES (5, N'Vũng Tàu', N'Bà Rịa - Vũng Tàu', N'Việt Nam', N'Thành phố biển gần TP.HCM, thích hợp cho các chuyến đi ngắn ngày.', N'/Content/images/destinations/vungtau1.jpg', N'Biển & cuối tuần', 1)
INSERT [dbo].[DiaDiem] ([MaDiaDiem], [TenDiaDiem], [ThanhPho], [QuocGia], [MoTa], [HinhAnh], [PhongCach], [TrangThai]) VALUES (6, N'Hà Nội', N'Hà Nội', N'Việt Nam', N'Thủ đô nghìn năm văn hiến với phố cổ, hồ Hoàn Kiếm, di tích lịch sử và ẩm thực đặc sắc.', N'/Content/images/destinations/hanoi.jpg', N'Văn hóa & lịch sử', 1)
INSERT [dbo].[DiaDiem] ([MaDiaDiem], [TenDiaDiem], [ThanhPho], [QuocGia], [MoTa], [HinhAnh], [PhongCach], [TrangThai]) VALUES (7, N'TP. Hồ Chí Minh', N'TP. Hồ Chí Minh', N'Việt Nam', N'Thành phố lớn nhất cả nước, sôi động với kiến trúc thuộc địa, chợ truyền thống và ẩm thực đường phố.', N'/Content/images/destinations/hcm.jpg', N'Đô thị & ẩm thực', 1)
INSERT [dbo].[DiaDiem] ([MaDiaDiem], [TenDiaDiem], [ThanhPho], [QuocGia], [MoTa], [HinhAnh], [PhongCach], [TrangThai]) VALUES (8, N'Hội An', N'Đà Nẵng', N'Việt Nam', N'Phố cổ ven sông Thu Bồn, được UNESCO công nhận là Di sản văn hóa thế giới, nổi tiếng với đèn lồng và ẩm thực.', N'/Content/images/destinations/hoian.jpg', N'Phố cổ & văn hóa', 1)
INSERT [dbo].[DiaDiem] ([MaDiaDiem], [TenDiaDiem], [ThanhPho], [QuocGia], [MoTa], [HinhAnh], [PhongCach], [TrangThai]) VALUES (9, N'Huế', N'Huế', N'Việt Nam', N'Cố đô với quần thể di tích triều Nguyễn, sông Hương và ẩm thực cung đình.', N'/Content/images/destinations/hue.jpg', N'Di sản & lịch sử', 1)
INSERT [dbo].[DiaDiem] ([MaDiaDiem], [TenDiaDiem], [ThanhPho], [QuocGia], [MoTa], [HinhAnh], [PhongCach], [TrangThai]) VALUES (10, N'Sa Pa', N'Lào Cai', N'Việt Nam', N'Thị trấn vùng cao với ruộng bậc thang, bản làng dân tộc thiểu số và đỉnh Fansipan.', N'/Content/images/destinations/sapa.jpg', N'Núi rừng & trekking', 1)
INSERT [dbo].[DiaDiem] ([MaDiaDiem], [TenDiaDiem], [ThanhPho], [QuocGia], [MoTa], [HinhAnh], [PhongCach], [TrangThai]) VALUES (11, N'Hạ Long', N'Quảng Ninh', N'Việt Nam', N'Nơi có vịnh Hạ Long - Di sản thiên nhiên thế giới với hàng nghìn đảo đá vôi.', N'/Content/images/destinations/halong1.jpg', N'Vịnh biển & du thuyền', 1)
INSERT [dbo].[DiaDiem] ([MaDiaDiem], [TenDiaDiem], [ThanhPho], [QuocGia], [MoTa], [HinhAnh], [PhongCach], [TrangThai]) VALUES (12, N'Ninh Bình', N'Ninh Bình', N'Việt Nam', N'Vùng đất cố đô Hoa Lư với quần thể danh thắng Tràng An, núi đá vôi và sông nước yên bình.', N'/Content/images/destinations/ninhbinh.jpg', N'Thiên nhiên & di sản', 1)
SET IDENTITY_INSERT [dbo].[DiaDiem] OFF
GO
SET IDENTITY_INSERT [dbo].[DiaDiemDuLich] ON 

INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (1, 1, N'Bà Nà Hills', N'Khu vui chơi', N'Đà Nẵng', N'Khu du lịch nổi tiếng với Cầu Vàng và cảnh quan núi rừng.', N'/Content/images/places/ba-na-hills.jpg', CAST(900000.00 AS Decimal(18, 2)), N'07:00 - 22:00', 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (2, 1, N'Bãi biển Mỹ Khê', N'Bãi biển', N'Đà Nẵng', N'Một trong những bãi biển nổi tiếng nhất Đà Nẵng.', N'/Content/images/places/my-khe.jpg', CAST(0.00 AS Decimal(18, 2)), N'05:00 - 22:00', 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (3, 1, N'Cầu Rồng', N'Công trình', N'Đà Nẵng', N'Biểu tượng nổi tiếng của thành phố Đà Nẵng.', N'/Content/images/places/cau-rong.jpg', CAST(0.00 AS Decimal(18, 2)), N'Cả ngày', 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (4, 2, N'Hồ Xuân Hương', N'Hồ', N'Đà Lạt', N'Địa điểm tham quan nổi tiếng nằm tại trung tâm Đà Lạt.', N'/Content/images/places/ho-xuan-huong.jpg', CAST(0.00 AS Decimal(18, 2)), N'Cả ngày', 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (5, 2, N'Đồi chè Cầu Đất', N'Đồi chè', N'Xuân Trường, Đà Lạt', N'Địa điểm check-in nổi tiếng với những đồi chè xanh mướt.', N'/Content/images/places/cau-dat.jpg', CAST(0.00 AS Decimal(18, 2)), N'06:00 - 18:00', 0, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (6, 3, N'VinWonders Nha Trang', N'Khu vui chơi', N'Nha Trang', N'Khu vui chơi giải trí nổi tiếng tại Nha Trang.', N'/Content/images/places/vinwonders-nhatrang.jpg', CAST(950000.00 AS Decimal(18, 2)), N'08:30 - 21:00', 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (7, 4, N'Bãi Sao', N'Bãi biển', N'Phú Quốc', N'Bãi biển nổi tiếng với cát trắng và nước biển trong xanh.', N'/Content/images/places/bai-sao.jpg', CAST(0.00 AS Decimal(18, 2)), N'Cả ngày', 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (8, 5, N'Hải đăng Vũng Tàu', N'Di tích', N'Vũng Tàu', N'Địa điểm ngắm cảnh nổi tiếng của Vũng Tàu.', N'/Content/images/places/hai-dang-vung-tau.jpg', CAST(0.00 AS Decimal(18, 2)), N'06:00 - 18:00', 0, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (9, 1, N'Ngũ Hành Sơn', N'Thiên nhiên', N'Hòa Hải, Ngũ Hành Sơn, Đà Nẵng', N'Cụm núi đá cẩm thạch với hang động và chùa chiền.', NULL, NULL, NULL, 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (10, 1, N'Bán đảo Sơn Trà', N'Thiên nhiên', N'Sơn Trà, Đà Nẵng', N'Bán đảo xanh với đường đèo ven biển, nổi tiếng với chùa Linh Ứng và voọc chà vá chân nâu.', NULL, CAST(0.00 AS Decimal(18, 2)), N'Cả ngày', 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (11, 1, N'Bảo tàng Điêu khắc Chăm', N'Bảo tàng', N'2 Tháng 9, Hải Châu, Đà Nẵng', N'Bảo tàng lưu giữ bộ sưu tập điêu khắc Chăm Pa lớn nhất thế giới.', NULL, NULL, NULL, 0, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (12, 2, N'Thiền viện Trúc Lâm', N'Tôn giáo', N'Phù Đổng Thiên Vương, Đà Lạt', N'Thiền viện nằm trên đồi thông, nhìn xuống hồ Tuyền Lâm.', NULL, NULL, NULL, 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (13, 2, N'Chợ Đà Lạt', N'Chợ', N'Nguyễn Thị Minh Khai, Đà Lạt', N'Chợ trung tâm thành phố, bán rau củ, hoa, đặc sản và đồ ăn vặt.', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, 0, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (14, 2, N'Dinh Bảo Đại', N'Di tích', N'Triệu Việt Vương, Đà Lạt', N'Biệt thự nghỉ mát của vua Bảo Đại, còn gọi là Dinh 3.', NULL, NULL, NULL, 0, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (15, 3, N'Tháp Bà Ponagar', N'Di tích', N'2 Tháng 4, Vĩnh Phước, Nha Trang', N'Quần thể tháp Chăm cổ trên đồi Cù Lao, còn giữ nhiều nét kiến trúc đặc trưng.', NULL, NULL, NULL, 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (16, 3, N'Chùa Long Sơn', N'Tôn giáo', N'23 Tháng 10, Nha Trang', N'Ngôi chùa có tượng Phật trắng lớn trên đỉnh đồi nhìn xuống thành phố.', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, 0, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (17, 3, N'Hòn Chồng', N'Thiên nhiên', N'Vĩnh Phước, Nha Trang', N'Cụm đá lớn ven biển, nơi ngắm bình minh và hoàng hôn.', NULL, NULL, NULL, 0, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (18, 4, N'Dinh Cậu', N'Di tích', N'Dương Đông, Phú Quốc', N'Ngôi miếu nhỏ trên mỏm đá ven biển, gắn với tín ngưỡng của ngư dân địa phương.', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, 0, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (19, 4, N'Vinpearl Safari Phú Quốc', N'Khu vui chơi', N'Bãi Dài, Gành Dầu, Phú Quốc', N'Vườn thú bán hoang dã tại Bãi Dài.', NULL, NULL, NULL, 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (20, 4, N'Bãi Khem', N'Bãi biển', N'An Thới, Phú Quốc', N'Bãi biển cát trắng mịn ở phía nam đảo.', NULL, CAST(0.00 AS Decimal(18, 2)), N'Cả ngày', 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (21, 5, N'Tượng Chúa Kitô Vua', N'Công trình', N'Núi Nhỏ, Vũng Tàu', N'Tượng Chúa Kitô cao trên đỉnh núi Nhỏ, nhìn ra biển.', NULL, NULL, NULL, 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (22, 5, N'Bạch Dinh', N'Di tích', N'4 Trần Phú, Vũng Tàu', N'Biệt thự kiến trúc Pháp nằm dưới chân núi Lớn.', NULL, NULL, NULL, 0, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (23, 5, N'Bãi Sau', N'Bãi biển', N'Thùy Vân, Vũng Tàu', N'Bãi biển dài, nhiều khách sạn và quán ăn dọc đường Thùy Vân.', NULL, CAST(0.00 AS Decimal(18, 2)), N'Cả ngày', 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (24, 6, N'Hồ Hoàn Kiếm', N'Hồ', N'Hoàn Kiếm, Hà Nội', N'Hồ nước trung tâm thủ đô, gắn với truyền thuyết trả gươm và có đền Ngọc Sơn nằm trên đảo nhỏ.', NULL, CAST(0.00 AS Decimal(18, 2)), N'Cả ngày', 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (25, 6, N'Văn Miếu - Quốc Tử Giám', N'Di tích', N'58 Quốc Tử Giám, Đống Đa, Hà Nội', N'Quần thể di tích được xây dựng từ thời Lý, được xem là trường đại học đầu tiên của Việt Nam.', NULL, NULL, NULL, 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (26, 6, N'Hoàng thành Thăng Long', N'Di tích', N'19C Hoàng Diệu, Ba Đình, Hà Nội', N'Khu di tích trung tâm của kinh thành Thăng Long xưa, được UNESCO công nhận là Di sản văn hóa thế giới.', NULL, NULL, NULL, 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (27, 6, N'Lăng Chủ tịch Hồ Chí Minh', N'Công trình', N'2 Hùng Vương, Ba Đình, Hà Nội', N'Công trình tưởng niệm Chủ tịch Hồ Chí Minh tại quảng trường Ba Đình, vào cửa miễn phí; lịch mở cửa thay đổi theo mùa.', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (28, 6, N'Phố cổ Hà Nội', N'Khu phố', N'Hoàn Kiếm, Hà Nội', N'Khu phố với các tuyến phố nghề truyền thống, nhiều quán ăn và cà phê nổi tiếng.', NULL, CAST(0.00 AS Decimal(18, 2)), N'Cả ngày', 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (29, 6, N'Nhà thờ Lớn Hà Nội', N'Công trình', N'40 Nhà Chung, Hoàn Kiếm, Hà Nội', N'Nhà thờ kiến trúc Gothic gần hồ Hoàn Kiếm, là điểm check-in quen thuộc của du khách.', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, 0, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (30, 7, N'Dinh Độc Lập', N'Di tích', N'135 Nam Kỳ Khởi Nghĩa, TP. Hồ Chí Minh', N'Di tích lịch sử gắn với sự kiện 30/4/1975, nay là nơi tham quan mở cửa cho công chúng.', NULL, CAST(65000.00 AS Decimal(18, 2)), NULL, 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (31, 7, N'Bảo tàng Chứng tích Chiến tranh', N'Bảo tàng', N'28 Võ Văn Tần, TP. Hồ Chí Minh', N'Bảo tàng trưng bày hình ảnh, hiện vật về chiến tranh Việt Nam.', NULL, CAST(40000.00 AS Decimal(18, 2)), NULL, 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (32, 7, N'Nhà thờ Đức Bà Sài Gòn', N'Công trình', N'1 Công xã Paris, Quận 1, TP. Hồ Chí Minh', N'Nhà thờ gạch đỏ do người Pháp xây dựng, nằm ngay trung tâm Quận 1.', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (33, 7, N'Bưu điện Trung tâm Sài Gòn', N'Công trình', N'2 Công xã Paris, Quận 1, TP. Hồ Chí Minh', N'Công trình kiến trúc Pháp cạnh Nhà thờ Đức Bà, vẫn đang hoạt động như bưu điện.', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, 0, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (34, 7, N'Chợ Bến Thành', N'Chợ', N'Lê Lợi, Quận 1, TP. Hồ Chí Minh', N'Ngôi chợ biểu tượng của thành phố, bán đồ lưu niệm, thực phẩm và ẩm thực địa phương.', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (35, 7, N'Phố đi bộ Nguyễn Huệ', N'Khu phố', N'Nguyễn Huệ, Quận 1, TP. Hồ Chí Minh', N'Không gian đi bộ dẫn từ UBND Thành phố ra bờ sông Sài Gòn.', NULL, CAST(0.00 AS Decimal(18, 2)), N'Cả ngày', 0, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (36, 8, N'Phố cổ Hội An', N'Khu phố', N'Hội An, Đà Nẵng', N'Khu phố cổ là Di sản văn hóa thế giới, vé tham quan khoảng 120.000đ/người cho các điểm trong phố cổ.', NULL, CAST(120000.00 AS Decimal(18, 2)), NULL, 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (37, 8, N'Chùa Cầu', N'Di tích', N'Nguyễn Thị Minh Khai, Hội An, Đà Nẵng', N'Cây cầu có mái che mang kiến trúc Nhật Bản, biểu tượng của phố cổ Hội An.', NULL, NULL, NULL, 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (38, 8, N'Biển An Bàng', N'Bãi biển', N'An Bàng, Hội An, Đà Nẵng', N'Bãi biển cách phố cổ vài km, có nhiều quán ăn và khu nghỉ dưỡng ven biển.', NULL, CAST(0.00 AS Decimal(18, 2)), N'Cả ngày', 0, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (39, 8, N'Rừng dừa Bảy Mẫu', N'Thiên nhiên', N'Cẩm Thanh, Hội An, Đà Nẵng', N'Rừng dừa nước nơi du khách đi thuyền thúng khám phá đời sống vùng sông nước.', NULL, NULL, NULL, 0, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (40, 8, N'Cù Lao Chàm', N'Đảo', N'Cù Lao Chàm, Hội An, Đà Nẵng', N'Cụm đảo ngoài khơi Hội An, thích hợp lặn ngắm san hô và tham quan làng chài.', NULL, NULL, NULL, 0, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (41, 9, N'Đại Nội Huế', N'Di tích', N'Phú Xuân, Huế', N'Hoàng cung triều Nguyễn bên trong Kinh thành Huế, thuộc quần thể di tích cố đô Huế.', NULL, CAST(200000.00 AS Decimal(18, 2)), NULL, 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (42, 9, N'Chùa Thiên Mụ', N'Di tích', N'Kim Long, Huế', N'Ngôi chùa cổ bên bờ sông Hương, một trong những biểu tượng của Huế.', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (43, 9, N'Lăng Khải Định', N'Di tích', N'Thủy Bằng, Hương Thủy, Huế', N'Lăng vua Khải Định nổi bật với sự kết hợp kiến trúc Đông - Tây.', NULL, CAST(150000.00 AS Decimal(18, 2)), NULL, 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (44, 9, N'Lăng Minh Mạng', N'Di tích', N'Hương Thọ, Hương Trà, Huế', N'Lăng vua Minh Mạng với không gian rộng, hài hòa với cảnh quan thiên nhiên.', NULL, CAST(150000.00 AS Decimal(18, 2)), NULL, 0, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (45, 9, N'Lăng Tự Đức', N'Di tích', N'Thủy Xuân, Huế', N'Lăng vua Tự Đức có hồ nước, rừng thông và nhiều công trình nhỏ bên trong.', NULL, CAST(150000.00 AS Decimal(18, 2)), NULL, 0, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (46, 9, N'Sông Hương', N'Sông', N'Huế', N'Dòng sông chảy qua trung tâm Huế, nổi tiếng với các chuyến du thuyền ngắm cảnh.', NULL, CAST(0.00 AS Decimal(18, 2)), N'Cả ngày', 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (47, 10, N'Đỉnh Fansipan', N'Thiên nhiên', N'Sa Pa, Lào Cai', N'Đỉnh núi cao 3.143 m, có thể lên bằng cáp treo hoặc trekking.', NULL, NULL, NULL, 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (48, 10, N'Bản Cát Cát', N'Làng bản', N'Cát Cát, Sa Pa, Lào Cai', N'Bản làng của người H''Mông, nổi tiếng với nghề thủ công và cảnh quan ruộng bậc thang.', NULL, NULL, NULL, 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (49, 10, N'Thung lũng Mường Hoa', N'Thiên nhiên', N'Mường Hoa, Sa Pa, Lào Cai', N'Thung lũng với ruộng bậc thang và các bản làng người dân tộc thiểu số.', NULL, CAST(0.00 AS Decimal(18, 2)), N'Cả ngày', 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (50, 10, N'Thác Bạc', N'Thác nước', N'Quốc lộ 4D, Sa Pa, Lào Cai', N'Thác nước cao nằm trên đường đi đèo Ô Quy Hồ.', NULL, NULL, NULL, 0, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (51, 10, N'Nhà thờ đá Sa Pa', N'Công trình', N'Trung tâm Sa Pa, Lào Cai', N'Nhà thờ cổ bằng đá ở trung tâm thị trấn, gần quảng trường.', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, 0, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (52, 11, N'Vịnh Hạ Long', N'Thiên nhiên', N'Hạ Long, Quảng Ninh', N'Vịnh có hàng nghìn đảo đá vôi, được UNESCO công nhận là Di sản thiên nhiên thế giới.', NULL, NULL, NULL, 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (53, 11, N'Hang Sửng Sốt', N'Hang động', N'Vịnh Hạ Long, Quảng Ninh', N'Một trong những hang động đẹp và rộng nhất trên vịnh Hạ Long.', NULL, NULL, NULL, 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (54, 11, N'Đảo Ti Tốp', N'Đảo', N'Vịnh Hạ Long, Quảng Ninh', N'Đảo nhỏ có bãi tắm và đường leo lên đỉnh ngắm toàn cảnh vịnh.', NULL, NULL, NULL, 0, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (55, 11, N'Sun World Hạ Long', N'Khu vui chơi', N'Bãi Cháy, Hạ Long, Quảng Ninh', N'Tổ hợp vui chơi giải trí tại Bãi Cháy với cáp treo và vòng quay mặt trời.', NULL, NULL, NULL, 0, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (56, 12, N'Quần thể danh thắng Tràng An', N'Thiên nhiên', N'Tràng An, Hoa Lư, Ninh Bình', N'Quần thể núi đá vôi, hang động và sông nước được UNESCO công nhận là Di sản thế giới.', NULL, NULL, NULL, 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (57, 12, N'Tam Cốc - Bích Động', N'Thiên nhiên', N'Ninh Hải, Hoa Lư, Ninh Bình', N'Tuyến đò ngắm cảnh sông Ngô Đồng và chùa Bích Động.', NULL, NULL, NULL, 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (58, 12, N'Chùa Bái Đình', N'Tôn giáo', N'Gia Sinh, Gia Viễn, Ninh Bình', N'Quần thể chùa quy mô lớn với nhiều tượng và công trình kiến trúc.', NULL, NULL, NULL, 1, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (59, 12, N'Hang Múa', N'Thiên nhiên', N'Ninh Xuân, Hoa Lư, Ninh Bình', N'Điểm leo bậc thang lên đỉnh núi ngắm toàn cảnh Tam Cốc.', NULL, NULL, NULL, 0, 1)
INSERT [dbo].[DiaDiemDuLich] ([MaDDDL], [MaDiaDiem], [TenDiaDiem], [LoaiDiaDiem], [DiaChi], [MoTa], [HinhAnh], [GiaVe], [GioMoCua], [NoiBat], [TrangThai]) VALUES (60, 12, N'Cố đô Hoa Lư', N'Di tích', N'Trường Yên, Hoa Lư, Ninh Bình', N'Kinh đô của nước Đại Cồ Việt thời nhà Đinh và Tiền Lê.', NULL, NULL, NULL, 0, 1)
SET IDENTITY_INSERT [dbo].[DiaDiemDuLich] OFF
GO
SET IDENTITY_INSERT [dbo].[DonDatPhong] ON 

INSERT [dbo].[DonDatPhong] ([MaDon], [MaDonCode], [MaKH], [MaKS], [MaKM], [NgayNhanPhong], [NgayTraPhong], [SoNguoiLon], [SoTreEm], [TongTien], [GiamGia], [ThanhTien], [TrangThai], [TrangThaiCode], [PhuongThucThanhToan], [GhiChu], [NgayHetHan], [NgayDat]) VALUES (1, N'TH000001', 1, 1, 1, CAST(N'2026-10-01' AS Date), CAST(N'2026-10-03' AS Date), 2, 0, CAST(1800000.00 AS Decimal(18, 2)), CAST(180000.00 AS Decimal(18, 2)), CAST(1620000.00 AS Decimal(18, 2)), N'Đã xác nhận', N'Confirmed', N'Chuyển khoản', N'Khách yêu cầu phòng view biển.', NULL, CAST(N'2026-10-01T22:02:50.430' AS DateTime))
INSERT [dbo].[DonDatPhong] ([MaDon], [MaDonCode], [MaKH], [MaKS], [MaKM], [NgayNhanPhong], [NgayTraPhong], [SoNguoiLon], [SoTreEm], [TongTien], [GiamGia], [ThanhTien], [TrangThai], [TrangThaiCode], [PhuongThucThanhToan], [GhiChu], [NgayHetHan], [NgayDat]) VALUES (2, N'TH000002', 2, 2, NULL, CAST(N'2026-10-10' AS Date), CAST(N'2026-10-12' AS Date), 2, 0, CAST(1600000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(1600000.00 AS Decimal(18, 2)), N'Chờ xác nhận', N'Pending', N'Thanh toán khi nhận phòng', NULL, NULL, CAST(N'2026-10-01T22:02:50.430' AS DateTime))
INSERT [dbo].[DonDatPhong] ([MaDon], [MaDonCode], [MaKH], [MaKS], [MaKM], [NgayNhanPhong], [NgayTraPhong], [SoNguoiLon], [SoTreEm], [TongTien], [GiamGia], [ThanhTien], [TrangThai], [TrangThaiCode], [PhuongThucThanhToan], [GhiChu], [NgayHetHan], [NgayDat]) VALUES (3, N'TH261002003611', 1, 1, NULL, CAST(N'2026-10-03' AS Date), CAST(N'2026-10-05' AS Date), 2, 0, CAST(1800000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(1800000.00 AS Decimal(18, 2)), N'Chờ xác nhận', N'Pending', NULL, N'', CAST(N'2026-10-02T00:56:11.997' AS DateTime), CAST(N'2026-10-02T00:36:11.997' AS DateTime))
INSERT [dbo].[DonDatPhong] ([MaDon], [MaDonCode], [MaKH], [MaKS], [MaKM], [NgayNhanPhong], [NgayTraPhong], [SoNguoiLon], [SoTreEm], [TongTien], [GiamGia], [ThanhTien], [TrangThai], [TrangThaiCode], [PhuongThucThanhToan], [GhiChu], [NgayHetHan], [NgayDat]) VALUES (4, N'TH261002003622', 1, 1, NULL, CAST(N'2026-10-03' AS Date), CAST(N'2026-10-05' AS Date), 2, 0, CAST(1800000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(1800000.00 AS Decimal(18, 2)), N'Đã thanh toán', N'Paid', N'Thẻ nội địa', N'', CAST(N'2026-10-02T00:56:22.167' AS DateTime), CAST(N'2026-10-02T00:36:22.167' AS DateTime))
INSERT [dbo].[DonDatPhong] ([MaDon], [MaDonCode], [MaKH], [MaKS], [MaKM], [NgayNhanPhong], [NgayTraPhong], [SoNguoiLon], [SoTreEm], [TongTien], [GiamGia], [ThanhTien], [TrangThai], [TrangThaiCode], [PhuongThucThanhToan], [GhiChu], [NgayHetHan], [NgayDat]) VALUES (5, N'TH261002103543', 1, 1, NULL, CAST(N'2026-10-02' AS Date), CAST(N'2026-10-05' AS Date), 2, 0, CAST(2700000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(2700000.00 AS Decimal(18, 2)), N'Chờ xác nhận', N'Pending', NULL, N'', CAST(N'2026-10-02T10:55:43.330' AS DateTime), CAST(N'2026-10-02T10:35:43.330' AS DateTime))
INSERT [dbo].[DonDatPhong] ([MaDon], [MaDonCode], [MaKH], [MaKS], [MaKM], [NgayNhanPhong], [NgayTraPhong], [SoNguoiLon], [SoTreEm], [TongTien], [GiamGia], [ThanhTien], [TrangThai], [TrangThaiCode], [PhuongThucThanhToan], [GhiChu], [NgayHetHan], [NgayDat]) VALUES (6, N'TH261002103724', 1, 1, NULL, CAST(N'2026-10-03' AS Date), CAST(N'2026-10-05' AS Date), 2, 0, CAST(1800000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(1800000.00 AS Decimal(18, 2)), N'Đã thanh toán', N'Paid', N'Thẻ nội địa', N'', CAST(N'2026-10-02T10:57:24.020' AS DateTime), CAST(N'2026-10-02T10:37:24.020' AS DateTime))
INSERT [dbo].[DonDatPhong] ([MaDon], [MaDonCode], [MaKH], [MaKS], [MaKM], [NgayNhanPhong], [NgayTraPhong], [SoNguoiLon], [SoTreEm], [TongTien], [GiamGia], [ThanhTien], [TrangThai], [TrangThaiCode], [PhuongThucThanhToan], [GhiChu], [NgayHetHan], [NgayDat]) VALUES (7, N'TH261002103745', 1, 1, NULL, CAST(N'2026-10-03' AS Date), CAST(N'2026-10-05' AS Date), 2, 0, CAST(1800000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(1800000.00 AS Decimal(18, 2)), N'Chờ xác nhận', N'Pending', NULL, N'', CAST(N'2026-10-02T10:57:45.967' AS DateTime), CAST(N'2026-10-02T10:37:45.967' AS DateTime))
INSERT [dbo].[DonDatPhong] ([MaDon], [MaDonCode], [MaKH], [MaKS], [MaKM], [NgayNhanPhong], [NgayTraPhong], [SoNguoiLon], [SoTreEm], [TongTien], [GiamGia], [ThanhTien], [TrangThai], [TrangThaiCode], [PhuongThucThanhToan], [GhiChu], [NgayHetHan], [NgayDat]) VALUES (8, N'TH261002112656', 1, 1, NULL, CAST(N'2026-10-03' AS Date), CAST(N'2026-10-05' AS Date), 2, 0, CAST(1800000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(1800000.00 AS Decimal(18, 2)), N'Đã thanh toán', N'Paid', N'Thẻ nội địa', N'tôi muốn cái view phải đẹp một xíu
', CAST(N'2026-10-02T11:46:56.803' AS DateTime), CAST(N'2026-10-02T11:26:56.803' AS DateTime))
SET IDENTITY_INSERT [dbo].[DonDatPhong] OFF
GO
SET IDENTITY_INSERT [dbo].[HangBay] ON 

INSERT [dbo].[HangBay] ([MaHangBay], [TenHangBay], [MaIATA], [Logo], [TrangThai]) VALUES (1, N'Vietnam Airlines', N'VN', N'/Content/images/airlines/vietnam-airlines.png', 1)
INSERT [dbo].[HangBay] ([MaHangBay], [TenHangBay], [MaIATA], [Logo], [TrangThai]) VALUES (2, N'Vietjet Air', N'VJ', N'/Content/images/airlines/vietjet.png', 1)
INSERT [dbo].[HangBay] ([MaHangBay], [TenHangBay], [MaIATA], [Logo], [TrangThai]) VALUES (3, N'Bamboo Airways', N'QH', N'/Content/images/airlines/bamboo.png', 1)
INSERT [dbo].[HangBay] ([MaHangBay], [TenHangBay], [MaIATA], [Logo], [TrangThai]) VALUES (4, N'Vietravel Airlines', N'VU', N'/Content/images/airlines/vietravel.png', 1)
SET IDENTITY_INSERT [dbo].[HangBay] OFF
GO
SET IDENTITY_INSERT [dbo].[HinhAnhKhachSan] ON 

INSERT [dbo].[HinhAnhKhachSan] ([MaHinhAnh], [MaKS], [DuongDanAnh], [MoTa], [LaAnhDaiDien]) VALUES (1, 1, N'/Content/images/hotels/danang-1.jpg', N'Mặt tiền khách sạn', 1)
INSERT [dbo].[HinhAnhKhachSan] ([MaHinhAnh], [MaKS], [DuongDanAnh], [MoTa], [LaAnhDaiDien]) VALUES (2, 1, N'/Content/images/hotels/danang-2.jpg', N'Phòng khách sạn', 0)
INSERT [dbo].[HinhAnhKhachSan] ([MaHinhAnh], [MaKS], [DuongDanAnh], [MoTa], [LaAnhDaiDien]) VALUES (3, 1, N'/Content/images/hotels/danang-3.jpg', N'Hồ bơi', 0)
INSERT [dbo].[HinhAnhKhachSan] ([MaHinhAnh], [MaKS], [DuongDanAnh], [MoTa], [LaAnhDaiDien]) VALUES (4, 1, N'/Content/images/hotels/danang-4.jpg', N'View biển', 0)
INSERT [dbo].[HinhAnhKhachSan] ([MaHinhAnh], [MaKS], [DuongDanAnh], [MoTa], [LaAnhDaiDien]) VALUES (5, 2, N'/Content/images/hotels/dalat-1.jpg', N'Mặt tiền khách sạn', 1)
INSERT [dbo].[HinhAnhKhachSan] ([MaHinhAnh], [MaKS], [DuongDanAnh], [MoTa], [LaAnhDaiDien]) VALUES (6, 2, N'/Content/images/hotels/dalat-2.jpg', N'Phòng nghỉ', 0)
INSERT [dbo].[HinhAnhKhachSan] ([MaHinhAnh], [MaKS], [DuongDanAnh], [MoTa], [LaAnhDaiDien]) VALUES (7, 2, N'/Content/images/hotels/dalat-3.jpg', N'Khu vườn', 0)
INSERT [dbo].[HinhAnhKhachSan] ([MaHinhAnh], [MaKS], [DuongDanAnh], [MoTa], [LaAnhDaiDien]) VALUES (8, 3, N'/Content/images/hotels/nhatrang-1.jpg', N'Mặt tiền khách sạn', 1)
INSERT [dbo].[HinhAnhKhachSan] ([MaHinhAnh], [MaKS], [DuongDanAnh], [MoTa], [LaAnhDaiDien]) VALUES (9, 3, N'/Content/images/hotels/nhatrang-2.jpg', N'Phòng nghỉ', 0)
INSERT [dbo].[HinhAnhKhachSan] ([MaHinhAnh], [MaKS], [DuongDanAnh], [MoTa], [LaAnhDaiDien]) VALUES (10, 3, N'/Content/images/hotels/nhatrang-3.jpg', N'View biển', 0)
INSERT [dbo].[HinhAnhKhachSan] ([MaHinhAnh], [MaKS], [DuongDanAnh], [MoTa], [LaAnhDaiDien]) VALUES (11, 4, N'/Content/images/hotels/phuquoc-1.jpg', N'Resort', 1)
INSERT [dbo].[HinhAnhKhachSan] ([MaHinhAnh], [MaKS], [DuongDanAnh], [MoTa], [LaAnhDaiDien]) VALUES (12, 4, N'/Content/images/hotels/phuquoc-2.jpg', N'Phòng nghỉ', 0)
INSERT [dbo].[HinhAnhKhachSan] ([MaHinhAnh], [MaKS], [DuongDanAnh], [MoTa], [LaAnhDaiDien]) VALUES (13, 4, N'/Content/images/hotels/phuquoc-3.jpg', N'Hồ bơi', 0)
INSERT [dbo].[HinhAnhKhachSan] ([MaHinhAnh], [MaKS], [DuongDanAnh], [MoTa], [LaAnhDaiDien]) VALUES (14, 5, N'/Content/images/hotels/vungtau-1.jpg', N'Mặt tiền khách sạn', 1)
INSERT [dbo].[HinhAnhKhachSan] ([MaHinhAnh], [MaKS], [DuongDanAnh], [MoTa], [LaAnhDaiDien]) VALUES (15, 5, N'/Content/images/hotels/vungtau-2.jpg', N'Phòng nghỉ', 0)
SET IDENTITY_INSERT [dbo].[HinhAnhKhachSan] OFF
GO
SET IDENTITY_INSERT [dbo].[KhachHang] ON 

INSERT [dbo].[KhachHang] ([MaKH], [HoTen], [Email], [SoDienThoai], [NgaySinh], [GioiTinh], [HinhAnh], [TrangThai], [NgayTao]) VALUES (1, N'Nguyễn Văn An', N'an@gmail.com', N'0901234567', CAST(N'2005-05-10' AS Date), N'Nam', N'/Content/images/users/an.jpg', 1, CAST(N'2026-10-01T22:02:50.207' AS DateTime))
INSERT [dbo].[KhachHang] ([MaKH], [HoTen], [Email], [SoDienThoai], [NgaySinh], [GioiTinh], [HinhAnh], [TrangThai], [NgayTao]) VALUES (2, N'Trần Thị Lan', N'lan@gmail.com', N'0912345678', CAST(N'2004-08-15' AS Date), N'Nữ', N'/Content/images/users/lan.jpg', 1, CAST(N'2026-10-01T22:02:50.207' AS DateTime))
INSERT [dbo].[KhachHang] ([MaKH], [HoTen], [Email], [SoDienThoai], [NgaySinh], [GioiTinh], [HinhAnh], [TrangThai], [NgayTao]) VALUES (3, N'Lê Minh Anh', N'anh@gmail.com', N'0923456789', CAST(N'2006-01-20' AS Date), N'Nữ', N'/Content/images/users/anh.jpg', 1, CAST(N'2026-10-01T22:02:50.207' AS DateTime))
INSERT [dbo].[KhachHang] ([MaKH], [HoTen], [Email], [SoDienThoai], [NgaySinh], [GioiTinh], [HinhAnh], [TrangThai], [NgayTao]) VALUES (4, N'Lê Thanh Vĩ', N'html@gmail.com', N'010288808', NULL, NULL, NULL, 1, CAST(N'2026-10-02T11:32:16.870' AS DateTime))
SET IDENTITY_INSERT [dbo].[KhachHang] OFF
GO
SET IDENTITY_INSERT [dbo].[KhachSan] ON 

INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (1, N'Sea View Da Nang Hotel', N'Lô 41 Hoang Sa,Phường Tho Quang ,Quận Sơn Trà , Da Nang ., Thọ Quang, Đà Nẵng, Việt Nam', 1, 4, N'Khách sạn gần biển Mỹ Khê với không gian hiện đại và view biển đẹp.', 4, CAST(1850000.00 AS Decimal(18, 2)), N'/Content/images/hotels/seaview-danang.jpg', N'Khách sạn', N'02361234567', N'contact@seaviewdanang.vn', N'Nhận phòng từ 14:00, trả phòng trước 12:00. Vui lòng mang theo giấy tờ tùy thân khi nhận phòng.', 1, CAST(N'2026-10-01T22:02:50.320' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (2, N'The Western Hill Hotel', N'135-145 Phan Đình Phùng, Trung tâm Thành phố Đà Lạt, Đà Lạt, Việt Nam', 2, 4, N'Khách sạn phong cách hiện đại, gần trung tâm thành phố Đà Lạt.', 4, CAST(1016492.00 AS Decimal(18, 2)), N'/Content/images/hotels/dalat-thewesternhill.jpg', N'Khách sạn', N'0263 3565 888', N' sm1@thewesternhill.com', N'Nhận phòng từ 14:00, trả phòng trước 12:00. Vui lòng mang theo giấy tờ tùy thân khi nhận phòng.', 1, CAST(N'2026-10-01T22:02:50.320' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (3, N'AQUAMARINE CAM RANH', N'Km 20 Nguyen Tat Thanh St, Bai Dai Beach, Cam Hai Dong Ward, Cam Lam Town, Khanh Hoa Province, Cam Hải Đ&#244;ng, Nha Trang, Vietnam, 65000 , Cam Hải Đông, Nha Trang, Việt Nam', 3, 4, N'Khách sạn cao cấp nằm gần bãi biển Nha Trang.', 5, CAST(4383111.00 AS Decimal(18, 2)), N'/Content/images/hotels/aquamarine-nhatrang.jpg', N'Khách sạn cao cấp', N'02583 680 888', N'info@aquamarineresort.vn', N'Nhận phòng từ 14:00, trả phòng trước 12:00. Vui lòng mang theo giấy tờ tùy thân khi nhận phòng.', 1, CAST(N'2026-10-01T22:02:50.320' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (4, N'Sunset Hill Resort Phú Quốc', N'Hẻm 113 đường Trần Hưng Đạo, Khu Phố 7 Dương Đông, Đặc Khu Phú Quốc, tỉnh An Giang, Sunset Hill Resort, Dương Đông, Đảo Phú Quốc, Việt Nam', 4, 4, N'Resort cao cấp với không gian nghỉ dưỡng ven biển.', 5, CAST(1800000.00 AS Decimal(18, 2)), N'/Content/images/hotels/phuquoc-sunsethill.jpg', N'Resort', N'02973888888', N'contact@phuquocsunset.vn', N'Nhận phòng từ 14:00, trả phòng trước 12:00. Vui lòng mang theo giấy tờ tùy thân khi nhận phòng.', 1, CAST(N'2026-10-01T22:02:50.320' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (5, N'Annata Beach Hotel', N'Thắng Tam, TP. Vũng Tàu', 5, 4, N'Khách sạn gần biển, phù hợp cho chuyến du lịch cuối tuần.', 3, CAST(650000.00 AS Decimal(18, 2)), N'/Content/images/hotels/vungtau-annatabeach.jpg', N'Khách sạn', N'0254 7303 839', N'rev@annatabeachhotel.vn', N'Nhận phòng từ 14:00, trả phòng trước 12:00. Vui lòng mang theo giấy tờ tùy thân khi nhận phòng.', 1, CAST(N'2026-10-01T22:02:50.320' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (6, N'Peninsula Hotel Danang', N'84 Võ Nguyên Giáp, Sơn Trà, Đà Nẵng', 1, 4, N'Peninsula Hotel Danang là khách sạn 5 sao tọa lạc tại số 84 Võ Nguyên Giáp, quận Sơn Trà, thành phố Đà Nẵng, nằm ngay đối diện bãi biển Mỹ Khê thơ mộng', 5, CAST(1781000.00 AS Decimal(18, 2)), N'/Content/images/hotels/hotel6.jpg', N'Hotel', N'+84 (0) 236 3816 666', N'info@peninsulahotel.vn', N'Nhận phòng từ 14:00, trả phòng trước 12:00. Vui lòng mang theo giấy tờ tùy thân khi nhận phòng.', 1, CAST(N'2026-10-02T11:14:30.907' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (7, N'InterContinental Danang Sun Peninsula Resort', N'Bãi Bắc, bán đảo Sơn Trà, Đà Nẵng', 1, 4, N'Resort cao cấp trên bán đảo Sơn Trà, có bãi biển riêng.', 5, CAST(20600000.00 AS Decimal(18, 2)), N'/Content/images/hotels/ic-danang.jpg', N'Resort', N'+84 236 393 8888', NULL, N'Giờ nhận/trả phòng và chính sách hủy phòng áp dụng theo quy định của từng khách sạn; vui lòng liên hệ khách sạn để biết chi tiết.', 1, CAST(N'2026-10-04T17:12:33.113' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (8, N'Hyatt Regency Danang Resort & Spa', N'05 Trường Sa, Hòa Hải, Đà Nẵng, Việt Nam', 1, 4, N'Resort ven biển Non Nước, gần danh thắng Ngũ Hành Sơn.', 5, CAST(5050000.00 AS Decimal(18, 2)), N'/Content/images/hotels/hyatt-danang.jpg', N'Resort', N'+84 236 3981 234', NULL, N'Giờ nhận/trả phòng và chính sách hủy phòng áp dụng theo quy định của từng khách sạn; vui lòng liên hệ khách sạn để biết chi tiết.', 1, CAST(N'2026-10-04T17:12:33.113' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (9, N'Dalat Palace Heritage Hotel', N'2 Trần Phú, Đà Lạt, Lâm Đồng', 2, 4, N'Khách sạn kiến trúc Pháp từ đầu thế kỷ 20, nhìn ra hồ Xuân Hương.', 5, CAST(3500000.00 AS Decimal(18, 2)), N'/Content/images/hotels/dalat-palace.jpg', N'Khách sạn', N'+84 263 3825 444', N'palace.reservations@royaldl.com', N'Giờ nhận/trả phòng và chính sách hủy phòng áp dụng theo quy định của từng khách sạn; vui lòng liên hệ khách sạn để biết chi tiết.', 1, CAST(N'2026-10-04T17:12:33.113' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (10, N'Ana Mandara Villas Dalat', N'Lê Lai, Đà Lạt, Lâm Đồng', 2, 4, N'Khu nghỉ dưỡng gồm các biệt thự phong cách Pháp giữa đồi thông.', 5, CAST(2800000.00 AS Decimal(18, 2)), N'/Content/images/hotels/ana-mandara-dalat.jpg', N'Resort', N'02633 555 888', N'reservation@anamandara-resort.com', N'Giờ nhận/trả phòng và chính sách hủy phòng áp dụng theo quy định của từng khách sạn; vui lòng liên hệ khách sạn để biết chi tiết.', 1, CAST(N'2026-10-04T17:12:33.113' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (11, N'Sheraton Nha Trang Hotel & Spa', N'26-28 Trần Phú, Nha Trang, Khánh Hòa', 3, 4, N'Khách sạn đường biển Trần Phú, nhìn ra vịnh Nha Trang.', 5, CAST(2500000.00 AS Decimal(18, 2)), N'/Content/images/hotels/sheraton-nhatrang.jpg', N'Khách sạn', N'+84 258-3882222', NULL, N'Giờ nhận/trả phòng và chính sách hủy phòng áp dụng theo quy định của từng khách sạn; vui lòng liên hệ khách sạn để biết chi tiết.', 1, CAST(N'2026-10-04T17:12:33.113' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (12, N'Havana Nha Trang Hotel', N'38 Trần Phú, Nha Trang, Khánh Hòa', 3, 4, N'Khách sạn cao tầng sát biển trên đường Trần Phú.', 5, CAST(2200000.00 AS Decimal(18, 2)), N'/Content/images/hotels/havana-nhatrang.jpg', N'Khách sạn', N'(+84-4) 35553735', NULL, N'Giờ nhận/trả phòng và chính sách hủy phòng áp dụng theo quy định của từng khách sạn; vui lòng liên hệ khách sạn để biết chi tiết.', 1, CAST(N'2026-10-04T17:12:33.113' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (13, N'JW Marriott Phu Quoc Emerald Bay Resort & Spa', N'Bãi Khem, An Thới, Phú Quốc, An Giang', 4, 4, N'Resort cao cấp tại Bãi Khem, phía nam đảo Phú Quốc.', 5, CAST(8000000.00 AS Decimal(18, 2)), N'/Content/images/hotels/jw-phuquoc.jpg', N'Resort', N'+84-27-73779999', NULL, N'Giờ nhận/trả phòng và chính sách hủy phòng áp dụng theo quy định của từng khách sạn; vui lòng liên hệ khách sạn để biết chi tiết.', 1, CAST(N'2026-10-04T17:12:33.113' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (14, N'InterContinental Phu Quoc Long Beach Resort', N'Bãi Trường, Dương Tơ, Phú Quốc, An Giang', 4, 4, N'Resort ven biển Bãi Trường, phía tây đảo Phú Quốc.', 5, CAST(5000000.00 AS Decimal(18, 2)), N'/Content/images/hotels/ic-phuquoc.jpg', N'Resort', N' +84 297 397 8888', N'Reservations.ICPQ@ihg.com', N'Giờ nhận/trả phòng và chính sách hủy phòng áp dụng theo quy định của từng khách sạn; vui lòng liên hệ khách sạn để biết chi tiết.', 1, CAST(N'2026-10-04T17:12:33.113' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (15, N'Pullman Vung Tau', N'15 Thi Sách, Vũng Tàu', 5, 4, N'Khách sạn cao cấp tại trung tâm thành phố Vũng Tàu.', 5, CAST(2200000.00 AS Decimal(18, 2)), N'/Content/images/hotels/pullman-vungtau.jpg', N'Khách sạn', N' +84 254 355 1777', N'h7133@accor.com', N'Giờ nhận/trả phòng và chính sách hủy phòng áp dụng theo quy định của từng khách sạn; vui lòng liên hệ khách sạn để biết chi tiết.', 1, CAST(N'2026-10-04T17:12:33.113' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (16, N'Imperial Hotel Vung Tau', N'159 Thùy Vân, Vũng Tàu', 5, 4, N'Khách sạn ven biển Bãi Sau trên đường Thùy Vân.', 5, CAST(1800000.00 AS Decimal(18, 2)), N'/Content/images/hotels/imperial-vungtau.jpg', N'Khách sạn', N'+84 254 362 8888', N'info@imperialhotel.vn', N'Giờ nhận/trả phòng và chính sách hủy phòng áp dụng theo quy định của từng khách sạn; vui lòng liên hệ khách sạn để biết chi tiết.', 1, CAST(N'2026-10-04T17:12:33.113' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (17, N'Sofitel Legend Metropole Hanoi', N'15 Ngô Quyền, Hoàn Kiếm, Hà Nội', 6, 4, N'Khách sạn lịch sử phong cách thuộc địa Pháp, gần Nhà hát Lớn và hồ Hoàn Kiếm.', 5, CAST(6000000.00 AS Decimal(18, 2)), N'/Content/images/hotels/metropole-hanoi.jpg', N'Khách sạn', N'(+84) 243 826 6919', N'metropolehanoi.info@sofitel.com', N'Giờ nhận/trả phòng và chính sách hủy phòng áp dụng theo quy định của từng khách sạn; vui lòng liên hệ khách sạn để biết chi tiết.', 1, CAST(N'2026-10-04T17:12:33.113' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (18, N'Lotte Hotel Hanoi', N'54 Liễu Giai, Ba Đình, Hà Nội', 6, 4, N'Khách sạn cao tầng nằm trong tổ hợp Lotte Center Hà Nội, quận Ba Đình.', 5, CAST(3000000.00 AS Decimal(18, 2)), N'/Content/images/hotels/lotte-hanoi.jpg', N'Khách sạn', N'+84-24-3333-1000', N'hotelhanoi@lotte.net', N'Giờ nhận/trả phòng và chính sách hủy phòng áp dụng theo quy định của từng khách sạn; vui lòng liên hệ khách sạn để biết chi tiết.', 1, CAST(N'2026-10-04T17:12:33.113' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (19, N'Hilton Hanoi Opera', N'1 Lê Thánh Tông, Hoàn Kiếm, Hà Nội', 6, 4, N'Khách sạn nằm cạnh Nhà hát Lớn Hà Nội, thuận tiện đi bộ đến khu phố cổ.', 5, CAST(2500000.00 AS Decimal(18, 2)), N'/Content/images/hotels/hilton-opera-hanoi.jpg', N'Khách sạn', NULL, NULL, N'Giờ nhận/trả phòng và chính sách hủy phòng áp dụng theo quy định của từng khách sạn; vui lòng liên hệ khách sạn để biết chi tiết.', 1, CAST(N'2026-10-04T17:12:33.113' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (20, N'Park Hyatt Saigon', N'2 Công xã Paris, Bến Nghé, Quận 1, TP. Hồ Chí Minh', 7, 4, N'Khách sạn cao cấp tại trung tâm Quận 1, gần Nhà hát Thành phố và phố đi bộ Nguyễn Huệ.', 5, CAST(7000000.00 AS Decimal(18, 2)), N'/Content/images/hotels/park-hyatt-saigon.jpg', N'Khách sạn', N'028 3824 1234', NULL, N'Giờ nhận/trả phòng và chính sách hủy phòng áp dụng theo quy định của từng khách sạn; vui lòng liên hệ khách sạn để biết chi tiết.', 1, CAST(N'2026-10-04T17:12:33.113' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (21, N'Hotel Majestic Saigon', N'1 Đồng Khởi, Quận 1, TP. Hồ Chí Minh', 7, 4, N'Khách sạn lịch sử khai trương năm 1925, nằm bên bờ sông Sài Gòn.', 5, CAST(2500000.00 AS Decimal(18, 2)), N'/Content/images/hotels/majestic-saigon.jpg', N'Khách sạn', N'
(84 28) 3829 5517', N'
majestic@majesticsaigon.com', N'Giờ nhận/trả phòng và chính sách hủy phòng áp dụng theo quy định của từng khách sạn; vui lòng liên hệ khách sạn để biết chi tiết.', 1, CAST(N'2026-10-04T17:12:33.113' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (22, N'Rex Hotel Saigon', N'141 Nguyễn Huệ, Quận 1, TP. Hồ Chí Minh', 7, 4, N'Khách sạn nổi tiếng ngay trung tâm Quận 1, gần phố đi bộ Nguyễn Huệ.', 5, CAST(2500000.00 AS Decimal(18, 2)), N'/Content/images/hotels/rex-saigon.jpg', N'Khách sạn', N' 0917 590 900', N' rexhotel@rex.com.vn', N'Giờ nhận/trả phòng và chính sách hủy phòng áp dụng theo quy định của từng khách sạn; vui lòng liên hệ khách sạn để biết chi tiết.', 1, CAST(N'2026-10-04T17:12:33.113' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (23, N'Anantara Hoi An Resort', N'1 Phạm Hồng Thái, Hội An, Đà Nẵng', 8, 4, N'Resort bên sông Thu Bồn, cách khu phố cổ Hội An một quãng đi bộ ngắn.', 5, CAST(3500000.00 AS Decimal(18, 2)), N'/Content/images/hotels/anantara-hoian.jpg', N'Resort', N'+84 235 391 4555', N'hoian@anantara.com', N'Giờ nhận/trả phòng và chính sách hủy phòng áp dụng theo quy định của từng khách sạn; vui lòng liên hệ khách sạn để biết chi tiết.', 1, CAST(N'2026-10-04T17:12:33.113' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (24, N'Hotel Royal Hoi An - MGallery', N'39 Đào Duy Từ, Hội An, Đà Nẵng', 8, 4, N'Khách sạn ven sông Thu Bồn, gần khu phố cổ Hội An.', 5, CAST(3000000.00 AS Decimal(18, 2)), N'/Content/images/hotels/royal-hoian.jpg', N'Khách sạn', N'84 (0) 235 3950 777', N'info@hotelroyalhoian.vn', N'Giờ nhận/trả phòng và chính sách hủy phòng áp dụng theo quy định của từng khách sạn; vui lòng liên hệ khách sạn để biết chi tiết.', 1, CAST(N'2026-10-04T17:12:33.113' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (25, N'La Residence Hue Hotel & Spa', N'5 Lê Lợi, Huế', 9, 4, N'Khách sạn phong cách Art Deco bên bờ sông Hương.', 5, CAST(3500000.00 AS Decimal(18, 2)), N'/Content/images/hotels/la-residence-hue.jpg', N'Khách sạn', N'
+84 234 3837 475', N'reservations.laresidence.hue@azerai.com', N'Giờ nhận/trả phòng và chính sách hủy phòng áp dụng theo quy định của từng khách sạn; vui lòng liên hệ khách sạn để biết chi tiết.', 1, CAST(N'2026-10-04T17:12:33.113' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (26, N'Imperial Hotel Hue', N'8 Hùng Vương, Huế', 9, 4, N'Khách sạn trung tâm thành phố Huế, gần sông Hương.', 5, CAST(1800000.00 AS Decimal(18, 2)), N'/Content/images/hotels/imperial-hue.jpg', N'Khách sạn', N'0234 3882 222', N'info.hue@ttchotels.com', N'Giờ nhận/trả phòng và chính sách hủy phòng áp dụng theo quy định của từng khách sạn; vui lòng liên hệ khách sạn để biết chi tiết.', 1, CAST(N'2026-10-04T17:12:33.113' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (27, N'Hotel de la Coupole - MGallery', N'1 Hoàng Diệu, Sa Pa, Lào Cai', 10, 4, N'Khách sạn lấy cảm hứng từ phong cách Pháp, nằm ở trung tâm thị trấn Sa Pa.', 5, CAST(3500000.00 AS Decimal(18, 2)), N'/Content/images/hotels/coupole-sapa.jpg', N'Khách sạn', N' (+84)2143629999', N'ha5v2-re@accor.com', N'Giờ nhận/trả phòng và chính sách hủy phòng áp dụng theo quy định của từng khách sạn; vui lòng liên hệ khách sạn để biết chi tiết.', 1, CAST(N'2026-10-04T17:12:33.113' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (28, N'Vinpearl Resort & Spa Ha Long', N'Đảo Rều, Bãi Cháy, Hạ Long, Quảng Ninh', 11, 4, N'Resort trên đảo Rều, hướng ra vịnh Hạ Long.', 5, CAST(2500000.00 AS Decimal(18, 2)), N'/Content/images/hotels/vinpearl-halong.jpg', N'Resort', N' 1900 23 23 89', N'callcenter@vinpearl.com', N'Giờ nhận/trả phòng và chính sách hủy phòng áp dụng theo quy định của từng khách sạn; vui lòng liên hệ khách sạn để biết chi tiết.', 1, CAST(N'2026-10-04T17:12:33.113' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (29, N'Wyndham Legend Halong Hotel', N'12 Hạ Long, Bãi Cháy, Hạ Long, Quảng Ninh', 11, 4, N'Khách sạn khu Bãi Cháy, thuận tiện di chuyển ra bến tàu tham quan vịnh.', 5, CAST(2200000.00 AS Decimal(18, 2)), N'/Content/images/hotels/wyndham-halong.jpg', N'Khách sạn', N'+84-203-3636 555', N'info@wyndhamhalong.com

', N'Giờ nhận/trả phòng và chính sách hủy phòng áp dụng theo quy định của từng khách sạn; vui lòng liên hệ khách sạn để biết chi tiết.', 1, CAST(N'2026-10-04T17:12:33.113' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (30, N'Emeralda Resort Ninh Binh', N'Văn Lâm, Hoa Lư, Ninh Bình', 12, 4, N'Resort giữa cảnh quan núi đá vôi, gần khu danh thắng Tam Cốc.', 4, CAST(2200000.00 AS Decimal(18, 2)), N'/Content/images/hotels/emeralda-ninhbinh.jpg', N'Resort', N'+84 2293 658 333u', N'info@emeraldaresort.com', N'Giờ nhận/trả phòng và chính sách hủy phòng áp dụng theo quy định của từng khách sạn; vui lòng liên hệ khách sạn để biết chi tiết.', 1, CAST(N'2026-10-04T17:12:33.113' AS DateTime))
INSERT [dbo].[KhachSan] ([MaKS], [TenKS], [DiaChi], [MaDiaDiem], [MaTaiKhoanDoiTac], [MoTa], [SoSao], [GiaTu], [HinhAnhDaiDien], [LoaiHinh], [SoDienThoai], [Email], [ChinhSach], [TrangThai], [NgayTao]) VALUES (31, N'Mua Caves Ecolodge', N'Hang Múa, Ninh Xuân, Hoa Lư, Ninh Bình', 12, 4, N'Khu lưu trú sinh thái dạng bungalow gần khu du lịch Hang Múa.', 3, CAST(1800000.00 AS Decimal(18, 2)), N'/Content/images/hotels/muacaves-ninhbinh.jpg', N'Ecolodge', N' 0229 361 9679', NULL, N'Giờ nhận/trả phòng và chính sách hủy phòng áp dụng theo quy định của từng khách sạn; vui lòng liên hệ khách sạn để biết chi tiết.', 1, CAST(N'2026-10-04T17:12:33.113' AS DateTime))
SET IDENTITY_INSERT [dbo].[KhachSan] OFF
GO
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (1, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (1, 2)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (1, 3)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (1, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (1, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (1, 8)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (2, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (2, 3)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (2, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (2, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (3, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (3, 2)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (3, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (3, 5)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (3, 6)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (3, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (4, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (4, 2)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (4, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (4, 5)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (4, 6)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (4, 8)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (5, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (5, 3)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (5, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (5, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (7, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (7, 2)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (7, 3)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (7, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (7, 5)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (7, 6)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (7, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (7, 9)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (7, 10)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (7, 11)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (7, 12)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (8, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (8, 2)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (8, 3)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (8, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (8, 5)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (8, 6)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (8, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (8, 9)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (8, 10)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (8, 11)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (8, 12)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (9, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (9, 2)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (9, 3)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (9, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (9, 5)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (9, 6)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (9, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (9, 10)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (9, 11)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (10, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (10, 2)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (10, 3)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (10, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (10, 5)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (10, 6)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (10, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (10, 10)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (10, 11)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (11, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (11, 2)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (11, 3)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (11, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (11, 5)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (11, 6)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (11, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (11, 9)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (11, 10)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (11, 11)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (12, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (12, 2)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (12, 3)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (12, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (12, 5)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (12, 6)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (12, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (12, 9)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (12, 10)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (12, 11)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (13, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (13, 2)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (13, 3)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (13, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (13, 5)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (13, 6)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (13, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (13, 9)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (13, 10)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (13, 11)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (13, 12)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (14, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (14, 2)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (14, 3)
GO
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (14, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (14, 5)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (14, 6)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (14, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (14, 9)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (14, 10)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (14, 11)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (14, 12)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (15, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (15, 2)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (15, 3)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (15, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (15, 5)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (15, 6)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (15, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (15, 10)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (15, 11)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (16, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (16, 2)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (16, 3)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (16, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (16, 5)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (16, 6)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (16, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (16, 10)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (16, 11)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (17, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (17, 2)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (17, 3)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (17, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (17, 5)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (17, 6)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (17, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (17, 10)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (17, 11)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (18, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (18, 2)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (18, 3)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (18, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (18, 5)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (18, 6)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (18, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (18, 10)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (18, 11)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (19, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (19, 2)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (19, 3)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (19, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (19, 5)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (19, 6)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (19, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (19, 10)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (19, 11)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (20, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (20, 2)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (20, 3)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (20, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (20, 5)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (20, 6)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (20, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (20, 10)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (20, 11)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (21, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (21, 2)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (21, 3)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (21, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (21, 5)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (21, 6)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (21, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (21, 10)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (21, 11)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (22, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (22, 2)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (22, 3)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (22, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (22, 5)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (22, 6)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (22, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (22, 10)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (22, 11)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (23, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (23, 2)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (23, 3)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (23, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (23, 5)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (23, 6)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (23, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (23, 10)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (23, 11)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (24, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (24, 2)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (24, 3)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (24, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (24, 5)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (24, 6)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (24, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (24, 10)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (24, 11)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (25, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (25, 2)
GO
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (25, 3)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (25, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (25, 5)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (25, 6)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (25, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (25, 10)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (25, 11)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (26, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (26, 2)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (26, 3)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (26, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (26, 5)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (26, 6)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (26, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (26, 10)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (26, 11)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (27, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (27, 2)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (27, 3)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (27, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (27, 5)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (27, 6)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (27, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (27, 10)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (27, 11)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (28, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (28, 2)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (28, 3)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (28, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (28, 5)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (28, 6)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (28, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (28, 10)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (28, 11)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (28, 12)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (29, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (29, 2)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (29, 3)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (29, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (29, 5)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (29, 6)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (29, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (29, 10)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (29, 11)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (30, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (30, 2)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (30, 3)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (30, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (30, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (30, 11)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (31, 1)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (31, 2)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (31, 3)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (31, 4)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (31, 7)
INSERT [dbo].[KhachSan_TienIch] ([MaKS], [MaTienIch]) VALUES (31, 11)
GO
SET IDENTITY_INSERT [dbo].[KhuyenMai] ON 

INSERT [dbo].[KhuyenMai] ([MaKM], [TenKM], [MaCode], [PhanTramGiam], [SoTienGiam], [GiaTriToiThieu], [NgayBatDau], [NgayKetThuc], [SoLuong], [MoTa], [TrangThai]) VALUES (1, N'Chào mừng thành viên mới', N'WELCOME10', CAST(10.00 AS Decimal(5, 2)), NULL, CAST(500000.00 AS Decimal(18, 2)), CAST(N'2026-01-01T00:00:00.000' AS DateTime), CAST(N'2026-12-31T00:00:00.000' AS DateTime), 1000, N'Giảm 10% cho thành viên mới.', 1)
INSERT [dbo].[KhuyenMai] ([MaKM], [TenKM], [MaCode], [PhanTramGiam], [SoTienGiam], [GiaTriToiThieu], [NgayBatDau], [NgayKetThuc], [SoLuong], [MoTa], [TrangThai]) VALUES (2, N'Ưu đãi Travel50', N'TRAVEL50', NULL, CAST(50000.00 AS Decimal(18, 2)), CAST(500000.00 AS Decimal(18, 2)), CAST(N'2026-01-01T00:00:00.000' AS DateTime), CAST(N'2026-12-31T00:00:00.000' AS DateTime), 500, N'Giảm trực tiếp 50.000 VNĐ.', 1)
INSERT [dbo].[KhuyenMai] ([MaKM], [TenKM], [MaCode], [PhanTramGiam], [SoTienGiam], [GiaTriToiThieu], [NgayBatDau], [NgayKetThuc], [SoLuong], [MoTa], [TrangThai]) VALUES (3, N'Ưu đãi mùa hè', N'SUMMER20', CAST(20.00 AS Decimal(5, 2)), NULL, CAST(1000000.00 AS Decimal(18, 2)), CAST(N'2026-04-01T00:00:00.000' AS DateTime), CAST(N'2026-09-30T00:00:00.000' AS DateTime), 300, N'Giảm 20% cho kỳ nghỉ mùa hè.', 1)
SET IDENTITY_INSERT [dbo].[KhuyenMai] OFF
GO
SET IDENTITY_INSERT [dbo].[Phong] ON 

INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (1, 1, N'Deluxe Double', N'Phòng đôi', 2, 1, CAST(28.00 AS Decimal(10, 2)), CAST(900000.00 AS Decimal(18, 2)), 10, N'Phòng đôi hiện đại, view thành phố.', N'/Content/images/rooms/deluxe-double.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (2, 1, N'Deluxe Twin', N'Phòng 2 giường đơn', 2, 2, CAST(30.00 AS Decimal(10, 2)), CAST(1000000.00 AS Decimal(18, 2)), 8, N'Phòng 2 giường đơn phù hợp cho bạn bè hoặc gia đình.', N'/Content/images/rooms/deluxe-twin.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (3, 1, N'Family Room', N'Phòng gia đình', 4, 2, CAST(40.00 AS Decimal(10, 2)), CAST(1500000.00 AS Decimal(18, 2)), 5, N'Phòng rộng dành cho gia đình.', N'/Content/images/rooms/family-room.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (4, 2, N'Standard Room', N'Phòng tiêu chuẩn', 2, 1, CAST(25.00 AS Decimal(10, 2)), CAST(800000.00 AS Decimal(18, 2)), 10, N'Phòng tiêu chuẩn tiện nghi.', N'/Content/images/rooms/standard-room.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (5, 2, N'Garden View', N'Phòng view vườn', 2, 1, CAST(30.00 AS Decimal(10, 2)), CAST(1000000.00 AS Decimal(18, 2)), 6, N'Phòng có ban công và view vườn.', N'/Content/images/rooms/garden-view.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (6, 3, N'Deluxe Sea View', N'Phòng view biển', 2, 1, CAST(35.00 AS Decimal(10, 2)), CAST(1500000.00 AS Decimal(18, 2)), 8, N'Phòng cao cấp với view biển.', N'/Content/images/rooms/deluxe-sea-view.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (7, 4, N'Ocean View Room', N'Phòng view đại dương', 2, 1, CAST(38.00 AS Decimal(10, 2)), CAST(1800000.00 AS Decimal(18, 2)), 6, N'Phòng cao cấp hướng biển.', N'/Content/images/rooms/ocean-view.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (8, 5, N'Standard Double', N'Phòng đôi', 2, 1, CAST(25.00 AS Decimal(10, 2)), CAST(650000.00 AS Decimal(18, 2)), 12, N'Phòng đôi tiêu chuẩn.', N'/Content/images/rooms/standard-double.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (9, 7, N'Deluxe Double', N'Phòng đôi', 2, 1, CAST(30.00 AS Decimal(10, 2)), CAST(10000000.00 AS Decimal(18, 2)), 10, N'Phòng đôi tiện nghi, phù hợp cặp đôi hoặc khách đi một mình.', N'/Content/images/rooms/deluxe-double.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (10, 7, N'Deluxe Twin', N'Phòng 2 giường đơn', 2, 2, CAST(30.00 AS Decimal(10, 2)), CAST(10000000.00 AS Decimal(18, 2)), 10, N'Phòng 2 giường đơn, phù hợp bạn bè hoặc đồng nghiệp.', N'/Content/images/rooms/deluxe-twin.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (11, 7, N'Family Suite', N'Phòng gia đình', 4, 2, CAST(55.00 AS Decimal(10, 2)), CAST(18000000.00 AS Decimal(18, 2)), 4, N'Phòng rộng cho gia đình, tối đa 4 khách.', N'/Content/images/rooms/family-suite.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (12, 8, N'Deluxe Double', N'Phòng đôi', 2, 1, CAST(30.00 AS Decimal(10, 2)), CAST(3500000.00 AS Decimal(18, 2)), 10, N'Phòng đôi tiện nghi, phù hợp cặp đôi hoặc khách đi một mình.', N'/Content/images/rooms/deluxe-double.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (13, 8, N'Deluxe Twin', N'Phòng 2 giường đơn', 2, 2, CAST(30.00 AS Decimal(10, 2)), CAST(3500000.00 AS Decimal(18, 2)), 10, N'Phòng 2 giường đơn, phù hợp bạn bè hoặc đồng nghiệp.', N'/Content/images/rooms/deluxe-twin.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (14, 8, N'Family Suite', N'Phòng gia đình', 4, 2, CAST(55.00 AS Decimal(10, 2)), CAST(6300000.00 AS Decimal(18, 2)), 4, N'Phòng rộng cho gia đình, tối đa 4 khách.', N'/Content/images/rooms/family-suite.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (15, 9, N'Deluxe Double', N'Phòng đôi', 2, 1, CAST(30.00 AS Decimal(10, 2)), CAST(3500000.00 AS Decimal(18, 2)), 10, N'Phòng đôi tiện nghi, phù hợp cặp đôi hoặc khách đi một mình.', N'/Content/images/rooms/deluxe-double.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (16, 9, N'Deluxe Twin', N'Phòng 2 giường đơn', 2, 2, CAST(30.00 AS Decimal(10, 2)), CAST(3500000.00 AS Decimal(18, 2)), 10, N'Phòng 2 giường đơn, phù hợp bạn bè hoặc đồng nghiệp.', N'/Content/images/rooms/deluxe-twin.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (17, 9, N'Family Suite', N'Phòng gia đình', 4, 2, CAST(55.00 AS Decimal(10, 2)), CAST(6300000.00 AS Decimal(18, 2)), 4, N'Phòng rộng cho gia đình, tối đa 4 khách.', N'/Content/images/rooms/family-suite.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (18, 10, N'Deluxe Double', N'Phòng đôi', 2, 1, CAST(30.00 AS Decimal(10, 2)), CAST(2800000.00 AS Decimal(18, 2)), 10, N'Phòng đôi tiện nghi, phù hợp cặp đôi hoặc khách đi một mình.', N'/Content/images/rooms/deluxe-double.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (19, 10, N'Deluxe Twin', N'Phòng 2 giường đơn', 2, 2, CAST(30.00 AS Decimal(10, 2)), CAST(2800000.00 AS Decimal(18, 2)), 10, N'Phòng 2 giường đơn, phù hợp bạn bè hoặc đồng nghiệp.', N'/Content/images/rooms/deluxe-twin.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (20, 10, N'Family Suite', N'Phòng gia đình', 4, 2, CAST(55.00 AS Decimal(10, 2)), CAST(5040000.00 AS Decimal(18, 2)), 4, N'Phòng rộng cho gia đình, tối đa 4 khách.', N'/Content/images/rooms/family-suite.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (21, 11, N'Deluxe Double', N'Phòng đôi', 2, 1, CAST(30.00 AS Decimal(10, 2)), CAST(2500000.00 AS Decimal(18, 2)), 10, N'Phòng đôi tiện nghi, phù hợp cặp đôi hoặc khách đi một mình.', N'/Content/images/rooms/deluxe-double.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (22, 11, N'Deluxe Twin', N'Phòng 2 giường đơn', 2, 2, CAST(30.00 AS Decimal(10, 2)), CAST(2500000.00 AS Decimal(18, 2)), 10, N'Phòng 2 giường đơn, phù hợp bạn bè hoặc đồng nghiệp.', N'/Content/images/rooms/deluxe-twin.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (23, 11, N'Family Suite', N'Phòng gia đình', 4, 2, CAST(55.00 AS Decimal(10, 2)), CAST(4500000.00 AS Decimal(18, 2)), 4, N'Phòng rộng cho gia đình, tối đa 4 khách.', N'/Content/images/rooms/family-suite.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (24, 12, N'Deluxe Double', N'Phòng đôi', 2, 1, CAST(30.00 AS Decimal(10, 2)), CAST(2200000.00 AS Decimal(18, 2)), 10, N'Phòng đôi tiện nghi, phù hợp cặp đôi hoặc khách đi một mình.', N'/Content/images/rooms/deluxe-double.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (25, 12, N'Deluxe Twin', N'Phòng 2 giường đơn', 2, 2, CAST(30.00 AS Decimal(10, 2)), CAST(2200000.00 AS Decimal(18, 2)), 10, N'Phòng 2 giường đơn, phù hợp bạn bè hoặc đồng nghiệp.', N'/Content/images/rooms/deluxe-twin.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (26, 12, N'Family Suite', N'Phòng gia đình', 4, 2, CAST(55.00 AS Decimal(10, 2)), CAST(3960000.00 AS Decimal(18, 2)), 4, N'Phòng rộng cho gia đình, tối đa 4 khách.', N'/Content/images/rooms/family-suite.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (27, 13, N'Deluxe Double', N'Phòng đôi', 2, 1, CAST(30.00 AS Decimal(10, 2)), CAST(8000000.00 AS Decimal(18, 2)), 10, N'Phòng đôi tiện nghi, phù hợp cặp đôi hoặc khách đi một mình.', N'/Content/images/rooms/deluxe-double.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (28, 13, N'Deluxe Twin', N'Phòng 2 giường đơn', 2, 2, CAST(30.00 AS Decimal(10, 2)), CAST(8000000.00 AS Decimal(18, 2)), 10, N'Phòng 2 giường đơn, phù hợp bạn bè hoặc đồng nghiệp.', N'/Content/images/rooms/deluxe-twin.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (29, 13, N'Family Suite', N'Phòng gia đình', 4, 2, CAST(55.00 AS Decimal(10, 2)), CAST(14400000.00 AS Decimal(18, 2)), 4, N'Phòng rộng cho gia đình, tối đa 4 khách.', N'/Content/images/rooms/family-suite.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (30, 14, N'Deluxe Double', N'Phòng đôi', 2, 1, CAST(30.00 AS Decimal(10, 2)), CAST(5000000.00 AS Decimal(18, 2)), 10, N'Phòng đôi tiện nghi, phù hợp cặp đôi hoặc khách đi một mình.', N'/Content/images/rooms/deluxe-double.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (31, 14, N'Deluxe Twin', N'Phòng 2 giường đơn', 2, 2, CAST(30.00 AS Decimal(10, 2)), CAST(5000000.00 AS Decimal(18, 2)), 10, N'Phòng 2 giường đơn, phù hợp bạn bè hoặc đồng nghiệp.', N'/Content/images/rooms/deluxe-twin.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (32, 14, N'Family Suite', N'Phòng gia đình', 4, 2, CAST(55.00 AS Decimal(10, 2)), CAST(9000000.00 AS Decimal(18, 2)), 4, N'Phòng rộng cho gia đình, tối đa 4 khách.', N'/Content/images/rooms/family-suite.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (33, 15, N'Deluxe Double', N'Phòng đôi', 2, 1, CAST(30.00 AS Decimal(10, 2)), CAST(2200000.00 AS Decimal(18, 2)), 10, N'Phòng đôi tiện nghi, phù hợp cặp đôi hoặc khách đi một mình.', N'/Content/images/rooms/deluxe-double.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (34, 15, N'Deluxe Twin', N'Phòng 2 giường đơn', 2, 2, CAST(30.00 AS Decimal(10, 2)), CAST(2200000.00 AS Decimal(18, 2)), 10, N'Phòng 2 giường đơn, phù hợp bạn bè hoặc đồng nghiệp.', N'/Content/images/rooms/deluxe-twin.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (35, 15, N'Family Suite', N'Phòng gia đình', 4, 2, CAST(55.00 AS Decimal(10, 2)), CAST(3960000.00 AS Decimal(18, 2)), 4, N'Phòng rộng cho gia đình, tối đa 4 khách.', N'/Content/images/rooms/family-suite.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (36, 16, N'Deluxe Double', N'Phòng đôi', 2, 1, CAST(30.00 AS Decimal(10, 2)), CAST(1800000.00 AS Decimal(18, 2)), 10, N'Phòng đôi tiện nghi, phù hợp cặp đôi hoặc khách đi một mình.', N'/Content/images/rooms/deluxe-double.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (37, 16, N'Deluxe Twin', N'Phòng 2 giường đơn', 2, 2, CAST(30.00 AS Decimal(10, 2)), CAST(1800000.00 AS Decimal(18, 2)), 10, N'Phòng 2 giường đơn, phù hợp bạn bè hoặc đồng nghiệp.', N'/Content/images/rooms/deluxe-twin.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (38, 16, N'Family Suite', N'Phòng gia đình', 4, 2, CAST(55.00 AS Decimal(10, 2)), CAST(3240000.00 AS Decimal(18, 2)), 4, N'Phòng rộng cho gia đình, tối đa 4 khách.', N'/Content/images/rooms/family-suite.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (39, 17, N'Deluxe Double', N'Phòng đôi', 2, 1, CAST(30.00 AS Decimal(10, 2)), CAST(6000000.00 AS Decimal(18, 2)), 10, N'Phòng đôi tiện nghi, phù hợp cặp đôi hoặc khách đi một mình.', N'/Content/images/rooms/deluxe-double.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (40, 17, N'Deluxe Twin', N'Phòng 2 giường đơn', 2, 2, CAST(30.00 AS Decimal(10, 2)), CAST(6000000.00 AS Decimal(18, 2)), 10, N'Phòng 2 giường đơn, phù hợp bạn bè hoặc đồng nghiệp.', N'/Content/images/rooms/deluxe-twin.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (41, 17, N'Family Suite', N'Phòng gia đình', 4, 2, CAST(55.00 AS Decimal(10, 2)), CAST(10800000.00 AS Decimal(18, 2)), 4, N'Phòng rộng cho gia đình, tối đa 4 khách.', N'/Content/images/rooms/family-suite.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (42, 18, N'Deluxe Double', N'Phòng đôi', 2, 1, CAST(30.00 AS Decimal(10, 2)), CAST(3000000.00 AS Decimal(18, 2)), 10, N'Phòng đôi tiện nghi, phù hợp cặp đôi hoặc khách đi một mình.', N'/Content/images/rooms/deluxe-double.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (43, 18, N'Deluxe Twin', N'Phòng 2 giường đơn', 2, 2, CAST(30.00 AS Decimal(10, 2)), CAST(3000000.00 AS Decimal(18, 2)), 10, N'Phòng 2 giường đơn, phù hợp bạn bè hoặc đồng nghiệp.', N'/Content/images/rooms/deluxe-twin.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (44, 18, N'Family Suite', N'Phòng gia đình', 4, 2, CAST(55.00 AS Decimal(10, 2)), CAST(5400000.00 AS Decimal(18, 2)), 4, N'Phòng rộng cho gia đình, tối đa 4 khách.', N'/Content/images/rooms/family-suite.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (45, 19, N'Deluxe Double', N'Phòng đôi', 2, 1, CAST(30.00 AS Decimal(10, 2)), CAST(2500000.00 AS Decimal(18, 2)), 10, N'Phòng đôi tiện nghi, phù hợp cặp đôi hoặc khách đi một mình.', N'/Content/images/rooms/deluxe-double.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (46, 19, N'Deluxe Twin', N'Phòng 2 giường đơn', 2, 2, CAST(30.00 AS Decimal(10, 2)), CAST(2500000.00 AS Decimal(18, 2)), 10, N'Phòng 2 giường đơn, phù hợp bạn bè hoặc đồng nghiệp.', N'/Content/images/rooms/deluxe-twin.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (47, 19, N'Family Suite', N'Phòng gia đình', 4, 2, CAST(55.00 AS Decimal(10, 2)), CAST(4500000.00 AS Decimal(18, 2)), 4, N'Phòng rộng cho gia đình, tối đa 4 khách.', N'/Content/images/rooms/family-suite.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (48, 20, N'Deluxe Double', N'Phòng đôi', 2, 1, CAST(30.00 AS Decimal(10, 2)), CAST(7000000.00 AS Decimal(18, 2)), 10, N'Phòng đôi tiện nghi, phù hợp cặp đôi hoặc khách đi một mình.', N'/Content/images/rooms/deluxe-double.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (49, 20, N'Deluxe Twin', N'Phòng 2 giường đơn', 2, 2, CAST(30.00 AS Decimal(10, 2)), CAST(7000000.00 AS Decimal(18, 2)), 10, N'Phòng 2 giường đơn, phù hợp bạn bè hoặc đồng nghiệp.', N'/Content/images/rooms/deluxe-twin.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (50, 20, N'Family Suite', N'Phòng gia đình', 4, 2, CAST(55.00 AS Decimal(10, 2)), CAST(12600000.00 AS Decimal(18, 2)), 4, N'Phòng rộng cho gia đình, tối đa 4 khách.', N'/Content/images/rooms/family-suite.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (51, 21, N'Deluxe Double', N'Phòng đôi', 2, 1, CAST(30.00 AS Decimal(10, 2)), CAST(2500000.00 AS Decimal(18, 2)), 10, N'Phòng đôi tiện nghi, phù hợp cặp đôi hoặc khách đi một mình.', N'/Content/images/rooms/deluxe-double.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (52, 21, N'Deluxe Twin', N'Phòng 2 giường đơn', 2, 2, CAST(30.00 AS Decimal(10, 2)), CAST(2500000.00 AS Decimal(18, 2)), 10, N'Phòng 2 giường đơn, phù hợp bạn bè hoặc đồng nghiệp.', N'/Content/images/rooms/deluxe-twin.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (53, 21, N'Family Suite', N'Phòng gia đình', 4, 2, CAST(55.00 AS Decimal(10, 2)), CAST(4500000.00 AS Decimal(18, 2)), 4, N'Phòng rộng cho gia đình, tối đa 4 khách.', N'/Content/images/rooms/family-suite.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (54, 22, N'Deluxe Double', N'Phòng đôi', 2, 1, CAST(30.00 AS Decimal(10, 2)), CAST(2500000.00 AS Decimal(18, 2)), 10, N'Phòng đôi tiện nghi, phù hợp cặp đôi hoặc khách đi một mình.', N'/Content/images/rooms/deluxe-double.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (55, 22, N'Deluxe Twin', N'Phòng 2 giường đơn', 2, 2, CAST(30.00 AS Decimal(10, 2)), CAST(2500000.00 AS Decimal(18, 2)), 10, N'Phòng 2 giường đơn, phù hợp bạn bè hoặc đồng nghiệp.', N'/Content/images/rooms/deluxe-twin.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (56, 22, N'Family Suite', N'Phòng gia đình', 4, 2, CAST(55.00 AS Decimal(10, 2)), CAST(4500000.00 AS Decimal(18, 2)), 4, N'Phòng rộng cho gia đình, tối đa 4 khách.', N'/Content/images/rooms/family-suite.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (57, 23, N'Deluxe Double', N'Phòng đôi', 2, 1, CAST(30.00 AS Decimal(10, 2)), CAST(3500000.00 AS Decimal(18, 2)), 10, N'Phòng đôi tiện nghi, phù hợp cặp đôi hoặc khách đi một mình.', N'/Content/images/rooms/deluxe-double.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (58, 23, N'Deluxe Twin', N'Phòng 2 giường đơn', 2, 2, CAST(30.00 AS Decimal(10, 2)), CAST(3500000.00 AS Decimal(18, 2)), 10, N'Phòng 2 giường đơn, phù hợp bạn bè hoặc đồng nghiệp.', N'/Content/images/rooms/deluxe-twin.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (59, 23, N'Family Suite', N'Phòng gia đình', 4, 2, CAST(55.00 AS Decimal(10, 2)), CAST(6300000.00 AS Decimal(18, 2)), 4, N'Phòng rộng cho gia đình, tối đa 4 khách.', N'/Content/images/rooms/family-suite.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (60, 24, N'Deluxe Double', N'Phòng đôi', 2, 1, CAST(30.00 AS Decimal(10, 2)), CAST(3000000.00 AS Decimal(18, 2)), 10, N'Phòng đôi tiện nghi, phù hợp cặp đôi hoặc khách đi một mình.', N'/Content/images/rooms/deluxe-double.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (61, 24, N'Deluxe Twin', N'Phòng 2 giường đơn', 2, 2, CAST(30.00 AS Decimal(10, 2)), CAST(3000000.00 AS Decimal(18, 2)), 10, N'Phòng 2 giường đơn, phù hợp bạn bè hoặc đồng nghiệp.', N'/Content/images/rooms/deluxe-twin.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (62, 24, N'Family Suite', N'Phòng gia đình', 4, 2, CAST(55.00 AS Decimal(10, 2)), CAST(5400000.00 AS Decimal(18, 2)), 4, N'Phòng rộng cho gia đình, tối đa 4 khách.', N'/Content/images/rooms/family-suite.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (63, 25, N'Deluxe Double', N'Phòng đôi', 2, 1, CAST(30.00 AS Decimal(10, 2)), CAST(3500000.00 AS Decimal(18, 2)), 10, N'Phòng đôi tiện nghi, phù hợp cặp đôi hoặc khách đi một mình.', N'/Content/images/rooms/deluxe-double.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (64, 25, N'Deluxe Twin', N'Phòng 2 giường đơn', 2, 2, CAST(30.00 AS Decimal(10, 2)), CAST(3500000.00 AS Decimal(18, 2)), 10, N'Phòng 2 giường đơn, phù hợp bạn bè hoặc đồng nghiệp.', N'/Content/images/rooms/deluxe-twin.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (65, 25, N'Family Suite', N'Phòng gia đình', 4, 2, CAST(55.00 AS Decimal(10, 2)), CAST(6300000.00 AS Decimal(18, 2)), 4, N'Phòng rộng cho gia đình, tối đa 4 khách.', N'/Content/images/rooms/family-suite.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (66, 26, N'Deluxe Double', N'Phòng đôi', 2, 1, CAST(30.00 AS Decimal(10, 2)), CAST(1800000.00 AS Decimal(18, 2)), 10, N'Phòng đôi tiện nghi, phù hợp cặp đôi hoặc khách đi một mình.', N'/Content/images/rooms/deluxe-double.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (67, 26, N'Deluxe Twin', N'Phòng 2 giường đơn', 2, 2, CAST(30.00 AS Decimal(10, 2)), CAST(1800000.00 AS Decimal(18, 2)), 10, N'Phòng 2 giường đơn, phù hợp bạn bè hoặc đồng nghiệp.', N'/Content/images/rooms/deluxe-twin.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (68, 26, N'Family Suite', N'Phòng gia đình', 4, 2, CAST(55.00 AS Decimal(10, 2)), CAST(3240000.00 AS Decimal(18, 2)), 4, N'Phòng rộng cho gia đình, tối đa 4 khách.', N'/Content/images/rooms/family-suite.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (69, 27, N'Deluxe Double', N'Phòng đôi', 2, 1, CAST(30.00 AS Decimal(10, 2)), CAST(3500000.00 AS Decimal(18, 2)), 10, N'Phòng đôi tiện nghi, phù hợp cặp đôi hoặc khách đi một mình.', N'/Content/images/rooms/deluxe-double.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (70, 27, N'Deluxe Twin', N'Phòng 2 giường đơn', 2, 2, CAST(30.00 AS Decimal(10, 2)), CAST(3500000.00 AS Decimal(18, 2)), 10, N'Phòng 2 giường đơn, phù hợp bạn bè hoặc đồng nghiệp.', N'/Content/images/rooms/deluxe-twin.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (71, 27, N'Family Suite', N'Phòng gia đình', 4, 2, CAST(55.00 AS Decimal(10, 2)), CAST(6300000.00 AS Decimal(18, 2)), 4, N'Phòng rộng cho gia đình, tối đa 4 khách.', N'/Content/images/rooms/family-suite.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (72, 28, N'Deluxe Double', N'Phòng đôi', 2, 1, CAST(30.00 AS Decimal(10, 2)), CAST(2500000.00 AS Decimal(18, 2)), 10, N'Phòng đôi tiện nghi, phù hợp cặp đôi hoặc khách đi một mình.', N'/Content/images/rooms/deluxe-double.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (73, 28, N'Deluxe Twin', N'Phòng 2 giường đơn', 2, 2, CAST(30.00 AS Decimal(10, 2)), CAST(2500000.00 AS Decimal(18, 2)), 10, N'Phòng 2 giường đơn, phù hợp bạn bè hoặc đồng nghiệp.', N'/Content/images/rooms/deluxe-twin.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (74, 28, N'Family Suite', N'Phòng gia đình', 4, 2, CAST(55.00 AS Decimal(10, 2)), CAST(4500000.00 AS Decimal(18, 2)), 4, N'Phòng rộng cho gia đình, tối đa 4 khách.', N'/Content/images/rooms/family-suite.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (75, 29, N'Deluxe Double', N'Phòng đôi', 2, 1, CAST(30.00 AS Decimal(10, 2)), CAST(2200000.00 AS Decimal(18, 2)), 10, N'Phòng đôi tiện nghi, phù hợp cặp đôi hoặc khách đi một mình.', N'/Content/images/rooms/deluxe-double.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (76, 29, N'Deluxe Twin', N'Phòng 2 giường đơn', 2, 2, CAST(30.00 AS Decimal(10, 2)), CAST(2200000.00 AS Decimal(18, 2)), 10, N'Phòng 2 giường đơn, phù hợp bạn bè hoặc đồng nghiệp.', N'/Content/images/rooms/deluxe-twin.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (77, 29, N'Family Suite', N'Phòng gia đình', 4, 2, CAST(55.00 AS Decimal(10, 2)), CAST(3960000.00 AS Decimal(18, 2)), 4, N'Phòng rộng cho gia đình, tối đa 4 khách.', N'/Content/images/rooms/family-suite.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (78, 30, N'Deluxe Double', N'Phòng đôi', 2, 1, CAST(30.00 AS Decimal(10, 2)), CAST(2200000.00 AS Decimal(18, 2)), 10, N'Phòng đôi tiện nghi, phù hợp cặp đôi hoặc khách đi một mình.', N'/Content/images/rooms/deluxe-double.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (79, 30, N'Deluxe Twin', N'Phòng 2 giường đơn', 2, 2, CAST(30.00 AS Decimal(10, 2)), CAST(2200000.00 AS Decimal(18, 2)), 10, N'Phòng 2 giường đơn, phù hợp bạn bè hoặc đồng nghiệp.', N'/Content/images/rooms/deluxe-twin.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (80, 30, N'Family Suite', N'Phòng gia đình', 4, 2, CAST(55.00 AS Decimal(10, 2)), CAST(3960000.00 AS Decimal(18, 2)), 4, N'Phòng rộng cho gia đình, tối đa 4 khách.', N'/Content/images/rooms/family-suite.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (81, 31, N'Deluxe Double', N'Phòng đôi', 2, 1, CAST(30.00 AS Decimal(10, 2)), CAST(1800000.00 AS Decimal(18, 2)), 10, N'Phòng đôi tiện nghi, phù hợp cặp đôi hoặc khách đi một mình.', N'/Content/images/rooms/deluxe-double.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (82, 31, N'Deluxe Twin', N'Phòng 2 giường đơn', 2, 2, CAST(30.00 AS Decimal(10, 2)), CAST(1800000.00 AS Decimal(18, 2)), 10, N'Phòng 2 giường đơn, phù hợp bạn bè hoặc đồng nghiệp.', N'/Content/images/rooms/deluxe-twin.jpg', 1)
INSERT [dbo].[Phong] ([MaPhong], [MaKS], [TenPhong], [LoaiPhong], [SoNguoiToiDa], [SoGiuong], [DienTich], [GiaPhong], [SoLuong], [MoTa], [HinhAnh], [TrangThai]) VALUES (83, 31, N'Family Suite', N'Phòng gia đình', 4, 2, CAST(55.00 AS Decimal(10, 2)), CAST(3240000.00 AS Decimal(18, 2)), 4, N'Phòng rộng cho gia đình, tối đa 4 khách.', N'/Content/images/rooms/family-suite.jpg', 1)
SET IDENTITY_INSERT [dbo].[Phong] OFF
GO
SET IDENTITY_INSERT [dbo].[Quyen] ON 

INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (1, N'Đăng ký tài khoản', N'ACCOUNT_REGISTER', N'Cho phép đăng ký tài khoản', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (2, N'Đăng nhập', N'ACCOUNT_LOGIN', N'Cho phép đăng nhập hệ thống', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (3, N'Quản lý hồ sơ', N'ACCOUNT_PROFILE', N'Xem và cập nhật hồ sơ', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (4, N'Xem khách sạn', N'HOTEL_VIEW', N'Xem danh sách và thông tin khách sạn', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (5, N'Thêm khách sạn', N'HOTEL_CREATE', N'Thêm khách sạn', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (6, N'Sửa khách sạn', N'HOTEL_UPDATE', N'Cập nhật khách sạn', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (7, N'Xóa khách sạn', N'HOTEL_DELETE', N'Xóa khách sạn', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (8, N'Xem phòng', N'ROOM_VIEW', N'Xem phòng', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (9, N'Thêm phòng', N'ROOM_CREATE', N'Thêm phòng', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (10, N'Sửa phòng', N'ROOM_UPDATE', N'Cập nhật phòng', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (11, N'Xóa phòng', N'ROOM_DELETE', N'Xóa phòng', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (12, N'Đặt phòng', N'BOOKING_CREATE', N'Tạo đơn đặt phòng', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (13, N'Xem đơn đặt phòng', N'BOOKING_VIEW', N'Xem đơn đặt phòng', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (14, N'Xác nhận đơn', N'BOOKING_CONFIRM', N'Xác nhận đơn đặt phòng', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (15, N'Hủy đơn', N'BOOKING_CANCEL', N'Hủy đơn đặt phòng', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (16, N'Tạo thanh toán', N'PAYMENT_CREATE', N'Tạo thanh toán', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (17, N'Quản lý thanh toán', N'PAYMENT_MANAGE', N'Quản lý thanh toán', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (18, N'Xem chuyến bay', N'FLIGHT_VIEW', N'Xem chuyến bay', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (19, N'Đặt vé máy bay', N'FLIGHT_BOOK', N'Đặt vé máy bay', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (20, N'Xem vé máy bay', N'TICKET_VIEW', N'Xem vé máy bay', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (21, N'Đánh giá khách sạn', N'REVIEW_CREATE', N'Viết đánh giá', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (22, N'Quản lý đánh giá', N'REVIEW_MANAGE', N'Quản lý đánh giá', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (23, N'Quản lý yêu thích', N'FAVORITE_MANAGE', N'Thêm hoặc xóa khách sạn yêu thích', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (24, N'Xem bài viết', N'ARTICLE_VIEW', N'Xem bài viết', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (25, N'Thêm bài viết', N'ARTICLE_CREATE', N'Thêm bài viết', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (26, N'Sửa bài viết', N'ARTICLE_UPDATE', N'Sửa bài viết', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (27, N'Xóa bài viết', N'ARTICLE_DELETE', N'Xóa bài viết', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (28, N'Quản lý người dùng', N'USER_MANAGE', N'Quản lý tài khoản người dùng', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (29, N'Quản lý đối tác', N'PARTNER_MANAGE', N'Quản lý đối tác', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (30, N'Quản lý vai trò quyền', N'ROLE_PERMISSION_MANAGE', N'Quản lý vai trò và quyền', 1)
INSERT [dbo].[Quyen] ([MaQuyen], [TenQuyen], [MaQuyenCode], [MoTa], [TrangThai]) VALUES (31, N'Xem báo cáo', N'REPORT_VIEW', N'Xem báo cáo thống kê', 1)
SET IDENTITY_INSERT [dbo].[Quyen] OFF
GO
SET IDENTITY_INSERT [dbo].[TaiKhoan] ON 

INSERT [dbo].[TaiKhoan] ([MaTaiKhoan], [TenDangNhap], [Email], [MatKhau], [MaVaiTro], [MaKH], [TrangThai], [NgayTao], [LanDangNhapCuoi]) VALUES (1, N'khachhang01', N'an@gmail.com', N'123456', 1, 1, 1, CAST(N'2026-10-01T22:02:50.257' AS DateTime), CAST(N'2026-10-02T11:25:36.217' AS DateTime))
INSERT [dbo].[TaiKhoan] ([MaTaiKhoan], [TenDangNhap], [Email], [MatKhau], [MaVaiTro], [MaKH], [TrangThai], [NgayTao], [LanDangNhapCuoi]) VALUES (2, N'khachhang02', N'lan@gmail.com', N'123456', 1, 2, 1, CAST(N'2026-10-01T22:02:50.257' AS DateTime), NULL)
INSERT [dbo].[TaiKhoan] ([MaTaiKhoan], [TenDangNhap], [Email], [MatKhau], [MaVaiTro], [MaKH], [TrangThai], [NgayTao], [LanDangNhapCuoi]) VALUES (3, N'khachhang03', N'anh@gmail.com', N'123456', 1, 3, 1, CAST(N'2026-10-01T22:02:50.257' AS DateTime), NULL)
INSERT [dbo].[TaiKhoan] ([MaTaiKhoan], [TenDangNhap], [Email], [MatKhau], [MaVaiTro], [MaKH], [TrangThai], [NgayTao], [LanDangNhapCuoi]) VALUES (4, N'doitac01', N'partner@gmail.com', N'123456', 2, NULL, 1, CAST(N'2026-10-01T22:02:50.257' AS DateTime), CAST(N'2026-10-02T11:28:13.110' AS DateTime))
INSERT [dbo].[TaiKhoan] ([MaTaiKhoan], [TenDangNhap], [Email], [MatKhau], [MaVaiTro], [MaKH], [TrangThai], [NgayTao], [LanDangNhapCuoi]) VALUES (5, N'admin', N'admin@travelhub.vn', N'123456', 3, NULL, 1, CAST(N'2026-10-01T22:02:50.257' AS DateTime), CAST(N'2026-10-02T10:51:49.530' AS DateTime))
INSERT [dbo].[TaiKhoan] ([MaTaiKhoan], [TenDangNhap], [Email], [MatKhau], [MaVaiTro], [MaKH], [TrangThai], [NgayTao], [LanDangNhapCuoi]) VALUES (6, N'vile111', N'html@gmail.com', N'$2a$11$k/2B1FVEDk3IrgjPaJlaAOEsLY0BtpoiezDpxg4JuOf99ajfFeHv2', 1, 4, 1, CAST(N'2026-10-02T11:32:17.473' AS DateTime), CAST(N'2026-10-02T11:32:18.053' AS DateTime))
SET IDENTITY_INSERT [dbo].[TaiKhoan] OFF
GO
SET IDENTITY_INSERT [dbo].[ThanhToan] ON 

INSERT [dbo].[ThanhToan] ([MaThanhToan], [MaDon], [SoTien], [PhuongThuc], [MaGiaoDich], [TrangThai], [TrangThaiCode], [LoaiGiaoDich], [NgayThanhToan]) VALUES (1, 1, CAST(1620000.00 AS Decimal(18, 2)), N'Chuyển khoản', N'GD2026100001', N'Đã thanh toán', N'Paid', N'Charge', CAST(N'2026-09-20T10:30:00.000' AS DateTime))
INSERT [dbo].[ThanhToan] ([MaThanhToan], [MaDon], [SoTien], [PhuongThuc], [MaGiaoDich], [TrangThai], [TrangThaiCode], [LoaiGiaoDich], [NgayThanhToan]) VALUES (2, 4, CAST(1800000.00 AS Decimal(18, 2)), N'Thẻ nội địa', N'PAY4003719', N'Đã thanh toán', N'Paid', N'Charge', CAST(N'2026-10-02T00:37:19.153' AS DateTime))
INSERT [dbo].[ThanhToan] ([MaThanhToan], [MaDon], [SoTien], [PhuongThuc], [MaGiaoDich], [TrangThai], [TrangThaiCode], [LoaiGiaoDich], [NgayThanhToan]) VALUES (3, 6, CAST(1800000.00 AS Decimal(18, 2)), N'Thẻ nội địa', N'PAY6103737', N'Đã thanh toán', N'Paid', N'Charge', CAST(N'2026-10-02T10:37:37.930' AS DateTime))
INSERT [dbo].[ThanhToan] ([MaThanhToan], [MaDon], [SoTien], [PhuongThuc], [MaGiaoDich], [TrangThai], [TrangThaiCode], [LoaiGiaoDich], [NgayThanhToan]) VALUES (4, 8, CAST(1800000.00 AS Decimal(18, 2)), N'Thẻ nội địa', N'PAY8112659', N'Đã thanh toán', N'Paid', N'Charge', CAST(N'2026-10-02T11:26:59.947' AS DateTime))
SET IDENTITY_INSERT [dbo].[ThanhToan] OFF
GO
SET IDENTITY_INSERT [dbo].[TienIch] ON 

INSERT [dbo].[TienIch] ([MaTienIch], [TenTienIch], [Icon], [TrangThai]) VALUES (1, N'WiFi miễn phí', N'fa-wifi', 1)
INSERT [dbo].[TienIch] ([MaTienIch], [TenTienIch], [Icon], [TrangThai]) VALUES (2, N'Hồ bơi', N'fa-person-swimming', 1)
INSERT [dbo].[TienIch] ([MaTienIch], [TenTienIch], [Icon], [TrangThai]) VALUES (3, N'Bãi đỗ xe', N'fa-square-parking', 1)
INSERT [dbo].[TienIch] ([MaTienIch], [TenTienIch], [Icon], [TrangThai]) VALUES (4, N'Nhà hàng', N'fa-utensils', 1)
INSERT [dbo].[TienIch] ([MaTienIch], [TenTienIch], [Icon], [TrangThai]) VALUES (5, N'Phòng gym', N'fa-dumbbell', 1)
INSERT [dbo].[TienIch] ([MaTienIch], [TenTienIch], [Icon], [TrangThai]) VALUES (6, N'Spa', N'fa-spa', 1)
INSERT [dbo].[TienIch] ([MaTienIch], [TenTienIch], [Icon], [TrangThai]) VALUES (7, N'Lễ tân 24/7', N'fa-clock', 1)
INSERT [dbo].[TienIch] ([MaTienIch], [TenTienIch], [Icon], [TrangThai]) VALUES (8, N'Đưa đón sân bay', N'fa-plane', 1)
INSERT [dbo].[TienIch] ([MaTienIch], [TenTienIch], [Icon], [TrangThai]) VALUES (9, N'Bãi biển riêng', N'fa-umbrella-beach', 1)
INSERT [dbo].[TienIch] ([MaTienIch], [TenTienIch], [Icon], [TrangThai]) VALUES (10, N'Quầy bar', N'fa-martini-glass', 1)
INSERT [dbo].[TienIch] ([MaTienIch], [TenTienIch], [Icon], [TrangThai]) VALUES (11, N'Giặt ủi', N'fa-shirt', 1)
INSERT [dbo].[TienIch] ([MaTienIch], [TenTienIch], [Icon], [TrangThai]) VALUES (12, N'Khu vui chơi trẻ em', N'fa-child', 1)
SET IDENTITY_INSERT [dbo].[TienIch] OFF
GO
SET IDENTITY_INSERT [dbo].[VaiTro] ON 

INSERT [dbo].[VaiTro] ([MaVaiTro], [TenVaiTro], [MoTa], [TrangThai]) VALUES (1, N'Khách du lịch', N'Người sử dụng website để tìm kiếm, đặt phòng, mua vé máy bay và khám phá địa điểm du lịch.', 1)
INSERT [dbo].[VaiTro] ([MaVaiTro], [TenVaiTro], [MoTa], [TrangThai]) VALUES (2, N'Đối tác cung cấp dịch vụ', N'Đơn vị cung cấp khách sạn, phòng và các dịch vụ du lịch trên TravelHub.', 1)
INSERT [dbo].[VaiTro] ([MaVaiTro], [TenVaiTro], [MoTa], [TrangThai]) VALUES (3, N'Quản trị viên', N'Quản lý toàn bộ hệ thống TravelHub.', 1)
SET IDENTITY_INSERT [dbo].[VaiTro] OFF
GO
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (1, 1)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (1, 2)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (1, 3)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (1, 4)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (1, 8)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (1, 12)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (1, 13)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (1, 15)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (1, 16)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (1, 18)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (1, 19)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (1, 20)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (1, 21)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (1, 23)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (1, 24)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (2, 2)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (2, 3)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (2, 4)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (2, 5)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (2, 6)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (2, 7)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (2, 8)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (2, 9)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (2, 10)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (2, 11)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (2, 13)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (2, 14)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (2, 15)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (2, 17)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (2, 24)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (2, 25)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (2, 26)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 1)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 2)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 3)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 4)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 5)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 6)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 7)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 8)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 9)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 10)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 11)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 12)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 13)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 14)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 15)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 16)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 17)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 18)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 19)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 20)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 21)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 22)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 23)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 24)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 25)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 26)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 27)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 28)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 29)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 30)
INSERT [dbo].[VaiTro_Quyen] ([MaVaiTro], [MaQuyen]) VALUES (3, 31)
GO
SET IDENTITY_INSERT [dbo].[VeMayBay] ON 

INSERT [dbo].[VeMayBay] ([MaVe], [MaKH], [MaChuyenBay], [HoTenHanhKhach], [SoGiayTo], [SoGhe], [GiaVe], [TrangThai], [NgayDat]) VALUES (1, 1, 1, N'Nguyễn Văn An', N'079205001234', N'12A', CAST(1200000.00 AS Decimal(18, 2)), N'Đã đặt', CAST(N'2026-10-01T22:02:50.490' AS DateTime))
INSERT [dbo].[VeMayBay] ([MaVe], [MaKH], [MaChuyenBay], [HoTenHanhKhach], [SoGiayTo], [SoGhe], [GiaVe], [TrangThai], [NgayDat]) VALUES (2, 2, 2, N'Trần Thị Lan', N'079204005678', N'15B', CAST(950000.00 AS Decimal(18, 2)), N'Đã đặt', CAST(N'2026-10-01T22:02:50.490' AS DateTime))
SET IDENTITY_INSERT [dbo].[VeMayBay] OFF
GO
INSERT [dbo].[YeuThich] ([MaKH], [MaKS], [NgayThem]) VALUES (1, 1, CAST(N'2026-10-01T22:02:50.397' AS DateTime))
INSERT [dbo].[YeuThich] ([MaKH], [MaKS], [NgayThem]) VALUES (1, 3, CAST(N'2026-10-01T22:02:50.397' AS DateTime))
INSERT [dbo].[YeuThich] ([MaKH], [MaKS], [NgayThem]) VALUES (2, 2, CAST(N'2026-10-01T22:02:50.397' AS DateTime))
INSERT [dbo].[YeuThich] ([MaKH], [MaKS], [NgayThem]) VALUES (2, 4, CAST(N'2026-10-01T22:02:50.397' AS DateTime))
INSERT [dbo].[YeuThich] ([MaKH], [MaKS], [NgayThem]) VALUES (3, 1, CAST(N'2026-10-01T22:02:50.397' AS DateTime))
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_ChuyenBay_MaChuyen]    Script Date: 05/10/2026 11:17:38 CH ******/
ALTER TABLE [dbo].[ChuyenBay] ADD  CONSTRAINT [UQ_ChuyenBay_MaChuyen] UNIQUE NONCLUSTERED 
(
	[MaChuyen] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__DonDatPh__FD3AB1E584D02CCD]    Script Date: 05/10/2026 11:17:38 CH ******/
ALTER TABLE [dbo].[DonDatPhong] ADD UNIQUE NONCLUSTERED 
(
	[MaDonCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__KhachHan__A9D10534F6AE2833]    Script Date: 05/10/2026 11:17:38 CH ******/
ALTER TABLE [dbo].[KhachHang] ADD UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__KhuyenMa__152C7C5C92F70D72]    Script Date: 05/10/2026 11:17:38 CH ******/
ALTER TABLE [dbo].[KhuyenMai] ADD UNIQUE NONCLUSTERED 
(
	[MaCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Quyen__5637EE791F504980]    Script Date: 05/10/2026 11:17:38 CH ******/
ALTER TABLE [dbo].[Quyen] ADD UNIQUE NONCLUSTERED 
(
	[TenQuyen] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Quyen__D9D5AFD1515002DE]    Script Date: 05/10/2026 11:17:38 CH ******/
ALTER TABLE [dbo].[Quyen] ADD UNIQUE NONCLUSTERED 
(
	[MaQuyenCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__TaiKhoan__55F68FC0F7CD4916]    Script Date: 05/10/2026 11:17:38 CH ******/
ALTER TABLE [dbo].[TaiKhoan] ADD UNIQUE NONCLUSTERED 
(
	[TenDangNhap] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__TaiKhoan__A9D10534FDE5A64F]    Script Date: 05/10/2026 11:17:38 CH ******/
ALTER TABLE [dbo].[TaiKhoan] ADD UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UQ_TaiKhoan_MaKH]    Script Date: 05/10/2026 11:17:38 CH ******/
CREATE UNIQUE NONCLUSTERED INDEX [UQ_TaiKhoan_MaKH] ON [dbo].[TaiKhoan]
(
	[MaKH] ASC
)
WHERE ([MaKH] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__TienIch__0DE17DA854C52DDA]    Script Date: 05/10/2026 11:17:38 CH ******/
ALTER TABLE [dbo].[TienIch] ADD UNIQUE NONCLUSTERED 
(
	[TenTienIch] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__VaiTro__1DA558144746644A]    Script Date: 05/10/2026 11:17:38 CH ******/
ALTER TABLE [dbo].[VaiTro] ADD UNIQUE NONCLUSTERED 
(
	[TenVaiTro] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[BaiViet] ADD  DEFAULT (getdate()) FOR [NgayDang]
GO
ALTER TABLE [dbo].[BaiViet] ADD  DEFAULT ((0)) FOR [LuotXem]
GO
ALTER TABLE [dbo].[BaiViet] ADD  DEFAULT ((0)) FOR [NoiBat]
GO
ALTER TABLE [dbo].[BaiViet] ADD  DEFAULT ((1)) FOR [TrangThai]
GO
ALTER TABLE [dbo].[ChuyenBay] ADD  DEFAULT ((0)) FOR [SoGhe]
GO
ALTER TABLE [dbo].[ChuyenBay] ADD  DEFAULT ((0)) FOR [SoGheConLai]
GO
ALTER TABLE [dbo].[ChuyenBay] ADD  DEFAULT ((1)) FOR [TrangThai]
GO
ALTER TABLE [dbo].[DanhGia] ADD  DEFAULT (getdate()) FOR [NgayDanhGia]
GO
ALTER TABLE [dbo].[DanhGia] ADD  DEFAULT ((1)) FOR [TrangThai]
GO
ALTER TABLE [dbo].[DiaDiem] ADD  DEFAULT (N'Việt Nam') FOR [QuocGia]
GO
ALTER TABLE [dbo].[DiaDiem] ADD  DEFAULT ((1)) FOR [TrangThai]
GO
ALTER TABLE [dbo].[DiaDiemDuLich] ADD  DEFAULT ((0)) FOR [NoiBat]
GO
ALTER TABLE [dbo].[DiaDiemDuLich] ADD  DEFAULT ((1)) FOR [TrangThai]
GO
ALTER TABLE [dbo].[DonDatPhong] ADD  DEFAULT ((1)) FOR [SoNguoiLon]
GO
ALTER TABLE [dbo].[DonDatPhong] ADD  DEFAULT ((0)) FOR [SoTreEm]
GO
ALTER TABLE [dbo].[DonDatPhong] ADD  DEFAULT ((0)) FOR [TongTien]
GO
ALTER TABLE [dbo].[DonDatPhong] ADD  DEFAULT ((0)) FOR [GiamGia]
GO
ALTER TABLE [dbo].[DonDatPhong] ADD  DEFAULT ((0)) FOR [ThanhTien]
GO
ALTER TABLE [dbo].[DonDatPhong] ADD  DEFAULT (N'Chờ xác nhận') FOR [TrangThai]
GO
ALTER TABLE [dbo].[DonDatPhong] ADD  DEFAULT (getdate()) FOR [NgayDat]
GO
ALTER TABLE [dbo].[HangBay] ADD  DEFAULT ((1)) FOR [TrangThai]
GO
ALTER TABLE [dbo].[HinhAnhKhachSan] ADD  DEFAULT ((0)) FOR [LaAnhDaiDien]
GO
ALTER TABLE [dbo].[KhachHang] ADD  DEFAULT ((1)) FOR [TrangThai]
GO
ALTER TABLE [dbo].[KhachHang] ADD  DEFAULT (getdate()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[KhachSan] ADD  DEFAULT ((1)) FOR [TrangThai]
GO
ALTER TABLE [dbo].[KhachSan] ADD  DEFAULT (getdate()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[KhuyenMai] ADD  DEFAULT ((1)) FOR [TrangThai]
GO
ALTER TABLE [dbo].[Phong] ADD  DEFAULT ((1)) FOR [SoLuong]
GO
ALTER TABLE [dbo].[Phong] ADD  DEFAULT ((1)) FOR [TrangThai]
GO
ALTER TABLE [dbo].[Quyen] ADD  DEFAULT ((1)) FOR [TrangThai]
GO
ALTER TABLE [dbo].[TaiKhoan] ADD  DEFAULT ((1)) FOR [TrangThai]
GO
ALTER TABLE [dbo].[TaiKhoan] ADD  DEFAULT (getdate()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[ThanhToan] ADD  DEFAULT (N'Chờ thanh toán') FOR [TrangThai]
GO
ALTER TABLE [dbo].[TienIch] ADD  DEFAULT ((1)) FOR [TrangThai]
GO
ALTER TABLE [dbo].[VaiTro] ADD  DEFAULT ((1)) FOR [TrangThai]
GO
ALTER TABLE [dbo].[VeMayBay] ADD  DEFAULT (N'Đã đặt') FOR [TrangThai]
GO
ALTER TABLE [dbo].[VeMayBay] ADD  DEFAULT (getdate()) FOR [NgayDat]
GO
ALTER TABLE [dbo].[YeuThich] ADD  DEFAULT (getdate()) FOR [NgayThem]
GO
ALTER TABLE [dbo].[BaiViet]  WITH CHECK ADD  CONSTRAINT [FK_BaiViet_DiaDiem] FOREIGN KEY([MaDiaDiem])
REFERENCES [dbo].[DiaDiem] ([MaDiaDiem])
GO
ALTER TABLE [dbo].[BaiViet] CHECK CONSTRAINT [FK_BaiViet_DiaDiem]
GO
ALTER TABLE [dbo].[ChiTietDatPhong]  WITH CHECK ADD  CONSTRAINT [FK_ChiTietDatPhong_DonDatPhong] FOREIGN KEY([MaDon])
REFERENCES [dbo].[DonDatPhong] ([MaDon])
GO
ALTER TABLE [dbo].[ChiTietDatPhong] CHECK CONSTRAINT [FK_ChiTietDatPhong_DonDatPhong]
GO
ALTER TABLE [dbo].[ChiTietDatPhong]  WITH CHECK ADD  CONSTRAINT [FK_ChiTietDatPhong_Phong] FOREIGN KEY([MaPhong])
REFERENCES [dbo].[Phong] ([MaPhong])
GO
ALTER TABLE [dbo].[ChiTietDatPhong] CHECK CONSTRAINT [FK_ChiTietDatPhong_Phong]
GO
ALTER TABLE [dbo].[ChuyenBay]  WITH CHECK ADD  CONSTRAINT [FK_ChuyenBay_HangBay] FOREIGN KEY([MaHangBay])
REFERENCES [dbo].[HangBay] ([MaHangBay])
GO
ALTER TABLE [dbo].[ChuyenBay] CHECK CONSTRAINT [FK_ChuyenBay_HangBay]
GO
ALTER TABLE [dbo].[DanhGia]  WITH CHECK ADD  CONSTRAINT [FK_DanhGia_KhachHang] FOREIGN KEY([MaKH])
REFERENCES [dbo].[KhachHang] ([MaKH])
GO
ALTER TABLE [dbo].[DanhGia] CHECK CONSTRAINT [FK_DanhGia_KhachHang]
GO
ALTER TABLE [dbo].[DanhGia]  WITH CHECK ADD  CONSTRAINT [FK_DanhGia_KhachSan] FOREIGN KEY([MaKS])
REFERENCES [dbo].[KhachSan] ([MaKS])
GO
ALTER TABLE [dbo].[DanhGia] CHECK CONSTRAINT [FK_DanhGia_KhachSan]
GO
ALTER TABLE [dbo].[DiaDiemDuLich]  WITH CHECK ADD  CONSTRAINT [FK_DiaDiemDuLich_DiaDiem] FOREIGN KEY([MaDiaDiem])
REFERENCES [dbo].[DiaDiem] ([MaDiaDiem])
GO
ALTER TABLE [dbo].[DiaDiemDuLich] CHECK CONSTRAINT [FK_DiaDiemDuLich_DiaDiem]
GO
ALTER TABLE [dbo].[DonDatPhong]  WITH CHECK ADD  CONSTRAINT [FK_DonDatPhong_KhachHang] FOREIGN KEY([MaKH])
REFERENCES [dbo].[KhachHang] ([MaKH])
GO
ALTER TABLE [dbo].[DonDatPhong] CHECK CONSTRAINT [FK_DonDatPhong_KhachHang]
GO
ALTER TABLE [dbo].[DonDatPhong]  WITH CHECK ADD  CONSTRAINT [FK_DonDatPhong_KhachSan] FOREIGN KEY([MaKS])
REFERENCES [dbo].[KhachSan] ([MaKS])
GO
ALTER TABLE [dbo].[DonDatPhong] CHECK CONSTRAINT [FK_DonDatPhong_KhachSan]
GO
ALTER TABLE [dbo].[DonDatPhong]  WITH CHECK ADD  CONSTRAINT [FK_DonDatPhong_KhuyenMai] FOREIGN KEY([MaKM])
REFERENCES [dbo].[KhuyenMai] ([MaKM])
GO
ALTER TABLE [dbo].[DonDatPhong] CHECK CONSTRAINT [FK_DonDatPhong_KhuyenMai]
GO
ALTER TABLE [dbo].[HinhAnhKhachSan]  WITH CHECK ADD  CONSTRAINT [FK_HinhAnhKhachSan_KhachSan] FOREIGN KEY([MaKS])
REFERENCES [dbo].[KhachSan] ([MaKS])
GO
ALTER TABLE [dbo].[HinhAnhKhachSan] CHECK CONSTRAINT [FK_HinhAnhKhachSan_KhachSan]
GO
ALTER TABLE [dbo].[KhachSan]  WITH CHECK ADD  CONSTRAINT [FK_KhachSan_DiaDiem] FOREIGN KEY([MaDiaDiem])
REFERENCES [dbo].[DiaDiem] ([MaDiaDiem])
GO
ALTER TABLE [dbo].[KhachSan] CHECK CONSTRAINT [FK_KhachSan_DiaDiem]
GO
ALTER TABLE [dbo].[KhachSan]  WITH CHECK ADD  CONSTRAINT [FK_KhachSan_DoiTac] FOREIGN KEY([MaTaiKhoanDoiTac])
REFERENCES [dbo].[TaiKhoan] ([MaTaiKhoan])
GO
ALTER TABLE [dbo].[KhachSan] CHECK CONSTRAINT [FK_KhachSan_DoiTac]
GO
ALTER TABLE [dbo].[KhachSan_TienIch]  WITH CHECK ADD  CONSTRAINT [FK_KhachSanTienIch_KhachSan] FOREIGN KEY([MaKS])
REFERENCES [dbo].[KhachSan] ([MaKS])
GO
ALTER TABLE [dbo].[KhachSan_TienIch] CHECK CONSTRAINT [FK_KhachSanTienIch_KhachSan]
GO
ALTER TABLE [dbo].[KhachSan_TienIch]  WITH CHECK ADD  CONSTRAINT [FK_KhachSanTienIch_TienIch] FOREIGN KEY([MaTienIch])
REFERENCES [dbo].[TienIch] ([MaTienIch])
GO
ALTER TABLE [dbo].[KhachSan_TienIch] CHECK CONSTRAINT [FK_KhachSanTienIch_TienIch]
GO
ALTER TABLE [dbo].[Phong]  WITH CHECK ADD  CONSTRAINT [FK_Phong_KhachSan] FOREIGN KEY([MaKS])
REFERENCES [dbo].[KhachSan] ([MaKS])
GO
ALTER TABLE [dbo].[Phong] CHECK CONSTRAINT [FK_Phong_KhachSan]
GO
ALTER TABLE [dbo].[TaiKhoan]  WITH CHECK ADD  CONSTRAINT [FK_TaiKhoan_KhachHang] FOREIGN KEY([MaKH])
REFERENCES [dbo].[KhachHang] ([MaKH])
GO
ALTER TABLE [dbo].[TaiKhoan] CHECK CONSTRAINT [FK_TaiKhoan_KhachHang]
GO
ALTER TABLE [dbo].[TaiKhoan]  WITH CHECK ADD  CONSTRAINT [FK_TaiKhoan_VaiTro] FOREIGN KEY([MaVaiTro])
REFERENCES [dbo].[VaiTro] ([MaVaiTro])
GO
ALTER TABLE [dbo].[TaiKhoan] CHECK CONSTRAINT [FK_TaiKhoan_VaiTro]
GO
ALTER TABLE [dbo].[ThanhToan]  WITH CHECK ADD  CONSTRAINT [FK_ThanhToan_DonDatPhong] FOREIGN KEY([MaDon])
REFERENCES [dbo].[DonDatPhong] ([MaDon])
GO
ALTER TABLE [dbo].[ThanhToan] CHECK CONSTRAINT [FK_ThanhToan_DonDatPhong]
GO
ALTER TABLE [dbo].[VaiTro_Quyen]  WITH CHECK ADD  CONSTRAINT [FK_VaiTroQuyen_Quyen] FOREIGN KEY([MaQuyen])
REFERENCES [dbo].[Quyen] ([MaQuyen])
GO
ALTER TABLE [dbo].[VaiTro_Quyen] CHECK CONSTRAINT [FK_VaiTroQuyen_Quyen]
GO
ALTER TABLE [dbo].[VaiTro_Quyen]  WITH CHECK ADD  CONSTRAINT [FK_VaiTroQuyen_VaiTro] FOREIGN KEY([MaVaiTro])
REFERENCES [dbo].[VaiTro] ([MaVaiTro])
GO
ALTER TABLE [dbo].[VaiTro_Quyen] CHECK CONSTRAINT [FK_VaiTroQuyen_VaiTro]
GO
ALTER TABLE [dbo].[VeMayBay]  WITH CHECK ADD  CONSTRAINT [FK_VeMayBay_ChuyenBay] FOREIGN KEY([MaChuyenBay])
REFERENCES [dbo].[ChuyenBay] ([MaChuyenBay])
GO
ALTER TABLE [dbo].[VeMayBay] CHECK CONSTRAINT [FK_VeMayBay_ChuyenBay]
GO
ALTER TABLE [dbo].[VeMayBay]  WITH CHECK ADD  CONSTRAINT [FK_VeMayBay_KhachHang] FOREIGN KEY([MaKH])
REFERENCES [dbo].[KhachHang] ([MaKH])
GO
ALTER TABLE [dbo].[VeMayBay] CHECK CONSTRAINT [FK_VeMayBay_KhachHang]
GO
ALTER TABLE [dbo].[YeuThich]  WITH CHECK ADD  CONSTRAINT [FK_YeuThich_KhachHang] FOREIGN KEY([MaKH])
REFERENCES [dbo].[KhachHang] ([MaKH])
GO
ALTER TABLE [dbo].[YeuThich] CHECK CONSTRAINT [FK_YeuThich_KhachHang]
GO
ALTER TABLE [dbo].[YeuThich]  WITH CHECK ADD  CONSTRAINT [FK_YeuThich_KhachSan] FOREIGN KEY([MaKS])
REFERENCES [dbo].[KhachSan] ([MaKS])
GO
ALTER TABLE [dbo].[YeuThich] CHECK CONSTRAINT [FK_YeuThich_KhachSan]
GO
ALTER TABLE [dbo].[BaiViet]  WITH CHECK ADD  CONSTRAINT [CK_BaiViet_LuotXem] CHECK  (([LuotXem]>=(0)))
GO
ALTER TABLE [dbo].[BaiViet] CHECK CONSTRAINT [CK_BaiViet_LuotXem]
GO
ALTER TABLE [dbo].[ChiTietDatPhong]  WITH CHECK ADD  CONSTRAINT [CK_ChiTietDatPhong_DonGia] CHECK  (([DonGia]>=(0)))
GO
ALTER TABLE [dbo].[ChiTietDatPhong] CHECK CONSTRAINT [CK_ChiTietDatPhong_DonGia]
GO
ALTER TABLE [dbo].[ChiTietDatPhong]  WITH CHECK ADD  CONSTRAINT [CK_ChiTietDatPhong_SoLuong] CHECK  (([SoLuong]>(0)))
GO
ALTER TABLE [dbo].[ChiTietDatPhong] CHECK CONSTRAINT [CK_ChiTietDatPhong_SoLuong]
GO
ALTER TABLE [dbo].[ChuyenBay]  WITH CHECK ADD  CONSTRAINT [CK_ChuyenBay_GheConLai] CHECK  (([SoGheConLai]<=[SoGhe]))
GO
ALTER TABLE [dbo].[ChuyenBay] CHECK CONSTRAINT [CK_ChuyenBay_GheConLai]
GO
ALTER TABLE [dbo].[ChuyenBay]  WITH CHECK ADD  CONSTRAINT [CK_ChuyenBay_Gia] CHECK  (([GiaVe]>=(0)))
GO
ALTER TABLE [dbo].[ChuyenBay] CHECK CONSTRAINT [CK_ChuyenBay_Gia]
GO
ALTER TABLE [dbo].[ChuyenBay]  WITH CHECK ADD  CONSTRAINT [CK_ChuyenBay_SoGhe] CHECK  (([SoGhe]>=(0) AND [SoGheConLai]>=(0)))
GO
ALTER TABLE [dbo].[ChuyenBay] CHECK CONSTRAINT [CK_ChuyenBay_SoGhe]
GO
ALTER TABLE [dbo].[ChuyenBay]  WITH CHECK ADD  CONSTRAINT [CK_ChuyenBay_ThoiGian] CHECK  (([ThoiGianDen]>[ThoiGianDi]))
GO
ALTER TABLE [dbo].[ChuyenBay] CHECK CONSTRAINT [CK_ChuyenBay_ThoiGian]
GO
ALTER TABLE [dbo].[DanhGia]  WITH CHECK ADD  CONSTRAINT [CK_DanhGia_SoSao] CHECK  (([SoSao]>=(1) AND [SoSao]<=(5)))
GO
ALTER TABLE [dbo].[DanhGia] CHECK CONSTRAINT [CK_DanhGia_SoSao]
GO
ALTER TABLE [dbo].[DiaDiemDuLich]  WITH CHECK ADD  CONSTRAINT [CK_DiaDiemDuLich_GiaVe] CHECK  (([GiaVe] IS NULL OR [GiaVe]>=(0)))
GO
ALTER TABLE [dbo].[DiaDiemDuLich] CHECK CONSTRAINT [CK_DiaDiemDuLich_GiaVe]
GO
ALTER TABLE [dbo].[DonDatPhong]  WITH CHECK ADD  CONSTRAINT [CK_DonDatPhong_Ngay] CHECK  (([NgayTraPhong]>[NgayNhanPhong]))
GO
ALTER TABLE [dbo].[DonDatPhong] CHECK CONSTRAINT [CK_DonDatPhong_Ngay]
GO
ALTER TABLE [dbo].[DonDatPhong]  WITH CHECK ADD  CONSTRAINT [CK_DonDatPhong_NguoiLon] CHECK  (([SoNguoiLon]>(0)))
GO
ALTER TABLE [dbo].[DonDatPhong] CHECK CONSTRAINT [CK_DonDatPhong_NguoiLon]
GO
ALTER TABLE [dbo].[DonDatPhong]  WITH CHECK ADD  CONSTRAINT [CK_DonDatPhong_Tien] CHECK  (([TongTien]>=(0) AND [GiamGia]>=(0) AND [ThanhTien]>=(0)))
GO
ALTER TABLE [dbo].[DonDatPhong] CHECK CONSTRAINT [CK_DonDatPhong_Tien]
GO
ALTER TABLE [dbo].[DonDatPhong]  WITH CHECK ADD  CONSTRAINT [CK_DonDatPhong_TreEm] CHECK  (([SoTreEm]>=(0)))
GO
ALTER TABLE [dbo].[DonDatPhong] CHECK CONSTRAINT [CK_DonDatPhong_TreEm]
GO
ALTER TABLE [dbo].[KhachSan]  WITH CHECK ADD  CONSTRAINT [CK_KhachSan_GiaTu] CHECK  (([GiaTu]>=(0)))
GO
ALTER TABLE [dbo].[KhachSan] CHECK CONSTRAINT [CK_KhachSan_GiaTu]
GO
ALTER TABLE [dbo].[KhachSan]  WITH CHECK ADD  CONSTRAINT [CK_KhachSan_SoSao] CHECK  (([SoSao]>=(1) AND [SoSao]<=(5)))
GO
ALTER TABLE [dbo].[KhachSan] CHECK CONSTRAINT [CK_KhachSan_SoSao]
GO
ALTER TABLE [dbo].[KhuyenMai]  WITH CHECK ADD  CONSTRAINT [CK_KhuyenMai_PhanTram] CHECK  (([PhanTramGiam] IS NULL OR [PhanTramGiam]>=(0) AND [PhanTramGiam]<=(100)))
GO
ALTER TABLE [dbo].[KhuyenMai] CHECK CONSTRAINT [CK_KhuyenMai_PhanTram]
GO
ALTER TABLE [dbo].[KhuyenMai]  WITH CHECK ADD  CONSTRAINT [CK_KhuyenMai_SoTien] CHECK  (([SoTienGiam] IS NULL OR [SoTienGiam]>=(0)))
GO
ALTER TABLE [dbo].[KhuyenMai] CHECK CONSTRAINT [CK_KhuyenMai_SoTien]
GO
ALTER TABLE [dbo].[KhuyenMai]  WITH CHECK ADD  CONSTRAINT [CK_KhuyenMai_ThoiGian] CHECK  (([NgayKetThuc] IS NULL OR [NgayBatDau] IS NULL OR [NgayKetThuc]>[NgayBatDau]))
GO
ALTER TABLE [dbo].[KhuyenMai] CHECK CONSTRAINT [CK_KhuyenMai_ThoiGian]
GO
ALTER TABLE [dbo].[Phong]  WITH CHECK ADD  CONSTRAINT [CK_Phong_Gia] CHECK  (([GiaPhong]>=(0)))
GO
ALTER TABLE [dbo].[Phong] CHECK CONSTRAINT [CK_Phong_Gia]
GO
ALTER TABLE [dbo].[Phong]  WITH CHECK ADD  CONSTRAINT [CK_Phong_SoGiuong] CHECK  (([SoGiuong] IS NULL OR [SoGiuong]>(0)))
GO
ALTER TABLE [dbo].[Phong] CHECK CONSTRAINT [CK_Phong_SoGiuong]
GO
ALTER TABLE [dbo].[Phong]  WITH CHECK ADD  CONSTRAINT [CK_Phong_SoLuong] CHECK  (([SoLuong]>=(0)))
GO
ALTER TABLE [dbo].[Phong] CHECK CONSTRAINT [CK_Phong_SoLuong]
GO
ALTER TABLE [dbo].[Phong]  WITH CHECK ADD  CONSTRAINT [CK_Phong_SoNguoi] CHECK  (([SoNguoiToiDa]>(0)))
GO
ALTER TABLE [dbo].[Phong] CHECK CONSTRAINT [CK_Phong_SoNguoi]
GO
ALTER TABLE [dbo].[ThanhToan]  WITH CHECK ADD  CONSTRAINT [CK_ThanhToan_SoTien] CHECK  (([SoTien]>=(0)))
GO
ALTER TABLE [dbo].[ThanhToan] CHECK CONSTRAINT [CK_ThanhToan_SoTien]
GO
ALTER TABLE [dbo].[VeMayBay]  WITH CHECK ADD  CONSTRAINT [CK_VeMayBay_Gia] CHECK  (([GiaVe] IS NULL OR [GiaVe]>=(0)))
GO
ALTER TABLE [dbo].[VeMayBay] CHECK CONSTRAINT [CK_VeMayBay_Gia]
GO

SELECT
    kc.name AS ConstraintName,
    c.name AS ColumnName
FROM sys.key_constraints kc
INNER JOIN sys.index_columns ic
    ON kc.parent_object_id = ic.object_id
    AND kc.unique_index_id = ic.index_id
INNER JOIN sys.columns c
    ON ic.object_id = c.object_id
    AND ic.column_id = c.column_id
WHERE kc.name = 'UQ__DonDatPh__FD3AB1E584D02CCD';

USE TravelHubDB;
GO

SELECT
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'DonDatPhong'
  AND COLUMN_NAME = 'MaDonCode';

  SELECT TOP 20 MaDon, MaDonCode
FROM dbo.DonDatPhong
ORDER BY MaDon DESC;

USE TravelHubDB;
GO

EXEC sp_help 'dbo.DonDatPhong';

USE TravelHubDB;
GO

SELECT
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length AS MaxLength,
    c.is_nullable AS IsNullable
FROM sys.columns c
JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.DonDatPhong')
  AND c.name = 'MaDonCode';

  SELECT TOP 5
    MaDon,
    MaDonCode,
    TrangThai
FROM dbo.DonDatPhong
ORDER BY MaDon DESC;

USE TravelHubDB;
GO

SELECT
    MaDon,
    MaDonCode,
    TrangThai,
    ThanhTien
FROM dbo.DonDatPhong
WHERE MaDon = 15;
USE TravelHubDB;
GO

-- Kiểm tra khách hàng và khách sạn được lưu yêu thích
SELECT
    yt.MaKH,
    yt.MaKS,
    yt.NgayThem
FROM dbo.YeuThich AS yt
ORDER BY yt.NgayThem DESC;

-- Kiểm tra các cặp khách hàng - khách sạn bị lưu trùng
SELECT MaKH, MaKS, COUNT(*) AS SoLanLuu
FROM dbo.YeuThich
GROUP BY MaKH, MaKS
HAVING COUNT(*) > 1;

/* TravelHub - bổ sung quyền ngoại lệ cho từng tài khoản.
   Chạy script này một lần trên đúng database TravelHub đang dùng.
   Bảng chỉ lưu ngoại lệ: không có dòng = kế thừa quyền từ VaiTro_Quyen. */
IF OBJECT_ID(N'dbo.TaiKhoan_Quyen', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TaiKhoan_Quyen
    (
        MaTaiKhoan INT NOT NULL,
        MaQuyen INT NOT NULL,
        ChoPhep BIT NOT NULL,
        NguoiCapNhat INT NULL,
        NgayCapNhat DATETIME NOT NULL CONSTRAINT DF_TaiKhoan_Quyen_NgayCapNhat DEFAULT(GETDATE()),
        CONSTRAINT PK_TaiKhoan_Quyen PRIMARY KEY (MaTaiKhoan, MaQuyen),
        CONSTRAINT FK_TaiKhoan_Quyen_TaiKhoan FOREIGN KEY (MaTaiKhoan) REFERENCES dbo.TaiKhoan(MaTaiKhoan),
        CONSTRAINT FK_TaiKhoan_Quyen_Quyen FOREIGN KEY (MaQuyen) REFERENCES dbo.Quyen(MaQuyen)
    );
    CREATE INDEX IX_TaiKhoan_Quyen_MaQuyen ON dbo.TaiKhoan_Quyen(MaQuyen);
END;
GO

SELECT DB_NAME() AS DatabaseDangChay;

SELECT OBJECT_ID(
    N'dbo.TaiKhoan_Quyen',
    N'U'
) AS BangTaiKhoanQuyen;