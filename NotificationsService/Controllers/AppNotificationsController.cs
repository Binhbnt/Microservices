using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationsService.DTOs;
using NotificationsService.Interface;
using NotificationsService.Services;
using System.Text.Json;
using System.Threading.Tasks;

namespace NotificationsService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Yêu cầu tất cả các endpoint trong đây phải xác thực
public class AppNotificationsController : ControllerBase
{
    private readonly IAppNotificationService _notificationService;

    public AppNotificationsController(IAppNotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet("state")]
    public async Task<IActionResult> GetNotificationState()
    {
        // User được lấy từ token mà frontend gửi lên
        var state = await _notificationService.GetNotificationStateAsync(User);
        return Ok(state);
    }

    [HttpPost("mark-all-as-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _notificationService.MarkAllAsReadAsync(User);
        return NoContent(); // Trả về 204 No Content khi thành công
    }

    // Endpoint này có thể được gọi từ các service khác (như LeaveRequestService)
    // để tạo thông báo mới. Cần cân nhắc về vấn đề bảo mật cho nó.
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateNotification([FromBody] AppNotificationCreateDto dto)
    {
        await _notificationService.CreateNotificationAsync(dto);
        return CreatedAtAction(nameof(GetNotificationState), new { }, null); // Trả về 201 Created
    }

    [HttpGet]
    public async Task<IActionResult> GetAllNotificationsForUser(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 20)
    {
        var pagedResult = await _notificationService.GetAllForUserAsync(User, pageNumber, pageSize);

        Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(pagedResult.PaginationMetadata));

        return Ok(pagedResult.Items);
    }

    [HttpPost("{id:int}/mark-as-read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var result = await _notificationService.MarkAsReadAsync(id, User);
        if (result)
        {
            return NoContent(); // 204 No Content
        }
        return Ok(); // Hoặc trả về một trạng thái khác nếu không có gì để cập nhật
    }


}