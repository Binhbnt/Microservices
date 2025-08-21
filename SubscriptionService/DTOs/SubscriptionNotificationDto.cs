namespace SubscriptionService.Dtos;

public class SubscriptionNotificationDto
{
    public Guid Id { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? Provider { get; set; }
    public string ExpiryDate { get; set; } = string.Empty;
    public int DaysRemaining { get; set; }
    public string EventType { get; set; } = string.Empty; // "CREATE", "UPDATE", "DELETE"
    public string TriggeredBy { get; set; } = string.Empty;
}