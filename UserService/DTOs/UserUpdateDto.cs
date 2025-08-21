using System.ComponentModel.DataAnnotations;
using UserService.Enums;
namespace UserService.DTOs
{
    public class UserUpdateDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string MaSoNhanVien { get; set; } = string.Empty;
        [Required]
        public string HoTen { get; set; } = string.Empty;
        public string? ChucVu { get; set; }
        public string? BoPhan { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        public string? MatKhau { get; set; }
        public UserRole Role { get; set; }
    }
}