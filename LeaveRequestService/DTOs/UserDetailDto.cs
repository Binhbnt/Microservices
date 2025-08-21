// DTO này dùng để hứng dữ liệu từ UserService
namespace LeaveRequestService.DTOs;

public class UserDetailDto
{
    public int Id { get; set; }
    public string? MaSoNhanVien { get; set; }
    public string? HoTen { get; set; }
    public string? BoPhan { get; set; }
    public string? ChucVu { get; set; }
    public string? Email { get; set; }
    public string Role { get; set; } = string.Empty;
    public int TotalLeaveEntitlement { get; set; } // Tổng số phép được hưởng
    public double DaysTaken { get; set; }         // Số ngày đã nghỉ
    public double RemainingLeaveDays { get; set; }  // Số ngày còn lại
    public DateTime NgayTao { get; set; } 

}