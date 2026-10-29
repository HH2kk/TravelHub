using System;
using System.Linq;
using System.Reflection;
using Dapper;
using TravelHub.Data;
using TravelHub.Services;
using Xunit;

namespace TravelHub.Tests
{
    /// <summary>Nghiệp vụ trang Admin: vé máy bay, chuyến bay, hồ sơ khách 360°, dashboard.</summary>
    public class AdminFlightTests
    {
        static int _seat;

        static int NewFlight(System.Data.IDbConnection c, DateTime dep, int seats = 100, int left = 90, string status = "Scheduled")
        {
            var code = "T" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            return c.ExecuteScalar<int>(@"INSERT ChuyenBay (MaHangBay, MaChuyen, SanBayDi, SanBayDen, ThoiGianDi, ThoiGianDen, GiaVe, SoGhe, SoGheConLai, TrangThai, MaSanBayDi, MaSanBayDen, TrangThaiChuyen)
                OUTPUT INSERTED.MaChuyenBay VALUES (1, @c, N'Hà Nội', N'Đà Nẵng', @d, DATEADD(minute,90,@d), 1000000, @s, @l, 1, 'HAN', 'DAD', @st)", new { c = code, d = dep, s = seats, l = left, st = status });
        }

        static int NewTicket(System.Data.IDbConnection c, int customer, int flight, decimal price = 1000000, string code = "Booked", string text = "Đã đặt")
        {
            return c.ExecuteScalar<int>(@"INSERT VeMayBay (MaKH, MaChuyenBay, HoTenHanhKhach, SoGiayTo, SoGhe, GiaVe, TrangThai, NgayDat, TrangThaiCode, MaVeCode, HangGhe)
                OUTPUT INSERTED.MaVe VALUES (@k, @f, N'Hành khách test', '0123', @seat, @p, @t, GETDATE(), @code, @mv, N'Phổ thông')", new { k = customer, f = flight, p = price, t = text, code, seat = (System.Threading.Interlocked.Increment(ref _seat) % 90 + 1) + "A", mv = "TT" + Guid.NewGuid().ToString("N").Substring(0, 10) });
        }

        [Fact] public void Cancelling_a_ticket_returns_the_seat_once()
        {
            var f = new Fixture(1);
            using (var c = TravelDb.Open())
            {
                var fl = NewFlight(c, DateTime.Now.AddDays(5)); var t = NewTicket(c, f.Customer, fl);
                Assert.True(FlightAdmin.CancelTicket(t, "test"));
                Assert.Equal(91, c.ExecuteScalar<int>("SELECT SoGheConLai FROM ChuyenBay WHERE MaChuyenBay=@fl", new { fl }));
                Assert.False(FlightAdmin.CancelTicket(t, "again"));
                Assert.Equal(91, c.ExecuteScalar<int>("SELECT SoGheConLai FROM ChuyenBay WHERE MaChuyenBay=@fl", new { fl }));
                Assert.Equal("Cancelled", FlightAdmin.Ticket(t).StatusCode);
                Assert.Equal("test", FlightAdmin.Ticket(t).CancelReason);
            }
        }

        [Fact] public void Seat_is_not_returned_when_the_flight_already_departed()
        {
            var f = new Fixture(1);
            using (var c = TravelDb.Open())
            {
                var fl = NewFlight(c, DateTime.Now.AddDays(-2)); var t = NewTicket(c, f.Customer, fl);
                Assert.True(FlightAdmin.CancelTicket(t, "no show"));
                Assert.Equal(90, c.ExecuteScalar<int>("SELECT SoGheConLai FROM ChuyenBay WHERE MaChuyenBay=@fl", new { fl }));
            }
        }

        [Fact] public void Completed_ticket_cannot_be_cancelled_and_checkin_needs_booked_state()
        {
            var f = new Fixture(1);
            using (var c = TravelDb.Open())
            {
                var fl = NewFlight(c, DateTime.Now.AddDays(3)); var done = NewTicket(c, f.Customer, fl, 1, "Completed", "Đã bay"); var open = NewTicket(c, f.Customer, fl);
                Assert.False(FlightAdmin.CancelTicket(done, "x")); Assert.False(FlightAdmin.CheckIn(done));
                Assert.True(FlightAdmin.CheckIn(open)); Assert.False(FlightAdmin.CheckIn(open));
                Assert.Equal("CheckedIn", FlightAdmin.Ticket(open).StatusCode);
            }
        }

        [Fact] public void Cancelling_a_flight_cancels_active_tickets_only()
        {
            var f = new Fixture(1);
            using (var c = TravelDb.Open())
            {
                var fl = NewFlight(c, DateTime.Now.AddDays(4));
                var a = NewTicket(c, f.Customer, fl); var b = NewTicket(c, f.Customer2, fl); var done = NewTicket(c, f.Customer, fl, 1, "Completed", "Đã bay");
                var n = FlightAdmin.SetFlightStatus(fl, "Cancelled", "Thời tiết xấu");
                Assert.Equal(2, n);
                Assert.Equal("Cancelled", FlightAdmin.Flight(fl).Status);
                Assert.Equal("Cancelled", FlightAdmin.Ticket(a).StatusCode); Assert.Equal("Cancelled", FlightAdmin.Ticket(b).StatusCode);
                Assert.Equal("Completed", FlightAdmin.Ticket(done).StatusCode);
                Assert.Equal("Thời tiết xấu", FlightAdmin.Ticket(a).CancelReason);
            }
        }

        [Fact] public void Invalid_flight_status_is_rejected()
        {
            Assert.Throws<ArgumentException>(() => FlightAdmin.SetFlightStatus(1, "Exploded", null));
        }

        [Fact] public void Lifecycle_sync_marks_landed_flights_and_flown_tickets()
        {
            var f = new Fixture(1);
            using (var c = TravelDb.Open())
            {
                var fl = NewFlight(c, DateTime.Now.AddDays(-3)); var t = NewTicket(c, f.Customer, fl);
                typeof(FlightAdmin).GetField("_lastSync", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, DateTime.MinValue);
                FlightAdmin.SyncLifecycle();
                Assert.Equal("Arrived", FlightAdmin.Flight(fl).Status);
                Assert.Equal("Completed", FlightAdmin.Ticket(t).StatusCode);
            }
        }

        [Fact] public void Ticket_list_filters_by_customer_and_status()
        {
            var f = new Fixture(1);
            using (var c = TravelDb.Open())
            {
                var fl = NewFlight(c, DateTime.Now.AddDays(6)); NewTicket(c, f.Customer, fl); NewTicket(c, f.Customer, fl, 1, "Cancelled", "Đã hủy"); NewTicket(c, f.Customer2, fl);
                int total;
                var mine = FlightAdmin.Tickets(new TicketFilter { customer = f.Customer }, out total, 0, 50);
                Assert.Equal(2, total); Assert.All(mine, t => Assert.Equal(f.Customer, t.CustomerId));
                FlightAdmin.Tickets(new TicketFilter { customer = f.Customer, status = "Cancelled" }, out total, 0, 50); Assert.Equal(1, total);
                Assert.Equal(3, FlightAdmin.TicketsOfFlight(fl).Count);
            }
        }
    }

    public class CustomerAdminTests
    {
        [Fact] public void Customer360_aggregates_bookings_tickets_and_timeline_for_that_customer_only()
        {
            var f = new Fixture(1); var b = BookingService.Create(f.Customer, f.Req());
            using (var c = TravelDb.Open())
            {
                var fl = c.ExecuteScalar<int>(@"INSERT ChuyenBay (MaHangBay, MaChuyen, SanBayDi, SanBayDen, ThoiGianDi, ThoiGianDen, GiaVe, SoGhe, SoGheConLai, TrangThai, MaSanBayDi, MaSanBayDen) OUTPUT INSERTED.MaChuyenBay VALUES (1, @c, N'A', N'B', DATEADD(day,9,GETDATE()), DATEADD(day,10,GETDATE()), 500000, 50, 40, 1, 'HAN', 'SGN')", new { c = "T" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper() });
                c.Execute(@"INSERT VeMayBay (MaKH, MaChuyenBay, HoTenHanhKhach, SoGiayTo, SoGhe, GiaVe, TrangThai, NgayDat, TrangThaiCode, MaVeCode) VALUES
                    (@k, @f, N'A', '1', '1A', 700000, N'Đã đặt', GETDATE(), 'Booked', @m1), (@k, @f, N'B', '2', '1B', 300000, N'Đã hủy', GETDATE(), 'Cancelled', @m2), (@o, @f, N'C', '3', '1C', 900000, N'Đã đặt', GETDATE(), 'Booked', @m3)",
                    new { k = f.Customer, o = f.Customer2, f = fl, m1 = "T" + Guid.NewGuid().ToString("N").Substring(0, 10), m2 = "T" + Guid.NewGuid().ToString("N").Substring(0, 10), m3 = "T" + Guid.NewGuid().ToString("N").Substring(0, 10) });
            }
            var m = CustomerAdmin.Load(f.Customer);
            Assert.Equal(1, m.BookingCount); Assert.Equal(b.Id, m.Bookings[0].Id); Assert.Equal(f.HotelName, m.Bookings[0].Hotel);
            Assert.Equal(1, m.TicketCount); Assert.Equal(1, m.TicketsCancelled); Assert.Equal(700000m, m.TicketSpend);
            Assert.Equal(0m, m.HotelSpend);   // đơn chưa thanh toán không tính vào chi tiêu
            Assert.Equal(2, m.Tickets.Count);
            Assert.Contains(m.Timeline, e => e.Kind == "hotel"); Assert.Contains(m.Timeline, e => e.Kind == "flight");
            Assert.DoesNotContain(m.Tickets, t => t.CustomerId == f.Customer2);
        }

        [Fact] public void Customer360_returns_null_for_unknown_customer_and_list_supports_every_sort()
        {
            Assert.Null(CustomerAdmin.Load(int.MaxValue));
            int total;
            foreach (var sort in new string[] { null, "spend", "bookings", "new" }) { CustomerAdmin.List(null, sort, 0, 5, out total); Assert.True(total > 0); }
        }

        [Fact] public void Search_text_with_sql_wildcards_is_treated_literally()
        {
            int total; CustomerAdmin.List("%'; DROP TABLE KhachHang;--", null, 0, 5, out total); Assert.Equal(0, total);
            using (var c = TravelDb.Open()) Assert.True(c.ExecuteScalar<int>("SELECT COUNT(*) FROM KhachHang") > 0);
        }
    }

    public class DashboardTests
    {
        [Fact] public void Dashboard_loads_and_series_are_complete_and_consistent()
        {
            foreach (var days in new[] { 7, 30, 90 })
            {
                var d = DashboardService.Load(days);
                Assert.Equal(days, d.DailyRevenue.Count); Assert.Equal(days, d.DailyBookings.Count); Assert.Equal(days, d.DailyTickets.Count);
                Assert.Equal(DateTime.Today, d.DailyRevenue.Last().Key);
                Assert.True(d.Revenue >= -d.Refunded);
                Assert.All(d.Upcoming, fl => Assert.NotEqual("Cancelled", fl.Status));
                Assert.All(d.Todo, t => Assert.True(t.Count > 0));
            }
        }
    }
}
