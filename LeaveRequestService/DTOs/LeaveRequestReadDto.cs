// File: DTOs/LeaveRequestReadDto.cs
using LeaveRequestService.Enums;
namespace LeaveRequestService.DTOs;

public class LeaveRequestReadDto
{
    public int Id { get; set; }
    public int UserId { get; set; }

    // --- Các trường được "làm giàu" từ UserService ---
    public string? MaSoNhanVien { get; set; }
    public string? HoTen { get; set; }
    public string? BoPhan { get; set; }
    public string? ChucVu { get; set; }
    public string? Email { get; set; }
    public string Role { get; set; } = string.Empty;
    // ------------------------------------------------

    public LeaveType LoaiPhep { get; set; }
    public string? LyDo { get; set; }
    public DateTime NgayTu { get; set; }
    public DateTime NgayDen { get; set; }
    public string? GioTu { get; set; }   // Use string to hold HH:mm format
    public string? GioDen { get; set; }
    public LeaveRequestStatus TrangThai { get; set; }
    public string? LyDoXuLy { get; set; }
    public DateTime NgayTao { get; set; }
    public string? CongViecBanGiao { get; set; }
    public double DurationInDays { get; set; }

}