namespace LeaveRequestService.DTOs;

public class CurrentUserDto
{
    public int Id { get; set; }
    public string MaSoNhanVien { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string Email { get; set; } = "";
    public string BoPhan { get; set; } = "";
    public string ChucVu { get; set; } = "";
    public string Role { get; set; } = "";
}
