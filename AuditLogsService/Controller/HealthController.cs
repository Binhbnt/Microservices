// File: Controllers/HealthController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditLogService.Controllers; // hoặc tương ứng với từng service

[ApiController]
public class HealthController : ControllerBase
{
    [HttpGet("/health")] // tuyệt đối không có "api/" ở đây
    [AllowAnonymous]
    public IActionResult Health() => Ok("Healthy");
}
