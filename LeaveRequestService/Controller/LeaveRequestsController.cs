using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LeaveRequestService.Services;
using LeaveRequestService.DTOs;
using LeaveRequestService.Interface;

namespace LeaveRequestService.Controllers;

[ApiController]
[Route("api/[controller]")] // URL sẽ là /api/LeaveRequests
[Authorize] // Yêu cầu mọi request phải được xác thực
public class LeaveRequestsController : ControllerBase
{
    private readonly ILeaveRequestManagementService _leaveRequestService;
    private readonly ILogger<LeaveRequestsController> _logger;

    public LeaveRequestsController(ILeaveRequestManagementService leaveRequestService, ILogger<LeaveRequestsController> logger)
    {
        _leaveRequestService = leaveRequestService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateLeaveRequest([FromBody] LeaveRequestCreateDto createDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var newRequest = await _leaveRequestService.CreateAsync(createDto, User);
            return CreatedAtAction(nameof(GetRequestById), new { id = newRequest.Id }, newRequest);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo đơn xin phép.");
            return StatusCode(500, "Lỗi server nội bộ.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetLeaveRequests([FromQuery] string? searchTerm,
    [FromQuery] string? status)
    {
        var requests = await _leaveRequestService.GetAllAsync(User, searchTerm, status);
        return Ok(requests);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRequestById(int id)
    {
        try
        {
            var request = await _leaveRequestService.GetByIdAsync(id, User);
            if (request == null) return NotFound();
            return Ok(request);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,SuperUser")] // Chỉ Admin và SuperUser mới có quyền duyệt
    public async Task<IActionResult> UpdateRequestStatus(int id, [FromBody] LeaveRequestUpdateStatusDto updateDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var result = await _leaveRequestService.UpdateStatusAsync(id, updateDto, User);
            if (!result) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}/cancel")]
    public async Task<IActionResult> CancelRequest(int id)
    {
        try
        {
            var result = await _leaveRequestService.CancelAsync(id, User);
            if (!result) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/send-for-approval")]
    public async Task<IActionResult> SendRequestForApproval(int id)
    {
        try
        {
            var result = await _leaveRequestService.SendForApprovalAsync(id, User);
            if (!result) return BadRequest(new { message = "Không thể gửi yêu cầu duyệt." });
            return Ok(new { message = "Đã gửi yêu cầu duyệt thành công." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("process-approval")]
    [AllowAnonymous] // Cho phép truy cập không cần token đăng nhập
    public async Task<IActionResult> ProcessApproval([FromBody] ProcessApprovalDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var result = await _leaveRequestService.ProcessApprovalAsync(dto);
            if (!result)
            {
                return BadRequest(new { message = "Token không hợp lệ hoặc đã hết hạn." });
            }
            return Ok(new { message = "Xử lý đơn thành công." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/resubmit")]
    public async Task<IActionResult> ResubmitRequest(int id, [FromBody] LeaveRequestCreateDto dto)
    {
        try
        {
            var newRequest = await _leaveRequestService.ResubmitAsync(id, dto, User);
            return Ok(newRequest);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/request-revocation")]
    [Authorize]
    public async Task<IActionResult> RequestRevocation(int id)
    {
        try
        {
            var result = await _leaveRequestService.RequestRevocationAsync(id, User);
            if (!result) return BadRequest(new { message = "Gửi yêu cầu thu hồi thất bại." });
            return Ok(new { message = "Đã gửi yêu cầu thu hồi đến người quản lý." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("process-revocation")]
    [AllowAnonymous]
    public async Task<IActionResult> ProcessRevocation([FromBody] ProcessRevocationDto dto)
    {
        try
        {
            var result = await _leaveRequestService.ProcessRevocationAsync(dto);
            if (!result) return BadRequest(new { message = "Token thu hồi không hợp lệ hoặc đã hết hạn." });
            return Ok(new { message = "Đã xử lý yêu cầu thu hồi thành công." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportLeaveRequest(
    [FromQuery] string? searchTerm = null,
    [FromQuery] string? status = null)
    {
        try
        {
            var excelBytes = await _leaveRequestService.ExportLeaveRequestsToExcelAsync(User, searchTerm, status);

            var fileName = $"LeaveRequests_{DateTime.Now:yyyyMMddHHmm}.xlsx";
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xuất file Excel của LeaveRequest.");
            return StatusCode(500, new { message = $"Lỗi khi xuất Excel: {ex.Message}" });
        }
    }

    [HttpGet("dashboard-stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        try
        {
            var stats = await _leaveRequestService.GetDashboardStatsAsync(User);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thống kê dashboard.");
            return StatusCode(500, "Lỗi server.");
        }
    }

    [HttpGet("users-with-leave-details")]
    [Authorize(Roles = "Admin,SuperUser")]
    public async Task<IActionResult> GetAllUsersWithLeaveDetails(
    [FromQuery] bool isDeleted = false,
    [FromQuery] string? searchTerm = null,
    [FromQuery] string? role = null)
    {
        var users = await _leaveRequestService.GetAllUsersWithLeaveDetailsAsync(User, isDeleted, searchTerm, role);
        return Ok(users);
    }

}