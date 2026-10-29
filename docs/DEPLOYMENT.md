# DEPLOYMENT

1. **Máy chủ:** Windows Server + IIS, .NET Framework 4.8.1, ASP.NET 4.8, SQL Server. App pool .NET CLR v4, Integrated pipeline; identity có quyền đọc/ghi DB (hoặc dùng SQL login trong connection string) và ghi `Content\images\**\uploads`, `Content\images\avatars`.
2. **Build:** `MSBuild TravelHub.csproj /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=...` hoặc Publish từ Visual Studio. Đặt `<compilation debug="false">`, `customErrors mode="On"` trong `Web.config` (transform `Web.Release.config`).
3. **Database:** BACKUP → chạy migration 001–004 theo thứ tự (cờ `-I`). Chỉ chạy trên môi trường đã backup.
4. **Cấu hình secret** bằng biến môi trường của app pool/`App_Data\secrets.config` (ngoài thư mục public, quyền chỉ app pool đọc). `App:BaseUrl` = URL HTTPS công khai.
5. **HTTPS:** bắt buộc; đặt `<forms requireSSL="true">`, cân nhắc HSTS. Đăng ký URL VNPay: Return `https://domain/Payment/VnPayReturn`, IPN `https://domain/Payment/VnPayIpn` (IIS phải truy cập được từ internet).
6. **Admin đầu tiên:** biến `TRAVELHUB_ADMIN__*` (README mục 4), xóa mật khẩu sau lần chạy đầu.
7. **Email:** cấu hình SMTP thật, kiểm tra gửi thử (Admin → tạo tài khoản mới → nhận email xác minh).
8. **Tác vụ nền** chạy trong worker process; bật *Always Running*/`startMode=AlwaysRunning` và tắt Idle Timeout để email/hết hạn giữ phòng không bị dừng khi ít truy cập. Nếu chạy nhiều instance: hết hạn/outbox có thể chạy song song — các thao tác đều có khóa/điều kiện nên an toàn, nhưng cần cân nhắc scale.
9. **Sao lưu:** lịch backup DB định kỳ; sao lưu thư mục ảnh upload.
10. **Kiểm tra sau triển khai:** `docs/TEST_REPORT.md` (smoke test), thanh toán sandbox bằng thẻ test, xem Admin → Nhật ký.