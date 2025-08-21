using Microsoft.AspNetCore.Mvc;
using AuditLogService.Services;
using AuditLogService.DTOs;
using Microsoft.AspNetCore.Authorization;
using AuditLogService.Interface;

namespace AuditLogService.Controllers;

[ApiController]
[Route("api/[controller]")] // URL sẽ là: /api/AuditLogs
[Authorize] // Yêu cầu mọi request đến controller này đều phải có token hợp lệ
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogManagementService _auditLogService;

    public AuditLogsController(IAuditLogManagementService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// Endpoint để các dịch vụ khác gọi đến để tạo một bản ghi log mới.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateLog([FromBody] AuditLogCreateDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _auditLogService.CreateLogAsync(createDto);

        // Trả về 201 Created để báo hiệu đã tạo thành công
        // Hoặc có thể trả về Ok() cũng được vì đây là nghiệp vụ nền
        return StatusCode(201);
    }

    /// <summary>
    /// Endpoint để lấy lại toàn bộ lịch sử log, chỉ Admin mới có quyền xem.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")] // Chỉ Admin hoặc SuperUser mới được xem
    public async Task<IActionResult> GetAuditLogs()
    {
        var logs = await _auditLogService.GetLogsAsync();
        return Ok(logs);
    }

}