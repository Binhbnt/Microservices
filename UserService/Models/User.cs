using UserService.Enums;

namespace UserService.Models
{
    public class User
    {
        public int Id { get; set; }
        public string MaSoNhanVien { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string? ChucVu { get; set; }
        public string? BoPhan { get; set; }
        public string? Email { get; set; }
        public string? MatKhau { get; set; }
        public UserRole Role { get; set; } = UserRole.User;
        public DateTime NgayTao { get; set; }
        public bool IsDeleted { get; set; }
        public int? CreatedByUserId { get; set; }
        public int? LastUpdatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public string? AvatarUrl { get; set; }
        public int TotalLeaveEntitlement { get; set; } = 12;
    }
}