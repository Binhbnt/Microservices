using ApiGateway.Interface;
using ApiGateway.Models;
using ApiGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class HealthCheckController : ControllerBase
{
    private readonly IHealthCheckService _healthCheckService;
    private readonly IConfiguration _config;

    public HealthCheckController(IHealthCheckService healthCheckService, IConfiguration config)
    {
        _healthCheckService = healthCheckService;
        _config = config;
    }

    [HttpPost("check-all")]
    public async Task<IActionResult> CheckAll()
    {
        var services = new Dictionary<string, string>
    {
        { "ApiGateway", _config["ServiceUrls:ApiGateway"]! },
        { "UserService", _config["ServiceUrls:UserService"]! },
        { "LeaveRequestService", _config["ServiceUrls:LeaveRequestService"]! },
        { "NotificationService", _config["ServiceUrls:NotificationService"]! },
        { "AuditLogService", _config["ServiceUrls:AuditLogService"]! },
        { "SubscriptionService", _config["ServiceUrls:SubscriptionService"]! },
    };

        foreach (var (name, url) in services)
        {
            await _healthCheckService.CheckAndLogAsync(name, url);
        }

        return Ok("Health checks logged.");
    }

    [HttpGet("logs")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Dictionary<string, List<ServiceHealthLog>>>> GetGroupedLatestLogs()
    {
        var logs = await _healthCheckService.GetLatestLogsAsync(); // Vẫn lấy ra danh sách phẳng

        // Dùng LINQ để nhóm lại và sắp xếp
        var groupedLogs = logs
            .GroupBy(log => log.ServiceName)
            .ToDictionary(
                group => group.Key,
                // Sắp xếp các log theo thời gian để đảm bảo thứ tự đúng
                group => group.OrderBy(log => log.CheckedAt).ToList()
            );

        return Ok(groupedLogs);
    }

    [HttpGet("/health")]
    public IActionResult Health() => Ok("Gateway healthy");

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest()
    {
        var logs = await _healthCheckService.GetLatestLogsAsync();

        // Trả ra log mới nhất theo thời gian
        var latest = logs
            .GroupBy(l => l.ServiceName)
            .Select(g => g.OrderByDescending(l => l.CheckedAt).First())
            .ToList();

        return Ok(latest);
    }
}
