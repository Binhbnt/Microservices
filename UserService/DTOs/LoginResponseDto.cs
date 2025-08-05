
namespace UserService.DTOs
{
    public class LoginResponseDto
    {
        public int Id { get; set; }
        public string MaSoNhanVien { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Vai trò của người dùng
        public string Token { get; set; } = string.Empty; // JWT Token
        public string? ChucVu { get; set; }
        public string? BoPhan { get; set; }
        public string? AvatarUrl { get; set; } 
    }
}