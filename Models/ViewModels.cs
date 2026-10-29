using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TravelHub.Models
{
    public class DestinationItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string TravelStyle { get; set; }
        public string Region { get; set; }
        public int HotelCount { get; set; }
    }

    public class HotelCard
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int DestinationId { get; set; }
        public string DestinationName { get; set; }
        public int Stars { get; set; }
        public decimal PriceFrom { get; set; }
        public string CoverImageUrl { get; set; }
        public string Description { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public string PropertyType { get; set; }
        // Mở rộng cho trang kết quả tìm kiếm
        public bool BreakfastIncluded { get; set; }
        public string CancelPolicy { get; set; }
        public int RoomsLeft { get; set; }
        public decimal TotalForStay { get; set; }
        public List<string> Amenities { get; set; } = new List<string>();
    }

    public class RoomItem
    {
        public int Id { get; set; }
        public int HotelId { get; set; }
        public string Name { get; set; }
        public string RoomType { get; set; }
        public int MaxGuests { get; set; }
        public int? BedCount { get; set; }
        public decimal Price { get; set; }
        public int TotalInventory { get; set; }
        public int Occupied { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int Available { get { return Math.Max(0, TotalInventory - Occupied); } }
        public List<RoomImageItem> Images { get; set; } = new List<RoomImageItem>();
    }

    public class RoomImageItem
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string Url { get; set; }
        public string Caption { get; set; }
        public string Kind { get; set; }
        public int Order { get; set; }
        public bool IsCover { get; set; }
        public bool Visible { get; set; }
    }

    public class ReviewItem
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public int Rating { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PromotionItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public decimal? PercentOff { get; set; }
        public decimal? AmountOff { get; set; }
    }

    public class ArticleItem
    {
        public int Id { get; set; }
        public int DestinationId { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string ImageUrl { get; set; }
        public string Author { get; set; }
    }

    public class AttractionItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public decimal? TicketPrice { get; set; }
    }

    public class FlightItem
    {
        public int Id { get; set; }
        public string Airline { get; set; }
        public string FlightNumber { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime DepartureAt { get; set; }
        public DateTime ArrivalAt { get; set; }
        public decimal Price { get; set; }
        public int SeatsAvailable { get; set; }
    }

    public class UserSession
    {
        public int AccountId { get; set; }
        public int? CustomerId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }      // mã vai trò: Traveler | Partner | Admin | Staff | ContentManager
        public bool EmailConfirmed { get; set; }
    }

    public class BookingItem
    {
        public int Id { get; set; }
        public string BookingCode { get; set; }
        public int HotelId { get; set; }
        public string HotelName { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int Adults { get; set; }
        public decimal Total { get; set; }
        public string StatusText { get; set; }
        public string StatusCode { get; set; }
        public string RoomName { get; set; }
        public int Nights { get; set; }
    }

    public class HinhAnhKhachSan
    {
        public int MaHinhAnh { get; set; }
        public int MaKS { get; set; }
        public string DuongDanAnh { get; set; }
        public string MoTa { get; set; }
        public bool LaAnhDaiDien { get; set; }
        public string Loai { get; set; }
        public int ThuTu { get; set; }
        public bool HienThi { get; set; } = true;
    }

    public class PartnerHotelCreateViewModel
    {
        [Required, StringLength(150)] public string TenKS { get; set; }
        [Required] public int MaDiaDiem { get; set; }
        [Required, StringLength(255)] public string DiaChi { get; set; }
        [Range(1, 5)] public int SoSao { get; set; }
        [Range(0, 1000000000)] public decimal GiaTu { get; set; }
        public string MoTa { get; set; }
        public string HinhAnhDaiDien { get; set; }
    }

    // ---- Tài khoản ----
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập"), StringLength(50, MinimumLength = 4, ErrorMessage = "Tên đăng nhập 4–50 ký tự")]
        [RegularExpression(@"^[A-Za-z0-9_.]+$", ErrorMessage = "Chỉ dùng chữ, số, dấu chấm và gạch dưới")]
        [Display(Name = "Tên đăng nhập")] public string UserName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên"), StringLength(100)]
        [Display(Name = "Họ và tên")] public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email"), EmailAddress(ErrorMessage = "Email không hợp lệ"), StringLength(100)]
        [Display(Name = "Email")] public string Email { get; set; }

        [RegularExpression(@"^[0-9+\s]{8,15}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")] public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu"), StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự")]
        [DataType(DataType.Password), Display(Name = "Mật khẩu")] public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Mật khẩu nhập lại không khớp")]
        [DataType(DataType.Password), Display(Name = "Nhập lại mật khẩu")] public string ConfirmPassword { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập"), Display(Name = "Tên đăng nhập")] public string UserName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu"), DataType(DataType.Password), Display(Name = "Mật khẩu")] public string Password { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email"), EmailAddress(ErrorMessage = "Email không hợp lệ")] public string Email { get; set; }
    }

    public class ResetPasswordViewModel
    {
        public string Token { get; set; }
        [Required, EmailAddress] public string Email { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới"), StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự")]
        [DataType(DataType.Password), Display(Name = "Mật khẩu mới")] public string Password { get; set; }
        [Compare("Password", ErrorMessage = "Mật khẩu nhập lại không khớp")]
        [DataType(DataType.Password), Display(Name = "Nhập lại mật khẩu")] public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại"), DataType(DataType.Password), Display(Name = "Mật khẩu hiện tại")] public string OldPassword { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới"), StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự")]
        [DataType(DataType.Password), Display(Name = "Mật khẩu mới")] public string NewPassword { get; set; }
        [Compare("NewPassword", ErrorMessage = "Mật khẩu nhập lại không khớp")]
        [DataType(DataType.Password), Display(Name = "Nhập lại mật khẩu")] public string ConfirmPassword { get; set; }
    }

    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên"), StringLength(100), Display(Name = "Họ và tên")] public string FullName { get; set; }
        [RegularExpression(@"^[0-9+\s]{8,15}$", ErrorMessage = "Số điện thoại không hợp lệ"), Display(Name = "Số điện thoại")] public string Phone { get; set; }
        [Display(Name = "Ngày sinh"), DataType(DataType.Date)] public DateTime? BirthDate { get; set; }
        [Display(Name = "Giới tính")] public string Gender { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}
