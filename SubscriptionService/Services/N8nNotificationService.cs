using System.Text;
using System.Text.Json;
using SubscriptionService.Dtos;
using SubscriptionService.Interface;

namespace SubscriptionService.Services;

public class N8nNotificationService : INotificationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<N8nNotificationService> _logger;

    public N8nNotificationService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<N8nNotificationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendSubscriptionNotificationAsync(SubscriptionNotificationDto notification)
    {
        var webhookUrl = _configuration["N8nSettings:WebhookUrl"];

        // Nếu URL chưa được cấu hình hoặc là URL mẫu thì không làm gì cả
        if (string.IsNullOrEmpty(webhookUrl))
        {
            _logger.LogWarning("N8N Webhook URL is not configured. Skipping notification.");
            return;
        }

        try
        {
            var client = _httpClientFactory.CreateClient("N8nClient");
            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            //Truyền options vào khi serialize
            var jsonPayload = JsonSerializer.Serialize(notification, serializerOptions);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(webhookUrl, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully sent notification to n8n for service: {ServiceName}", notification.ServiceName);
            }
            else
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send notification to n8n. Status: {StatusCode}. Response: {ResponseBody}", response.StatusCode, responseBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while sending notification to n8n.");
        }
    }
}