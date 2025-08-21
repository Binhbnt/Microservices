using System.ComponentModel.DataAnnotations;
using UserService.Enums;
namespace UserService.DTOs
{
    public class UserCreateDto
    {
        [Required(ErrorMessage = "Mã số nhân viên là bắt buộc.")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Mã số nhân viên phải từ 6 đến 50 ký tự")]
        [RegularExpression(@"^NV\d{4}$", ErrorMessage = "Mã số nhân viên phải bắt đầu bằng 'NV' và theo sau là 4 chữ số (Ví dụ: NV0001)")]
        public string MaSoNhanVien { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Họ tên không vượt quá 100 ký tự.")]
        public string HoTen { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Chức vụ không vượt quá 100 ký tự.")]
        public string? ChucVu { get; set; }

        [StringLength(100, ErrorMessage = "Bộ phận không vượt quá 100 ký tự.")]
        public string? BoPhan { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Email không hợp lệ (Ví dụ: abc@abc.com).")]
        [StringLength(100, ErrorMessage = "Email không vượt quá 100 ký tự.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 100 ký tự.")]
        public string? MatKhau { get; set; } = string.Empty; // Mật khẩu plain text từ form

        [Required(ErrorMessage = "Vai trò là bắt buộc.")]
        public UserRole Role { get; set; } = UserRole.User;
        public string? AvatarUrl { get; set; }
    }
}