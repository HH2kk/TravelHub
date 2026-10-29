/* 002 - Hàm tính số phòng còn lại tối thiểu trên TỪNG ĐÊM của khoảng [@In, @Out).
   Đếm đơn đang giữ chỗ: loại trừ Đã hủy / Hết hạn và đơn "Chờ thanh toán" đã quá NgayHetHan.
   Trừ thêm phòng ngừng bán/bảo trì (PhongNgungBan). */
CREATE OR ALTER FUNCTION dbo.fn_PhongConLai (@MaPhong int, @In date, @Out date)
RETURNS int
AS
BEGIN
    DECLARE @n int = DATEDIFF(day, @In, @Out);
    IF @n < 1 RETURN 0;
    IF @n > 90 SET @n = 90;
    DECLARE @r int;

    SELECT @r = MIN(p.SoLuong - ISNULL(b.Sl, 0) - ISNULL(m.Sl, 0))
    FROM (SELECT TOP (@n) DATEADD(day, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1, @In) AS Ngay
          FROM sys.all_columns) d
    JOIN dbo.Phong p ON p.MaPhong = @MaPhong
    OUTER APPLY (
        SELECT SUM(ct.SoLuong) AS Sl
        FROM dbo.ChiTietDatPhong ct
        JOIN dbo.DonDatPhong o ON o.MaDon = ct.MaDon
        WHERE ct.MaPhong = p.MaPhong
          AND o.NgayNhanPhong <= d.Ngay AND o.NgayTraPhong > d.Ngay
          AND o.TrangThai NOT IN (N'Đã hủy', N'Hủy', N'Hết hạn', N'Cancelled', N'Canceled')
          AND NOT (o.TrangThai = N'Chờ thanh toán' AND o.NgayHetHan IS NOT NULL AND o.NgayHetHan < GETDATE())
    ) b
    OUTER APPLY (
        SELECT SUM(x.SoLuong) AS Sl
        FROM dbo.PhongNgungBan x
        WHERE x.MaPhong = p.MaPhong AND x.TuNgay <= d.Ngay AND x.DenNgay > d.Ngay
    ) m;

    RETURN CASE WHEN ISNULL(@r, 0) < 0 THEN 0 ELSE ISNULL(@r, 0) END;
END
