using AuditLogService.Models;

namespace AuditLogService.Interface;


public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log);
    Task<IEnumerable<AuditLog>> GetAllAsync();
}