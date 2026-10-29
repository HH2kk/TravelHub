/* 003 - ThanhToan.MaGiaoDich có UNIQUE (chỉ cho 1 NULL), nên MaGiaoDich = mã tham chiếu nội bộ (unique);
   mã giao dịch của cổng (vnp_TransactionNo) lưu ở cột riêng. */
SET XACT_ABORT ON;
IF COL_LENGTH('dbo.ThanhToan','MaGiaoDichCong') IS NULL ALTER TABLE dbo.ThanhToan ADD MaGiaoDichCong varchar(100) NULL;
IF COL_LENGTH('dbo.ThanhToan','MaPhanHoiCong') IS NULL ALTER TABLE dbo.ThanhToan ADD MaPhanHoiCong varchar(20) NULL;
IF COL_LENGTH('dbo.ThanhToan','NgayCapNhat') IS NULL ALTER TABLE dbo.ThanhToan ADD NgayCapNhat datetime NULL;
