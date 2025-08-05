using Polly;
using Npgsql;
using NotificationsService.Data;

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

        var retryPolicy = Policy
            .Handle<NpgsqlException>() 
            .WaitAndRetryAsync(5,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "[Polly] Retry {RetryCount}: Database connection failed. Waiting {TimeSpan}s. Error: {ErrorMessage}",
                        retryCount, timeSpan.TotalSeconds, exception.Message);
                });

        _ = Task.Run(async () =>
        {
            await retryPolicy.ExecuteAsync(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var connectionString = config.GetConnectionString("DefaultConnection");

                _logger.LogInformation("Attempting to initialize database schema...");
                await using var connection = new NpgsqlConnection(connectionString);

                var dbInitializer = new DatabaseInitializer(connection);
                await dbInitializer.InitializeAsync();

                _logger.LogInformation("Database schema initialization successful.");
            });
        }, cancellationToken);
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}