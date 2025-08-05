using ApiGateway.Models;

namespace ApiGateway.Interface;

public interface IHealthCheckService
{
    Task CheckAndLogAsync(string serviceName, string url);
    Task<List<ServiceHealthLog>> GetLatestLogsAsync();
}