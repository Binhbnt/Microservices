
using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Tên đăng nhập (Email hoặc Mã số NV) là bắt buộc.")]
        public string Username { get; set; } = string.Empty; // Có thể là Email hoặc MaSoNhanVien

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        public string Password { get; set; } = string.Empty;
    }
}