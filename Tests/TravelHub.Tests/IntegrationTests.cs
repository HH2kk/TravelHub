using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using TravelHub.Data;
using TravelHub.Models;
using TravelHub.Services;
using TravelHub.Services.Payment;
using TravelHub.Services;
using Xunit;

namespace TravelHub.Tests
{
    /// <summary>Chạy trên DB TravelHubDB_Test (app.config). Mỗi test tự tạo khách sạn/phòng/khách riêng để độc lập.</summary>
    public class Fixture
    {
        static int _seq;
        public int Customer, Customer2, Hotel, Room; public string HotelName;
        public DateTime In = DateTime.Today.AddDays(60 + (System.Threading.Interlocked.Increment(ref _seq) * 7));
        public DateTime Out { get { return In.AddDays(2); } }

        public Fixture(int rooms = 1)
        {
            var tag = Guid.NewGuid().ToString("N").Substring(0, 8); HotelName = "TEST-" + tag;
            using (var c = TravelDb.Open())
            {
                Customer = NewCustomer(c, tag + "a"); Customer2 = NewCustomer(c, tag + "b");
                Hotel = c.ExecuteScalar<int>(@"INSERT INTO KhachSan (TenKS, MaDiaDiem, DiaChi, SoSao, GiaTu, TrangThai, NgayTao, ChinhSachHuyLoai) VALUES (@n,1,N'Test',3,1000000,1,GETDATE(),'FLEX1'); SELECT CAST(SCOPE_IDENTITY() AS int);", new { n = "TEST-" + tag });
                Room = c.ExecuteScalar<int>(@"INSERT INTO Phong (MaKS, TenPhong, SoNguoiToiDa, GiaPhong, SoLuong, TrangThai) VALUES (@h,N'Test room',2,1000000,@r,1); SELECT CAST(SCOPE_IDENTITY() AS int);", new { h = Hotel, r = rooms });
            }
        }

        static int NewCustomer(System.Data.IDbConnection c, string tag)
        {
            return c.ExecuteScalar<int>("INSERT INTO KhachHang (HoTen, Email, TrangThai, NgayTao) VALUES (N'Khách test', @e, 1, GETDATE()); SELECT CAST(SCOPE_IDENTITY() AS int);", new { e = tag + "@example.test" });
        }

        public BookingRequest Req(int rooms = 1) { return new BookingRequest { HotelId = Hotel, RoomId = Room, CheckIn = In, CheckOut = Out, Adults = 2, Rooms = rooms, GuestName = "Khách test", GuestEmail = "guest-" + Guid.NewGuid().ToString("N").Substring(0, 6) + "@example.test", GuestPhone = "0900000000", AcceptTerms = true }; }

        public string StartPay(BookingDetail b) { return PaymentService.StartPayment(b.Id, Customer, "VNPAY", "127.0.0.1", "http://localhost/return"); }

        public static string TxnRefOf(int bookingId)
        {
            using (var c = TravelDb.Open()) return c.ExecuteScalar<string>("SELECT TOP 1 MaThamChieuVNPAY FROM ThanhToan WHERE MaDon=@d ORDER BY MaThanhToan DESC", new { d = bookingId });
        }

        public static NameValueCollection Callback(string txn, decimal amountVnd, string rc = "00", string ts = "00")
        {
            return VnPaySignatureTests.Signed(txn, (long)(amountVnd * 100), rc, ts, "TEST-ONLY-SECRET-NOT-REAL");
        }

        public static int EmailCount(string key) { using (var c = TravelDb.Open()) return c.ExecuteScalar<int>("SELECT COUNT(*) FROM EmailOutbox WHERE KhoaTrung=@k", new { k = key }); }
    }

    public class SearchAndAvailabilityTests
    {
        [Fact] public void Invalid_date_ranges_are_rejected()
        {
            Assert.Throws<BookingException>(() => BookingService.ValidateStay(DateTime.Today.AddDays(5), DateTime.Today.AddDays(5)));
            Assert.Throws<BookingException>(() => BookingService.ValidateStay(DateTime.Today.AddDays(6), DateTime.Today.AddDays(5)));
            Assert.Throws<BookingException>(() => BookingService.ValidateStay(DateTime.Today.AddDays(-2), DateTime.Today.AddDays(1)));
            BookingService.ValidateStay(DateTime.Today.AddDays(1), DateTime.Today.AddDays(2));
        }

        [Fact] public void Search_returns_hotel_only_when_a_room_fits_party_and_is_free()
        {
            var f = new Fixture(1);
            var s = new SearchCriteria { Q = f.HotelName, CheckIn = f.In, CheckOut = f.Out, Adults = 2, PageSize = 48 };
            Assert.Contains(HotelSearchService.Search(s).Items, h => h.Id == f.Hotel);
            s.Adults = 5;   // vượt sức chứa (2)
            Assert.DoesNotContain(HotelSearchService.Search(s).Items, h => h.Id == f.Hotel);
        }

        [Fact] public void Room_sold_out_on_a_middle_night_hides_hotel_for_that_stay()
        {
            var f = new Fixture(1);
            var b = BookingService.Create(f.Customer, new BookingRequest { HotelId = f.Hotel, RoomId = f.Room, CheckIn = f.In.AddDays(1), CheckOut = f.In.AddDays(2), Adults = 2, Rooms = 1, GuestName = "A", GuestEmail = "a@example.test", GuestPhone = "0900000000", AcceptTerms = true });
            var s = new SearchCriteria { Q = f.HotelName, CheckIn = f.In, CheckOut = f.In.AddDays(3), Adults = 2, PageSize = 48 };
            Assert.DoesNotContain(HotelSearchService.Search(s).Items, h => h.Id == f.Hotel);
            s.CheckIn = f.In.AddDays(2); s.CheckOut = f.In.AddDays(4);        // ngày trả phòng của đơn trên = ngày nhận mới → còn trống
            Assert.Contains(HotelSearchService.Search(s).Items, h => h.Id == f.Hotel);
        }

        [Fact] public void Capacity_is_enforced_by_booking()
        {
            var f = new Fixture(3); var r = f.Req(); r.Adults = 3;
            Assert.Throws<BookingException>(() => BookingService.Create(f.Customer, r));
        }
    }

    public class OverbookingTests
    {
        [Fact] public void Two_people_booking_the_last_room_at_once_only_one_wins()
        {
            var f = new Fixture(1);
            var results = Enumerable.Range(0, 2).Select(i => Task.Run(() =>
            {
                try { return BookingService.Create(i == 0 ? f.Customer : f.Customer2, f.Req()) != null; } catch (BookingException) { return false; }
            })).ToArray();
            Task.WaitAll(results);
            Assert.Equal(1, results.Count(r => r.Result));
        }

        [Fact] public void Many_concurrent_requests_never_exceed_inventory()
        {
            var f = new Fixture(3);
            var tasks = Enumerable.Range(0, 12).Select(i => Task.Run(() => { try { BookingService.Create(f.Customer, f.Req()); return 1; } catch (BookingException) { return 0; } })).ToArray();
            Task.WaitAll(tasks);
            Assert.Equal(3, tasks.Sum(t => t.Result));
        }

        [Fact] public void Expired_hold_frees_the_room()
        {
            var f = new Fixture(1);
            var b = BookingService.Create(f.Customer, f.Req());
            Assert.Throws<BookingException>(() => BookingService.Create(f.Customer2, f.Req()));
            using (var c = TravelDb.Open()) c.Execute("UPDATE DonDatPhong SET NgayHetHan=DATEADD(minute,-10,GETDATE()) WHERE MaDon=@id", new { id = b.Id });
            BookingService.Create(f.Customer2, f.Req());           // phòng đã được trả lại ngay khi hold quá hạn
            BookingService.ExpireHolds();
            Assert.Equal(BookingStatus.Expired, BookingService.Get(b.Id, f.Customer, false).Status);
        }

        [Fact] public void Cancelled_booking_releases_the_room()
        {
            var f = new Fixture(1);
            var b = BookingService.Create(f.Customer, f.Req());
            BookingService.Cancel(b.Id, f.Customer, false, 0, "test");
            BookingService.Create(f.Customer2, f.Req());
        }

        [Fact] public void Blocked_room_reduces_availability()
        {
            var f = new Fixture(1);
            using (var c = TravelDb.Open()) c.Execute("INSERT INTO PhongNgungBan (MaPhong, TuNgay, DenNgay, SoLuong, LyDo) VALUES (@r,@a,@b,1,N'bảo trì')", new { r = f.Room, a = f.In, b = f.Out });
            Assert.Throws<BookingException>(() => BookingService.Create(f.Customer, f.Req()));
        }
    }

    public class OwnershipTests
    {
        [Fact] public void Customer_cannot_read_or_cancel_someone_elses_booking()
        {
            var f = new Fixture(2);
            var b = BookingService.Create(f.Customer, f.Req());
            Assert.Null(BookingService.Get(b.Id, f.Customer2, false));
            Assert.Throws<BookingException>(() => BookingService.Cancel(b.Id, f.Customer2, false, 0, "x"));
            Assert.NotNull(BookingService.Get(b.Id, f.Customer, false));
        }

        [Fact] public void Customer_cannot_start_payment_for_someone_elses_booking()
        {
            var f = new Fixture(2);
            var b = BookingService.Create(f.Customer, f.Req());
            Assert.Throws<BookingException>(() => PaymentService.StartPayment(b.Id, f.Customer2, "VNPAY", "127.0.0.1", "http://x"));
        }
    }

    public class PaymentFlowTests
    {
        [Fact] public void Valid_success_callback_confirms_booking_once_and_queues_email_and_receipt()
        {
            var f = new Fixture(2); var b = BookingService.Create(f.Customer, f.Req()); f.StartPay(b);
            var txn = Fixture.TxnRefOf(b.Id);
            var o = PaymentService.HandleCallback(Gateways.Get("VNPAY"), Fixture.Callback(txn, b.Total), "IPN");
            Assert.Equal("00", o.RspCode); Assert.True(o.NewlyConfirmed);
            var after = BookingService.Get(b.Id, f.Customer, false);
            Assert.Equal(BookingStatus.Paid, after.Status); Assert.Equal(b.Total, after.PaidAmount);
            Assert.Equal(1, Fixture.EmailCount("BOOKING_CONFIRMED:" + b.Id));
            Assert.NotNull(ReceiptService.LatestForBooking(b.Id));
        }

        [Fact] public void Repeated_callbacks_are_idempotent()
        {
            var f = new Fixture(2); var b = BookingService.Create(f.Customer, f.Req()); f.StartPay(b);
            var txn = Fixture.TxnRefOf(b.Id); var gw = Gateways.Get("VNPAY");
            Assert.True(PaymentService.HandleCallback(gw, Fixture.Callback(txn, b.Total), "IPN").NewlyConfirmed);
            for (int i = 0; i < 3; i++) { var o = PaymentService.HandleCallback(gw, Fixture.Callback(txn, b.Total), i % 2 == 0 ? "RETURN" : "IPN"); Assert.False(o.NewlyConfirmed); Assert.Equal("02", o.RspCode); }
            using (var c = TravelDb.Open())
            {
                Assert.Equal(1, c.ExecuteScalar<int>("SELECT COUNT(*) FROM ThanhToan WHERE MaDon=@d AND TrangThai=N'Thành công'", new { d = b.Id }));
                Assert.Equal(1, c.ExecuteScalar<int>("SELECT COUNT(*) FROM BienNhan WHERE MaDon=@d", new { d = b.Id }));
            }
            Assert.Equal(1, Fixture.EmailCount("BOOKING_CONFIRMED:" + b.Id));
        }

        [Fact] public void Invalid_signature_does_not_confirm()
        {
            var f = new Fixture(2); var b = BookingService.Create(f.Customer, f.Req()); f.StartPay(b);
            var cb = Fixture.Callback(Fixture.TxnRefOf(b.Id), b.Total); cb["vnp_ResponseCode"] = "00"; cb["vnp_SecureHash"] = new string('a', 128);
            var o = PaymentService.HandleCallback(Gateways.Get("VNPAY"), cb, "IPN");
            Assert.Equal("97", o.RspCode); Assert.Equal(BookingStatus.Pending, BookingService.Get(b.Id, f.Customer, false).Status);
        }

        [Fact] public void Amount_mismatch_is_rejected()
        {
            var f = new Fixture(2); var b = BookingService.Create(f.Customer, f.Req()); f.StartPay(b);
            var o = PaymentService.HandleCallback(Gateways.Get("VNPAY"), Fixture.Callback(Fixture.TxnRefOf(b.Id), b.Total - 1000), "IPN");
            Assert.Equal("04", o.RspCode); Assert.Equal(BookingStatus.Pending, BookingService.Get(b.Id, f.Customer, false).Status);
        }

        [Fact] public void Failed_payment_keeps_hold_and_allows_retry()
        {
            var f = new Fixture(2); var b = BookingService.Create(f.Customer, f.Req()); f.StartPay(b);
            PaymentService.HandleCallback(Gateways.Get("VNPAY"), Fixture.Callback(Fixture.TxnRefOf(b.Id), b.Total, "24", "02"), "IPN");
            var d = BookingService.Get(b.Id, f.Customer, false);
            Assert.Equal(BookingStatus.Pending, d.Status); Assert.Equal(0m, d.PaidAmount);
            f.StartPay(b);                                    // thử lại an toàn
            var o = PaymentService.HandleCallback(Gateways.Get("VNPAY"), Fixture.Callback(Fixture.TxnRefOf(b.Id), b.Total), "IPN");
            Assert.True(o.NewlyConfirmed);
        }

        [Fact] public void Unknown_reference_is_reported_not_crashing()
        {
            var o = PaymentService.HandleCallback(Gateways.Get("VNPAY"), Fixture.Callback("DOES-NOT-EXIST", 1000), "IPN");
            Assert.Equal("01", o.RspCode);
        }

        [Fact] public void Late_success_after_expiry_with_room_still_free_restores_booking()
        {
            var f = new Fixture(2); var b = BookingService.Create(f.Customer, f.Req()); f.StartPay(b);
            using (var c = TravelDb.Open()) c.Execute("UPDATE DonDatPhong SET NgayHetHan=DATEADD(minute,-30,GETDATE()) WHERE MaDon=@d", new { d = b.Id });
            BookingService.ExpireHolds();
            Assert.Equal(BookingStatus.Expired, BookingService.Get(b.Id, f.Customer, false).Status);
            var o = PaymentService.HandleCallback(Gateways.Get("VNPAY"), Fixture.Callback(Fixture.TxnRefOf(b.Id), b.Total), "IPN");
            Assert.True(o.NewlyConfirmed); Assert.Equal(BookingStatus.Paid, BookingService.Get(b.Id, f.Customer, false).Status);
        }

        [Fact] public void Late_success_when_room_was_resold_goes_to_refund_not_double_booking()
        {
            var f = new Fixture(1); var b = BookingService.Create(f.Customer, f.Req()); f.StartPay(b);
            using (var c = TravelDb.Open()) c.Execute("UPDATE DonDatPhong SET NgayHetHan=DATEADD(minute,-30,GETDATE()) WHERE MaDon=@d", new { d = b.Id });
            BookingService.ExpireHolds();
            var other = BookingService.Create(f.Customer2, f.Req());         // người khác mua phòng cuối
            var o = PaymentService.HandleCallback(Gateways.Get("VNPAY"), Fixture.Callback(Fixture.TxnRefOf(b.Id), b.Total), "IPN");
            Assert.False(o.NewlyConfirmed);
            Assert.NotEqual(BookingStatus.Paid, BookingService.Get(b.Id, f.Customer, false).Status);
            using (var c = TravelDb.Open())
            {
                Assert.Equal(1, c.ExecuteScalar<int>("SELECT COUNT(*) FROM YeuCauHoanTien WHERE MaDon=@d AND TrangThai='PENDING'", new { d = b.Id }));
                Assert.Equal(1, c.ExecuteScalar<int>("SELECT COUNT(*) FROM DonDatPhong WHERE MaKS=@h AND TrangThai IN (N'Chờ thanh toán', N'Đã thanh toán')", new { h = f.Hotel }));
            }
        }

        [Fact] public void Superseded_attempt_succeeding_late_does_not_confirm_twice()
        {
            var f = new Fixture(2); var b = BookingService.Create(f.Customer, f.Req());
            f.StartPay(b); var first = Fixture.TxnRefOf(b.Id);
            f.StartPay(b); var second = Fixture.TxnRefOf(b.Id);
            var gw = Gateways.Get("VNPAY");
            Assert.True(PaymentService.HandleCallback(gw, Fixture.Callback(second, b.Total), "IPN").NewlyConfirmed);
            var late = PaymentService.HandleCallback(gw, Fixture.Callback(first, b.Total), "IPN");
            Assert.False(late.NewlyConfirmed);
            using (var c = TravelDb.Open())
            {
                Assert.Equal(1, c.ExecuteScalar<int>("SELECT COUNT(*) FROM ThanhToan WHERE MaDon=@d AND TrangThai=N'Thành công'", new { d = b.Id }));
                Assert.Equal(1, c.ExecuteScalar<int>("SELECT COUNT(*) FROM YeuCauHoanTien WHERE MaDon=@d", new { d = b.Id }));  // khoản trùng được xếp hàng hoàn
            }
        }

        [Fact] public void Concurrent_duplicate_callbacks_confirm_exactly_once()
        {
            var f = new Fixture(2); var b = BookingService.Create(f.Customer, f.Req()); f.StartPay(b);
            var txn = Fixture.TxnRefOf(b.Id); var gw = Gateways.Get("VNPAY");
            var t = Enumerable.Range(0, 5).Select(_ => Task.Run(() => PaymentService.HandleCallback(gw, Fixture.Callback(txn, b.Total), "IPN").NewlyConfirmed)).ToArray();
            Task.WaitAll(t);
            Assert.Equal(1, t.Count(x => x.Result));
            Assert.Equal(1, Fixture.EmailCount("BOOKING_CONFIRMED:" + b.Id));
        }
    }

    public class RefundTests
    {
        [Fact] public void Cancelling_a_paid_booking_early_creates_full_refund_request()
        {
            var f = new Fixture(2); var b = BookingService.Create(f.Customer, f.Req()); f.StartPay(b);
            PaymentService.HandleCallback(Gateways.Get("VNPAY"), Fixture.Callback(Fixture.TxnRefOf(b.Id), b.Total), "IPN");
            var r = BookingService.Cancel(b.Id, f.Customer, false, 0, "đổi kế hoạch");
            Assert.Equal(b.Total, r.RefundAmount);
            var d = BookingService.Get(b.Id, f.Customer, false);
            Assert.Equal(BookingStatus.Cancelled, d.Status); Assert.Equal("PENDING", d.Refund.TrangThai);
        }

        [Fact] public void Cancelling_twice_does_not_create_a_second_refund()
        {
            var f = new Fixture(2); var b = BookingService.Create(f.Customer, f.Req()); f.StartPay(b);
            PaymentService.HandleCallback(Gateways.Get("VNPAY"), Fixture.Callback(Fixture.TxnRefOf(b.Id), b.Total), "IPN");
            BookingService.Cancel(b.Id, f.Customer, false, 0, "x");
            Assert.Throws<BookingException>(() => BookingService.Cancel(b.Id, f.Customer, false, 0, "x"));
            using (var c = TravelDb.Open()) Assert.Equal(1, c.ExecuteScalar<int>("SELECT COUNT(*) FROM YeuCauHoanTien WHERE MaDon=@d", new { d = b.Id }));
        }

        [Fact] public void Unpaid_booking_cancel_needs_no_refund()
        {
            var f = new Fixture(2); var b = BookingService.Create(f.Customer, f.Req());
            Assert.Equal(0m, BookingService.Cancel(b.Id, f.Customer, false, 0, "x").RefundAmount);
        }

        [Fact] public void Completed_booking_cannot_be_cancelled()
        {
            var f = new Fixture(2); var b = BookingService.Create(f.Customer, f.Req());
            using (var c = TravelDb.Open()) c.Execute("UPDATE DonDatPhong SET TrangThai=N'Hoàn thành' WHERE MaDon=@d", new { d = b.Id });
            Assert.Throws<BookingException>(() => BookingService.Cancel(b.Id, f.Customer, false, 0, "x"));
        }
    }

    public class ReceiptAndEmailTests
    {
        [Fact] public void Receipt_pdf_is_generated_with_vietnamese_text_from_stored_snapshot()
        {
            var f = new Fixture(2); var b = BookingService.Create(f.Customer, f.Req()); f.StartPay(b);
            PaymentService.HandleCallback(Gateways.Get("VNPAY"), Fixture.Callback(Fixture.TxnRefOf(b.Id), b.Total), "IPN");
            var no = ReceiptService.LatestForBooking(b.Id);
            var pdf = ReceiptService.RenderPdfBySoBienNhan(no);
            Assert.True(pdf.Length > 3000); Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(pdf, 0, 4));
            // Snapshot bất biến: đổi giá phòng sau đó không làm thay đổi biên nhận đã phát hành.
            int bid; var before = ReceiptService.Load(no, out bid).Total;
            using (var c = TravelDb.Open()) c.Execute("UPDATE Phong SET GiaPhong=GiaPhong*2 WHERE MaPhong=@r", new { r = f.Room });
            Assert.Equal(before, ReceiptService.Load(no, out bid).Total);
        }

        [Fact] public void Receipt_is_created_once_per_payment()
        {
            var f = new Fixture(2); var b = BookingService.Create(f.Customer, f.Req()); f.StartPay(b);
            PaymentService.HandleCallback(Gateways.Get("VNPAY"), Fixture.Callback(Fixture.TxnRefOf(b.Id), b.Total), "IPN");
            using (var c = TravelDb.Open())
            {
                var pid = c.ExecuteScalar<int>("SELECT MaThanhToan FROM ThanhToan WHERE MaDon=@d AND TrangThai=N'Thành công'", new { d = b.Id });
                Assert.Equal(ReceiptService.Create(b.Id, pid), ReceiptService.Create(b.Id, pid));
            }
        }

        [Fact] public void Enqueue_deduplicates_by_key_and_sends_to_pickup_directory()
        {
            var key = "TEST:" + Guid.NewGuid().ToString("N");
            Assert.True(EmailService.Enqueue("TEST", key, "x@example.test", "Tiêu đề tiếng Việt", "<p>Xin chào</p>"));
            Assert.False(EmailService.Enqueue("TEST", key, "x@example.test", "Tiêu đề tiếng Việt", "<p>Xin chào</p>"));
            var dir = @"C:\Temp\th_test_mail"; Directory.CreateDirectory(dir);
            for (int i = 0; i < 100; i++) { EmailService.ProcessPending(); using (var cc = TravelDb.Open()) if (cc.ExecuteScalar<int>("SELECT COUNT(*) FROM EmailOutbox WHERE TrangThai='PENDING' AND SoLanThu < 5") == 0) break; }
            using (var c = TravelDb.Open()) Assert.Equal("SENT", c.ExecuteScalar<string>("SELECT TrangThai FROM EmailOutbox WHERE KhoaTrung=@k", new { k = key }));
        }

        [Fact] public void Email_template_encodes_user_input()
        {
            var html = EmailTemplates.Render("verify-email", new System.Collections.Generic.Dictionary<string, string> { { "Name", "<script>alert(1)</script>" }, { "ButtonUrl", "http://x" } });
            Assert.DoesNotContain("<script>alert(1)</script>", html); Assert.Contains("&lt;script&gt;", html);
        }
    }

    public class AccountTests
    {
        [Fact] public void Register_verify_login_reset_flow()
        {
            var svc = new AccountService(); var tag = Guid.NewGuid().ToString("N").Substring(0, 8);
            var m = new RegisterViewModel { UserName = "u" + tag, FullName = "Người Dùng", Email = tag + "@example.test", Password = "Passw0rd!x", ConfirmPassword = "Passw0rd!x" };
            Assert.Null(svc.Register(m));
            Assert.NotNull(svc.Register(m));                                  // trùng
            var login = svc.Login(m.UserName, m.Password);
            Assert.NotNull(login.Session); Assert.False(login.Session.EmailConfirmed);
            Assert.Null(svc.Login(m.UserName, "sai").Session);
            // Xác thực email bằng token hợp lệ (token gốc chỉ có trong email → lấy từ outbox)
            string token;
            using (var c = TravelDb.Open())
            {
                var html = c.ExecuteScalar<string>("SELECT TOP 1 NoiDungHtml FROM EmailOutbox WHERE Den=@e AND Loai='VERIFY_EMAIL' ORDER BY MaEmail DESC", new { e = m.Email });
                token = System.Net.WebUtility.UrlDecode(System.Text.RegularExpressions.Regex.Match(System.Net.WebUtility.HtmlDecode(html), "token=([^\"&]+)").Groups[1].Value);
            }
            Assert.False(svc.ConfirmEmail(m.Email, "token-sai"));
            Assert.True(svc.ConfirmEmail(m.Email, token));
            Assert.True(svc.Login(m.UserName, m.Password).Session.EmailConfirmed);
            Assert.False(svc.ConfirmEmail(m.Email, token));                   // dùng một lần
            Assert.False(svc.ResetPassword(m.Email, "bogus", "NewPassw0rd!"));
        }

        [Fact] public void Repeated_wrong_passwords_lock_the_account()
        {
            var svc = new AccountService(); var tag = Guid.NewGuid().ToString("N").Substring(0, 8);
            svc.Register(new RegisterViewModel { UserName = "l" + tag, FullName = "X", Email = tag + "@example.test", Password = "Passw0rd!x", ConfirmPassword = "Passw0rd!x" });
            for (int i = 0; i < 5; i++) svc.Login("l" + tag, "wrong");
            var r = svc.Login("l" + tag, "Passw0rd!x");
            Assert.Null(r.Session); Assert.Contains("tạm khóa", r.Error);
        }

        [Fact] public void Plaintext_passwords_are_never_accepted()
        {
            Assert.False(TravelHub.Data.PasswordHasher.Verify("secret", "secret"));
            Assert.True(TravelHub.Data.PasswordHasher.Verify(TravelHub.Data.PasswordHasher.Hash("secret"), "secret"));
        }
    }

    public class MenuAndDataTests
    {
        [Fact] public void Megamenu_lists_only_real_data_with_positive_counts()
        {
            MenuService.Invalidate(); var m = MenuService.Get();
            Assert.NotEmpty(m.Destinations); Assert.NotEmpty(m.PropertyTypes); Assert.True(m.TotalHotels > 0);
            Assert.All(m.Styles, s => Assert.True(s.Count > 0)); Assert.All(m.Regions, r => Assert.True(r.Count > 0));
            Assert.All(m.PropertyTypes, p => Assert.True(p.Count > 0)); Assert.True(m.MinPrice > 0 && m.MaxPrice >= m.MinPrice);
            using (var c = TravelDb.Open()) Assert.True(m.TotalHotels <= c.ExecuteScalar<int>("SELECT COUNT(*) FROM KhachSan WHERE TrangThai=1") && m.TotalHotels >= c.ExecuteScalar<int>("SELECT COUNT(*) FROM KhachSan WHERE TrangThai=1 AND TenKS NOT LIKE 'TEST-%'"));
        }

        [Fact] public void Menu_promotions_are_only_active_ones()
        {
            var m = MenuService.Get();
            using (var c = TravelDb.Open())
                foreach (var p in m.Promotions)
                    Assert.Equal(1, c.ExecuteScalar<int>("SELECT COUNT(*) FROM KhuyenMai WHERE MaKM=@id AND TrangThai=1 AND (NgayKetThuc IS NULL OR NgayKetThuc>=GETDATE()) AND (NgayBatDau IS NULL OR NgayBatDau<=GETDATE())", new { id = p.Id }));
        }

        [Fact] public void User_menu_shows_pending_payment_with_countdown_then_hides_after_cancel()
        {
            var f = new Fixture(2); var b = BookingService.Create(f.Customer, f.Req());
            var um = MenuService.ForUser(f.Customer);
            Assert.Equal(1, um.PendingCount); Assert.Equal(b.Id, um.PendingBookingId); Assert.True(um.PendingExpires > DateTime.Now);
            BookingService.Cancel(b.Id, f.Customer, false, 0, "test");
            Assert.Equal(0, MenuService.ForUser(f.Customer).PendingCount);
        }

        [Fact] public void User_menu_counts_upcoming_trips_only_for_paid_bookings()
        {
            var f = new Fixture(2); var b = BookingService.Create(f.Customer, f.Req());
            Assert.Equal(0, MenuService.ForUser(f.Customer).UpcomingTrips);
            using (var c = TravelDb.Open()) c.Execute("UPDATE DonDatPhong SET TrangThai=N'Đã thanh toán', NgayHetHan=NULL WHERE MaDon=@id", new { id = b.Id });
            var um = MenuService.ForUser(f.Customer); Assert.Equal(1, um.UpcomingTrips); Assert.Equal(b.Id, um.NextTripId);
        }

        [Fact] public void Every_open_hotel_has_rooms_images_and_a_destination_and_images_exist_on_disk()
        {
            using (var c = TravelDb.Open())
            {
                var bad = c.Query<string>(@"SELECT k.TenKS FROM KhachSan k WHERE k.TrangThai=1 AND k.TenKS NOT LIKE 'TEST-%' AND (NOT EXISTS (SELECT 1 FROM Phong p WHERE p.MaKS=k.MaKS AND p.TrangThai=1) OR NOT EXISTS (SELECT 1 FROM HinhAnhKhachSan i WHERE i.MaKS=k.MaKS) OR k.HinhAnhDaiDien IS NULL)").ToList();
                Assert.Empty(bad);
                var root = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\.."));
                var paths = c.Query<string>(@"SELECT HinhAnh FROM DiaDiem UNION SELECT HinhAnhDaiDien FROM KhachSan WHERE TenKS NOT LIKE 'TEST-%' UNION SELECT DuongDanAnh FROM HinhAnhKhachSan WHERE MaKS IN (SELECT MaKS FROM KhachSan WHERE TenKS NOT LIKE 'TEST-%') UNION SELECT HinhAnh FROM BaiViet UNION SELECT HinhAnh FROM Phong WHERE MaKS IN (SELECT MaKS FROM KhachSan WHERE TenKS NOT LIKE 'TEST-%')").Where(p => p != null).ToList();
                var missing = paths.Where(p => !System.IO.File.Exists(System.IO.Path.Combine(root, p.TrimStart('/').Replace('/', '\\')))).ToList();
                Assert.Empty(missing);
            }
        }
    }
}