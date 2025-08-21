// ApiGateway/Services/HealthLogCleanupService.cs

using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration; // Thêm using này

namespace ApiGateway.Services
{
    public class HealthLogCleanupService : BackgroundService
    {
        private readonly ILogger<HealthLogCleanupService> _logger;
        
        private readonly IMongoCollection<dynamic> _collection;

        public HealthLogCleanupService(
            IMongoClient mongoClient, 
            IConfiguration config, 
            ILogger<HealthLogCleanupService> logger)
        {
            _logger = logger;

            // Lấy database và collection từ client đã được tiêm vào
            var databaseName = config["MongoDB:Database"];
            var database = mongoClient.GetDatabase(databaseName);
            // Lưu ý: Tên collection ở đây là "HealthCheckLogs"
            _collection = database.GetCollection<dynamic>("HealthCheckLogs");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var thresholdDate = DateTime.UtcNow.AddDays(-14);
                try
                {
                    var filter = Builders<dynamic>.Filter.Lt("checkedAt", thresholdDate);
                    // Sử dụng _collection trực tiếp
                    var result = await _collection.DeleteManyAsync(filter, cancellationToken: stoppingToken);

                    if (result.DeletedCount > 0)
                    {
                        _logger.LogInformation("🧹 Dọn log health: đã xóa {Count} bản ghi cũ hơn {Threshold}", result.DeletedCount, thresholdDate);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi dọn log health");
                }

                // Chờ 6 tiếng cho lần chạy tiếp theo
                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }
    }
}