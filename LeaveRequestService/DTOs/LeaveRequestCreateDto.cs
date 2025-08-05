// File: DTOs/LeaveRequestCreateDto.cs
using LeaveRequestService.Enums;
using System.ComponentModel.DataAnnotations;
namespace LeaveRequestService.DTOs;

public class LeaveRequestCreateDto
{
    [Required]
    public LeaveType LoaiPhep { get; set; }

    public string? LyDo { get; set; }

    [Required]
    public DateTime NgayTu { get; set; }

    [Required]
    public DateTime NgayDen { get; set; }
    public string? GioTu { get; set; }   // Use string for "HH:mm" format
    public string? GioDen { get; set; }
    public string? CongViecBanGiao { get; set; }
}