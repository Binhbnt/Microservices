// File: Models/LeaveRequest.cs
using System.ComponentModel.DataAnnotations.Schema;
using LeaveRequestService.Enums;
namespace LeaveRequestService.Models;

public class LeaveRequest
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public LeaveType LoaiPhep { get; set; }
    public string? LyDo { get; set; }
    public DateTime NgayTu { get; set; }
    public DateTime NgayDen { get; set; }
    public string? GioTu { get; set; }
    public string? GioDen { get; set; }
    public string? CongViecBanGiao { get; set; }
    public LeaveRequestStatus TrangThai { get; set; }
    public string? LyDoXuLy { get; set; }
    public bool DaGuiN8n { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? LastUpdatedAt { get; set; }
    public int? CreatedByUserId { get; set; }
    public int? LastUpdatedByUserId { get; set; }
    public int? CreatorRole { get; set; }
    public string? ApprovalToken { get; set; }
    public DateTime? ApprovalTokenExpires { get; set; }
    public string? RevocationToken { get; set; }
    public DateTime? RevocationTokenExpires { get; set; }
    [NotMapped] // Thuộc tính này không được ánh xạ vào cột trong DB
    public double DurationInDays => CalculateDuration();
    private double CalculateDuration()
    {
        // --- Cấu hình ---
        var lunchStart = new TimeSpan(12, 0, 0);
        var lunchEnd = new TimeSpan(13, 0, 0);
        var workHoursPerDay = 8.0;

        // --- Tách riêng hai lệnh TryParse để tránh lỗi short-circuiting ---
        bool isStartTimeValid = TimeSpan.TryParse(GioTu, out var startTime);
        bool isEndTimeValid = TimeSpan.TryParse(GioDen, out var endTime);

        // Chỉ thực hiện tính toán phức tạp khi CẢ HAI giờ từ và đến đều hợp lệ
        if (isStartTimeValid && isEndTimeValid)
        {
            double totalWorkHours = 0;

            for (var date = NgayTu.Date; date <= NgayDen.Date; date = date.AddDays(1))
            {
                var dayStart = (date == NgayTu.Date) ? startTime : new TimeSpan(8, 0, 0);
                var dayEnd = (date == NgayDen.Date) ? endTime : new TimeSpan(17, 0, 0);

                var grossDuration = dayEnd - dayStart;

                var lunchOverlapStart = Max(dayStart, lunchStart);
                var lunchOverlapEnd = Min(dayEnd, lunchEnd);
                var lunchOverlap = lunchOverlapEnd - lunchOverlapStart;

                if (lunchOverlap < TimeSpan.Zero)
                {
                    lunchOverlap = TimeSpan.Zero;
                }

                var netWorkDuration = grossDuration - lunchOverlap;
                totalWorkHours += netWorkDuration.TotalHours;
            }

            return totalWorkHours / workHoursPerDay;
        }
        else
        {
            // Nếu không có giờ cụ thể hoặc giờ không hợp lệ, tính trọn ngày
            return (NgayDen.Date - NgayTu.Date).TotalDays + 1;
        }
    }
    private TimeSpan Max(TimeSpan t1, TimeSpan t2) => t1 > t2 ? t1 : t2;
    private TimeSpan Min(TimeSpan t1, TimeSpan t2) => t1 < t2 ? t1 : t2;
}
