// File: AuditLogsService/Models/AuditLog.cs

namespace AuditLogService.Models;

public class AuditLog
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? Details { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? RequesterIpAddress { get; set; }
}