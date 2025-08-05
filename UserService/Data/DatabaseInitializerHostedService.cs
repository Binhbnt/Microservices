using Polly;
using Npgsql;
using UserService.Data;

public class DatabaseInitializerHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializerHostedService> _logger;

    public DatabaseInitializerHostedService(IServiceProvider serviceProvider, ILogger<DatabaseInitializerHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Database Initializer Hosted Service is starting.");

        // Chính sách retry của Polly: Sẽ thử lại nếu kết nối DB thất bại
        var retryPolicy = Policy
            .Handle<NpgsqlException>() // Chỉ bắt lỗi kết nối của PostgreSQL
            .WaitAndRetryAsync(5, // Thử lại tối đa 5 lần
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Thời gian chờ tăng dần: 2s, 4s, 8s...
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "[Polly] Retry {RetryCount}: Database connection failed. Waiting {TimeSpan}s. Error: {ErrorMessage}",
                        retryCount, timeSpan.TotalSeconds, exception.Message);
                });

        // Chạy policy trong một luồng riêng để không block ứng dụng
        _ = Task.Run(async () =>
        {
            await retryPolicy.ExecuteAsync(async () =>
            {
                // Tạo một scope riêng để lấy các service khác
                using var scope = _serviceProvider.CreateScope();
                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var connectionString = config.GetConnectionString("DefaultConnection");

                _logger.LogInformation("Attempting to initialize database schema...");
                
                // Tạo kết nối mới và sạch
                await using var connection = new NpgsqlConnection(connectionString);

                // Gọi lớp DatabaseInitializer để tạo bảng (chúng ta sẽ sửa file này ở bước sau)
                var dbInitializer = new DatabaseInitializer(connection);
                await dbInitializer.InitializeAsync();

                _logger.LogInformation("Database schema initialization successful.");
            });
        }, cancellationToken);
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}