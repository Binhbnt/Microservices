// File: DTOs/LeaveRequestUpdateStatusDto.cs
using LeaveRequestService.Enums;
using System.ComponentModel.DataAnnotations;
namespace LeaveRequestService.DTOs;
public class LeaveRequestUpdateStatusDto
{
    [Required]
    public LeaveRequestStatus TrangThaiMoi { get; set; }
    
    public string? LyDoXuLy { get; set; }
}