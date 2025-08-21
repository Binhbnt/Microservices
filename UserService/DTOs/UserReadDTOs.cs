namespace UserService.DTOs
{
    public class UserReadDto
    {
        public int Id { get; set; }
        public string MaSoNhanVien { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string? ChucVu { get; set; }
        public string? BoPhan { get; set; }
        public string? Email { get; set; }
        public string Role { get; set; } = string.Empty; // Sẽ hiển thị tên của Role, vd: "Người dùng"
        public DateTime NgayTao { get; set; }
        public string? AvatarUrl { get; set; }
    }
}