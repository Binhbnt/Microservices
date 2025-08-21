using SubscriptionService.Dtos;
using SubscriptionService.Interface;

namespace SubscriptionService.Services;

public class SubscriptionExpiryNotifier : BackgroundService
{
    private readonly ILogger<SubscriptionExpiryNotifier> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    // Cấu hình lịch trình: Chạy vào 9h00 và 21h00 HÀNG NGÀY
    private static readonly TimeSpan[] ScheduledTimes = { new(9, 0, 0), new(21, 0, 0) };

    public SubscriptionExpiryNotifier(ILogger<SubscriptionExpiryNotifier> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Subscription Expiry Notifier is starting.");
        
        // Chạy lần đầu ngay khi khởi động
        await DoWorkAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetDelayUntilNextScheduledRun();
            _logger.LogInformation("Next daily check scheduled for {ScheduledTime}. Waiting for {Delay}.", DateTime.Now.Add(delay), delay);
            await Task.Delay(delay, stoppingToken);
            
            await DoWorkAsync(stoppingToken);
        }

        _logger.LogInformation("Subscription Expiry Notifier is stopping.");
    }

    private async Task DoWorkAsync(CancellationToken stoppingToken = default)
    {
        _logger.LogInformation("Running daily check for expiring subscriptions...");
        try
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<ISubscriptionRepository>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                
                var expiringServices = await repository.GetExpiringSubscriptionsAsync(60);

                if (expiringServices.Any())
                {
                     _logger.LogInformation("Found {Count} expiring subscriptions. Sending notifications...", expiringServices.Count());
                    foreach (var service in expiringServices)
                    {
                        if (stoppingToken.IsCancellationRequested) break;
                        
                        var notificationDto = new SubscriptionNotificationDto
                        {
                            Id = service.Id,
                            ServiceName = service.Name,
                            Provider = service.Provider,
                            ExpiryDate = service.ExpiryDate.ToString("dd/MM/yyyy"),
                            DaysRemaining = (service.ExpiryDate.Date - DateTime.UtcNow.Date).Days,
                            EventType = "EXPIRING_SOON",
                            TriggeredBy = "System Scheduler"
                        };
                        await notificationService.SendSubscriptionNotificationAsync(notificationDto);
                    }
                }
                else
                {
                    _logger.LogInformation("No expiring subscriptions found.");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while checking for expiring subscriptions.");
        }
    }

    private static TimeSpan GetDelayUntilNextScheduledRun()
    {
        var now = DateTime.Now;
        var today = now.Date;

        // Tìm thời điểm chạy tiếp theo trong ngày hôm nay
        foreach (var time in ScheduledTimes.OrderBy(t => t))
        {
            var scheduledDateTime = today.Add(time);
            if (scheduledDateTime > now)
            {
                return scheduledDateTime - now;
            }
        }

        // Nếu tất cả các giờ trong hôm nay đã qua, thì lấy giờ đầu tiên của ngày mai
        var nextDayRunTime = today.AddDays(1).Add(ScheduledTimes.OrderBy(t => t).First());
        return nextDayRunTime - now;
    }
}