namespace NotificationsService.Models;

public class AppNotification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Message { get; set; }
    public bool IsRead { get; set; }
    public string? Url { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? TriggeredByUserId { get; set; }
    public string? TriggeredByUsername { get; set; }
}