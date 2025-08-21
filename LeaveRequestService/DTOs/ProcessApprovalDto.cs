// File: DTOs/ProcessApprovalDto.cs
using System.ComponentModel.DataAnnotations;
namespace LeaveRequestService.DTOs;
public class ProcessApprovalDto
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public string Action { get; set; } = string.Empty; // "approve" hoặc "reject"

    public string? LyDoXuLy { get; set; }
}