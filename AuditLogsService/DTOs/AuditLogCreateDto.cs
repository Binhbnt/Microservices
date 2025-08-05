using System.ComponentModel.DataAnnotations;

namespace AuditLogService.DTOs;

public class AuditLogCreateDto
{
    public int? UserId { get; set; }
    public string? Username { get; set; }

    [Required]
    [StringLength(100)]
    public string ActionType { get; set; } = string.Empty;

    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? Details { get; set; }
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public string? RequesterIpAddress { get; set; }
}