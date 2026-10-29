# VISUAL_QA_REPORT

## 1. Môi trường
- Website chạy thật: IIS Express, `http://localhost:49688`, database `TravelHubDB` (dữ liệu mẫu), VNPay ở chế độ test cục bộ (secret giả), email ghi ra thư mục.
- Trình duyệt: Chrome (tiện ích Claude-in-Chrome), screenshot chụp từ trình duyệt thật. Desktop: cửa sổ 1440 × 1000 (vùng chụp 1536 × 730).
- **Mobile**: tiện ích không ép được viewport hẹp (cửa sổ tối thiểu ~500 px), nên mobile được kiểm tra bằng **trang khung tạm** nhúng website trong `<iframe>` rộng đúng **390 × 844** (media query tính theo viewport iframe, giống thiết bị thật). Trang khung đã xóa sau khi kiểm tra. Đây là mô phỏng viewport, **không phải thiết bị thật** (không kiểm tra cảm ứng, bàn phím ảo, thanh địa chỉ co giãn).
- Không dùng Playwright (không cài thêm công cụ; công cụ trình duyệt có sẵn đủ dùng).
- Tài khoản đăng nhập thử: tài khoản Admin dev tạo bằng biến môi trường; đã vô hiệu hóa sau kiểm thử. Dữ liệu thử đã xóa.

## 2. Ảnh hero — điều kiện chưa đạt hoàn toàn
Yêu cầu: góc nhìn **từ trên cao xuống** đường băng, máy bay cất cánh. **Không đạt đúng góc nhìn**: không tìm được ảnh miễn phí/có giấy phép đúng cả hai điều kiện (Wikimedia Commons, Pexels, Unsplash đã tìm; ảnh top-down đều là máy bay đỗ ở sân đỗ, ảnh cất cánh đều chụp nghiêng/ngang). Đã chọn ảnh **máy bay đang cất cánh trên đường băng** (Drinu Cutajar / Pexels, hoàng hôn, máy bay là tâm điểm, không phải máy bay đỗ). Ngoài ra tôi đã tạo bằng AI (Canva) một ảnh top-down đúng mô tả (id media Canva `MAHZYNPABDs`, 1680 × 944), nhưng **không tải được bản độ phân giải đầy đủ** về repo (chỉ có thumbnail 200 px) nên chưa dùng. Nếu muốn dùng, tải ảnh đó từ Canva, đặt vào `images/hero/hero-takeoff.jpg` (và `hero-takeoff-mobile.jpg`) — không cần sửa mã. Hero **không** dùng video nền (không có nguồn video phù hợp; ảnh tối ưu 253 KB desktop / 116 KB mobile, có `<link rel="preload">`, không lazy-load).
Ảnh có livery hãng vận chuyển (DHL) — là ảnh bên thứ ba; thay nếu cần tránh gắn thương hiệu.

## 3. URL đã kiểm tra bằng trình duyệt
`/` (desktop + mobile), `/Hotels?sort=price-asc`, `/Hotels?q=zzzz` (trạng thái rỗng), `/Hotels/Details/1` và `/3` (desktop + mobile), `/Account/Login?returnUrl=…` → đăng nhập → `/Booking/Checkout` (+ validation), `/Admin`, `/Admin/Bookings`.
Quét HTTP 30 trang công khai/đăng nhập/Admin: tất cả 200 (booking id sai → 404 đúng kỳ vọng).

## 4. Screenshot (`artifacts/visual-qa/`)
| Tệp | Trang | Viewport |
|---|---|---|
| `01-home-desktop-hero-v1.jpg`, `02-home-desktop-hero-v2.jpg` | Hero trước/sau chỉnh vị trí ảnh | 1440 |
| `03-search-panel-guests-open.jpg` | Search panel: đã chọn Đà Nẵng, ngày, popover khách & phòng | 1440 |
| `04`–`08-home-desktop-*.jpg` | Điểm đến (editorial), chỗ nghỉ, trải nghiệm, CTA cuối, footer | 1440 |
| `09-search-results-desktop.jpg`, `20-search-empty-state.jpg` | Kết quả tìm kiếm, trạng thái không có kết quả | 1440 |
| `10`–`13-home-mobile-*.jpg` | Hero mobile, menu mobile, điểm đến, chỗ nghỉ (trước khi sửa tỉ lệ ảnh) | 390 |
| `14-checkout-desktop.jpg`, `15-checkout-validation.jpg` | Checkout, thông báo lỗi “Vui lòng nhập số điện thoại” | 1440 |
| `16-admin-dashboard-v1.jpg`, `17-admin-bookings.jpg` | Admin (dashboard bản trước khi sửa thương hiệu/biểu đồ rỗng; danh sách booking) | 1440 |
| `18`, `19-hotel-details-*.jpg` | Chi tiết chỗ nghỉ: gallery, phòng, chính sách, khung liên hệ | 1440 |
| `21`, `22-mobile-hotel-details-*.jpg` | Chi tiết chỗ nghỉ trên mobile (sau khi gallery thích ứng) | 390 |

Chưa chụp lại sau chỉnh sửa cuối: dashboard Admin (thương hiệu mới + trạng thái biểu đồ rỗng đã hiển thị trong ảnh 17 ở phần sidebar, nhưng biểu đồ rỗng chưa chụp lại), trang chủ mobile phần “chỗ nghỉ” sau khi đổi tỉ lệ ảnh 4:3, màn hình footer sau khi sửa nút “Đăng ký”.

## 5. Lỗi trực quan phát hiện → xử lý
| # | Lỗi | Xử lý |
|---|---|---|
| 1 | Tiếng Việt hiển thị sai dấu trên view (thiếu `fileEncoding`) — đã sửa ở lượt trước | giữ nguyên |
| 2 | Hero: máy bay bị crop mất/che bởi search panel khi viewport thấp | đổi `background-position` 90% 58%, panel hẹp lại |
| 3 | Mobile: ảnh hero không thấy máy bay; panel che hết | tạo ảnh hero dọc riêng (kéo giãn nền bên dưới để máy bay nằm nửa trên), ẩn đoạn mô tả, panel gọn 2 cột |
| 4 | Mobile: hero bị ghi đè bởi rule cũ `min-height:560px` | xóa rule cũ |
| 5 | Chọn gợi ý điểm đến bằng chuột bị mất giá trị (focus loop) | sửa JS (không reset id khi focus, bỏ qua click trong popover) |
| 6 | Tìm “Da Nang” không ra “Đà Nẵng” (Đ ≠ D trong collation) | chuẩn hóa Đ→D ở truy vấn |
| 7 | Logo footer có khoảng trắng “Travel Hub”; icon logo header đặc khi cuộn | sửa markup/CSS thương hiệu |
| 8 | Bài viết Đà Lạt crop gần như toàn trời | `object-position` thấp hơn |
| 9 | Ảnh bìa “Dalat Garden” (motel xe hơi) kém | đổi ảnh bìa sang ảnh sảnh khách sạn khác (dữ liệu dev) |
| 10 | CTA cuối: tiêu đề sát đoạn mô tả; nút “Đăng ký” footer xuống dòng | thêm margin, `nowrap` |
| 11 | Gallery chi tiết có ô trống khi < 5 ảnh | lưới thích ứng theo số ảnh |
| 12 | Lỗi console `TypeError` ở sự kiện `pageshow` (do tôi gõ sai `$$`) | sửa; console sau đó chỉ còn lỗi từ **tiện ích trình duyệt**, không từ website |
| 13 | Admin: biểu đồ doanh thu rỗng trông như lỗi; logo thiếu | thêm trạng thái rỗng + logo |

## 6. Responsive
Đã kiểm tra mobile 390 px: header + menu thu gọn, hero, search panel (xếp 2 cột, popover khách), lưới điểm đến, thẻ chỗ nghỉ, chi tiết chỗ nghỉ (gallery, thẻ phòng xếp dọc). **Chưa kiểm tra**: tablet (768–1024 px), 360 px, landscape, checkout/Admin ở mobile (CSS có media query nhưng chưa xem), bộ lọc trang kết quả dạng drawer (hiện bộ lọc xếp phía trên danh sách trên mobile, chưa phải drawer).

## 7. Tài nguyên & console
- Không có request ảnh/CSS/JS lỗi trong các trang đã mở; console chỉ có `ChunkLoadError` từ tiện ích Chrome (ngoài phạm vi website).
- Font Plus Jakarta Sans tải từ Google Fonts (cần internet; offline rơi về Segoe UI). Bundle CSS/JS dùng URL có hash nội dung (cache an toàn).
- Chưa đo hiệu năng (Lighthouse), chưa dùng WebP/AVIF (System.Drawing không xuất được; ảnh JPEG tối ưu).

## 8. Tương tác & nghiệp vụ sau khi đổi giao diện
Đã thao tác thật trên trình duyệt: gõ “da n” → gợi ý (điểm đến + chỗ nghỉ), chọn bằng chuột và bàn phím, nhập ngày (ngày trả tự đẩy lên sau ngày nhận), tăng số khách, bấm Tìm → `/hotels?q=…&destinationId=1&checkIn=…&adults=3…` → kết quả; đăng nhập có `returnUrl` về Checkout; validation Checkout; Admin. Chạy lại: 61 test tự động **61/61 đạt**; luồng HTTP đăng ký → xác minh → đặt phòng → thanh toán (IPN ký) → **Đã thanh toán** vẫn đạt. Chưa thử bấm yêu thích bằng trình duyệt (chỉ kiểm thử endpoint trước đó).

## 9. Điểm đánh giá (tự đánh giá từ screenshot; bằng chứng = ảnh ở mục 4)
| Tiêu chí | Điểm | Ghi chú |
|---|---|---|
| Mức độ chuyên nghiệp | 8 | Bố cục, typography, nhịp section tốt |
| Chất lượng ảnh hero | 8 | Điện ảnh, nét; **không** top-down |
| Máy bay/đường băng nổi bật | 7 | Máy bay là tâm điểm; đường băng ít thấy do crop rộng |
| Độ dễ đọc tiêu đề | 9 | Overlay trái + chữ trắng/teal |
| Search panel | 8 | Một hàng desktop, popover khách/gợi ý chạy thật |
| Màu & typography | 8 | Navy/teal nhất quán |
| Khoảng cách/hierarchy | 8 | |
| Ảnh phòng/chỗ nghỉ | 6 | Ảnh minh họa từ nguồn mở, chất lượng không đồng đều (vd. motel, tòa nhà ảm đạm) — **dưới mục tiêu 8**; cần ảnh thật của đối tác |
| Nhất quán giữa các trang | 8 | Trang trong dùng cùng header/search band/footer |
| Trải nghiệm mobile | 7 | Chỉ kiểm tra ở 390 px mô phỏng |
| Khả năng đọc/thao tác | 8 | Focus, aria cơ bản |
| Hoàn thiện Admin | 7 | Chức năng đủ, nhưng giao diện còn đơn giản hơn trang công khai |

## 10. Trạng thái nghiệm thu
**Đạt có điều kiện.** Website chạy thật, screenshot thật, hero có máy bay cất cánh, search panel dùng được, desktop + mobile (mô phỏng) đã xem, nghiệp vụ không bị ảnh hưởng. **Chưa đạt/không kiểm tra**: ảnh hero top-down, ảnh phòng chất lượng ≥ 8/10, tablet/thiết bị thật, drawer bộ lọc mobile, hiệu năng, video nền, các màn hình đã nêu ở mục 4 “chưa chụp lại”. Chưa có ảnh tham khảo của bạn trong cuộc trò chuyện nên thiết kế bám theo mô tả bằng chữ, không so được với ảnh mẫu.
---

## 11. Cập nhật lượt 2 — hero minh họa điện ảnh (theo video tham khảo)
Sau khi xem video demo (khung hình trích bằng ffmpeg; không nghe được âm thanh): mẫu là **tranh minh họa phẳng nhìn từ trên xuống**, không phải ảnh chụp. Hero đã làm lại hoàn toàn:
- **Cảnh SVG tự vẽ** (`Views/Shared/_HeroScene.cshtml`): đường băng có vạch tim/vạch ngưỡng/đèn biên chạy nhấp nháy, đường lăn, sân đỗ, nhà ga + cầu hành khách + 3 máy bay đỗ, xe phục vụ chạy trên sân đỗ; **máy bay chở khách nhìn từ trên xuống tăng tốc rồi cất cánh** (phóng to dần, bóng đổ tách xa) lặp 12 giây; ảnh hero cũ và dòng ghi công Pexels đã bỏ.
- **Điện ảnh**: ánh hoàng hôn, vignette, nhiễu hạt phim, chữ tiêu đề trượt lên theo từng dòng, parallax theo chuột và theo cuộn (cảnh trôi + mờ dần), băng chữ outline các điểm đến chạy ngang, số liệu đếm lên (số **thật** từ database), phần "Chỗ nghỉ" nền tối.
- **Chuyển cảnh khi bấm Tìm chỗ nghỉ**: màn hình tối dần, máy bay lướt ngang rồi mới sang trang kết quả (đã thử: sang đúng `/hotels?q=Nha+Trang&destinationId=3…`).
- **Mobile**: cảnh xoay dọc, đường băng chạy lên, máy bay cất cánh hướng lên đầu màn hình (ảnh 26–27).
- `prefers-reduced-motion`: tắt toàn bộ chuyển động (máy bay đứng yên giữa đường băng, chữ hiện ngay). Không dùng Three.js, không thư viện animation; chỉ CSS/SVG + ~60 dòng JS.
- Kết quả: **hết hạn chế "ảnh hero không phải top-down"** ở mục 2 — hero giờ đúng góc nhìn từ trên cao xuống, máy bay cất cánh, nhưng là **minh họa**, không phải ảnh thật.

Chưa kiểm tra lượt này: hiệu năng animation trên máy yếu/di động thật (cảnh SVG có filter blur + 2 lớp đèn glow; nếu giật cần giảm), tablet, trình duyệt khác Chrome (dùng `translate:` CSS cho xe chạy — Safari cũ có thể không chạy, chỉ mất hiệu ứng xe), checkout/Admin không đổi. Ảnh phòng/khách sạn vẫn là điểm yếu (xem mục 9). Ảnh chụp mới: `23`–`29` trong `artifacts/visual-qa/`.
## 12. Nâng cấp menu điều hướng + mở rộng dữ liệu

"Thanh menu" được hiểu là menu điều hướng chính. Đã làm: mega-menu (Chỗ nghỉ / Điểm đến / Trải nghiệm / Khám phá / Hỗ trợ) lấy số liệu thật từ DB (`Services/MenuService.cs`, cache 2 phút, tự xoá khi Admin lưu); chip "Hoàn tất thanh toán" đếm ngược và huy hiệu Yêu thích theo từng người dùng; huy hiệu Admin sidebar (đơn chờ, đối soát, hoàn tiền, liên hệ mới); lọc `/Destinations?style=&region=`; sửa lọc `?stars=` bị bỏ qua và tìm kiếm tiếng Việt Đ/đ.

Đã kiểm tra bằng Chrome (1440px): panel Chỗ nghỉ, Điểm đến, Trải nghiệm; lọc miền Trung; `/Hotels?stars=5` (6 kết quả); chip đếm ngược + huy hiệu yêu thích (dữ liệu QA tạm, đã xoá sau khi kiểm tra); badge Admin. Lỗi bố cục phát hiện khi có chip (chữ menu xuống dòng) đã sửa bằng `white-space:nowrap` và thu gọn chip.
Chưa kiểm tra bằng mắt: panel Khám phá/Hỗ trợ và accordion menu mobile (chỉ build + smoke test URL 200). Ảnh chụp lần này chưa lưu vào `artifacts/visual-qa/`.

## 13. Admin v2
Đã xem bằng Chrome 1536px: dashboard (hero, KPI, donut, chuyến bay sắp bay, top), danh sách chuyến bay, thẻ lên máy bay chi tiết vé, hồ sơ khách 360° (tab Tổng quan/Đặt phòng/Vé máy bay). Phát hiện và sửa: tên thành phố trong dữ liệu chuyến bay bị mất dấu (literal thiếu tiền tố N) và vé cũ của chuyến đã bay vẫn hiển thị "Đã đặt". Chưa kiểm tra bằng mắt: giao diện mobile của Admin, các trang Customers Index/Airlines/Search/Flights Edit (chỉ kiểm tra HTTP 200).
