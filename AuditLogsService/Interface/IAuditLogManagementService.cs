
using AuditLogService.DTOs;

namespace AuditLogService.Interface;

public interface IAuditLogManagementService
{
    // Nhiệm vụ tạo một bản ghi log mới
    Task CreateLogAsync(AuditLogCreateDto dto);

    // Nhiệm vụ lấy ra danh sách các log đã ghi
    Task<IEnumerable<AuditLogReadDto>> GetLogsAsync();

}