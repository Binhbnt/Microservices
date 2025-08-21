// ApiGateway/Services/HealthCheckService.cs

using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Configuration;
using ApiGateway.Models;
using ApiGateway.Interface;
using Microsoft.AspNetCore.SignalR;
using ApiGateway.Hubs;
namespace ApiGateway.Services;

public class HealthCheckService : IHealthCheckService
{
    private readonly IMongoCollection<ServiceHealthLog> _collection;
    private readonly HttpClient _httpClient;
    private readonly IHubContext<HealthHub> _hubContext;

    public HealthCheckService(IMongoClient mongoClient, IConfiguration config, IHubContext<HealthHub> hubContext)
    {
        try
        {
            // Lấy database name từ config
            var databaseName = config["MongoDB:Database"];

            // Thay vào đó, sử dụng 'mongoClient' đã được tiêm vào (là Singleton).
            var database = mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<ServiceHealthLog>("HealthLogs");
            
            _hubContext = hubContext;
            _httpClient = new HttpClient();

            // Log này chỉ để xác nhận service đã được khởi tạo, không phải kết nối mới.
            Console.WriteLine($"✅ HealthCheckService đã khởi tạo và sử dụng database: {databaseName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Lỗi khởi tạo HealthCheckService: " + ex.Message);
            throw;
        }
    }

    public async Task CheckAndLogAsync(string serviceName, string url)
    {
        var log = new ServiceHealthLog
        {
            ServiceName = serviceName,
            CheckedAt = DateTime.UtcNow
        };

        try
        {
            var response = await _httpClient.GetAsync(url);
            log.Status = response.IsSuccessStatusCode ? "Healthy" : "Unreachable";
        }
        catch
        {
            log.Status = "Error";
        }

        await _collection.InsertOneAsync(log);
        await _hubContext.Clients.All.SendAsync("HealthLogUpdated", new
        {
            serviceName = log.ServiceName,
            status = log.Status,
            checkedAt = log.CheckedAt
        });
    }

    public async Task<List<ServiceHealthLog>> GetLatestLogsAsync()
    {
        return await _collection.Find(_ => true)
            .SortByDescending(x => x.CheckedAt)
            .Limit(50)
            .ToListAsync();
    }
}