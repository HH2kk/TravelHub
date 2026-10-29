using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TravelHub.Services;

namespace TravelHub.Models
{
    public class BookingRequest
    {
        public int HotelId { get; set; }
        public int RoomId { get; set; }
        [DataType(DataType.Date)] public DateTime CheckIn { get; set; }
        [DataType(DataType.Date)] public DateTime CheckOut { get; set; }
        [Range(1, 20, ErrorMessage = "Số người lớn 1–20")] public int Adults { get; set; } = 2;
        [Range(0, 20)] public int Children { get; set; }
        [Range(1, 10, ErrorMessage = "Số phòng 1–10")] public int Rooms { get; set; } = 1;
        [Required(ErrorMessage = "Vui lòng nhập họ tên khách lưu trú"), StringLength(100)] public string GuestName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập email"), EmailAddress(ErrorMessage = "Email không hợp lệ"), StringLength(100)] public string GuestEmail { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại"), RegularExpression(@"^[0-9+\s]{8,15}$", ErrorMessage = "Số điện thoại không hợp lệ")] public string GuestPhone { get; set; }
        [StringLength(500)] public string Note { get; set; }
        [StringLength(50)] public string CouponCode { get; set; }
        public bool AcceptTerms { get; set; }
    }

    public class CheckoutViewModel
    {
        public BookingRequest Request { get; set; }
        public HotelCard Hotel { get; set; }
        public RoomItem Room { get; set; }
        public PriceQuote Quote { get; set; }
        public string CancelPolicyCode { get; set; }
        public string CancelPolicyText { get; set; }
        public string CheckInTime { get; set; }
        public string CheckOutTime { get; set; }
        public string CouponMessage { get; set; }
        public bool EmailConfirmed { get; set; }
    }

    public class PaymentRow
    {
        public int MaThanhToan { get; set; }
        public decimal SoTien { get; set; }
        public string PhuongThuc { get; set; }
        public string MaGiaoDich { get; set; }
        public string MaThamChieuVNPAY { get; set; }
        public string TrangThai { get; set; }
        public string TrangThaiCode { get; set; }
        public string LoaiGiaoDich { get; set; }
        public DateTime? NgayThanhToan { get; set; }
        public DateTime NgayTao { get; set; }
    }

    public class HistoryRow
    {
        public string TuTrangThai { get; set; }
        public string DenTrangThai { get; set; }
        public string LyDo { get; set; }
        public string NguoiThucHien { get; set; }
        public DateTime NgayTao { get; set; }
    }

    public class RefundRow
    {
        public int MaYeuCau { get; set; }
        public int MaDon { get; set; }
        public string MaDonCode { get; set; }
        public string TenKhach { get; set; }
        public decimal SoTien { get; set; }
        public string LyDo { get; set; }
        public string TrangThai { get; set; }
        public string MaGiaoDichHoan { get; set; }
        public string LyDoTuChoi { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime? NgayXuLy { get; set; }
    }

    public class BookingDetail
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string Code { get; set; }
        public string Status { get; set; }
        public string StatusCode { get { return BookingStatus.Code(Status); } }
        public int HotelId { get; set; }
        public string HotelName { get; set; }
        public string HotelAddress { get; set; }
        public string HotelPhone { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public string RoomType { get; set; }
        public int Quantity { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int Nights { get { return (int)(CheckOut.Date - CheckIn.Date).TotalDays; } }
        public int Adults { get; set; }
        public int Children { get; set; }
        public string GuestName { get; set; }
        public string GuestEmail { get; set; }
        public string GuestPhone { get; set; }
        public string Note { get; set; }
        public string InternalNote { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public string CancelPolicyCode { get; set; }
        public string CancelPolicyText { get; set; }
        public string CheckInTime { get; set; }
        public string CheckOutTime { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CancelReason { get; set; }
        public string SnapshotJson { get; set; }
        public PriceQuote Quote { get; set; }
        public decimal PaidAmount { get; set; }
        public bool HasReview { get; set; }
        public string CoverImage { get; set; }
        public List<PaymentRow> Payments { get; set; } = new List<PaymentRow>();
        public List<HistoryRow> History { get; set; } = new List<HistoryRow>();
        public RefundRow Refund { get; set; }
        public bool IsHoldActive { get { return StatusCode == "Pending" && ExpiresAt.HasValue && ExpiresAt.Value > DateTime.Now; } }
    }

    public class BookingListRow
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string HotelName { get; set; }
        public string RoomName { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; }
        public string StatusCode { get { return BookingStatus.Code(Status); } }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string CoverImage { get; set; }
    }

    public class BookingException : Exception
    {
        public BookingException(string message) : base(message) { }
    }
}
