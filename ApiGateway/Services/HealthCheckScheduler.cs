using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using ApiGateway.Interface;

namespace ApiGateway.Services;

public class HealthCheckScheduler : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<HealthCheckScheduler> _logger;
    private readonly IConfiguration _config;

    public HealthCheckScheduler(IServiceScopeFactory scopeFactory, ILogger<HealthCheckScheduler> logger, IConfiguration config)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🔄 HealthCheckScheduler started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var healthCheckService = scope.ServiceProvider.GetRequiredService<IHealthCheckService>();

            var services = new Dictionary<string, string>
            {
                { "ApiGateway", _config["ServiceUrls:ApiGateway"]! },
                { "UserService", _config["ServiceUrls:UserService"]! },
                { "LeaveRequestService", _config["ServiceUrls:LeaveRequestService"]! },
                { "NotificationService", _config["ServiceUrls:NotificationService"]! },
                { "AuditLogService", _config["ServiceUrls:AuditLogService"]! }
            };

            foreach (var (name, url) in services)
            {
                await healthCheckService.CheckAndLogAsync(name, url);
            }

            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }
}
