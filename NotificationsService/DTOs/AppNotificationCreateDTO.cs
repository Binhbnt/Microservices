namespace NotificationsService.DTOs;

public class AppNotificationCreateDto
{
    public int UserId { get; set; }
    public string Message { get; set; }
    public string? Url { get; set; }
    public int? TriggeredByUserId { get; set; }
    public string? TriggeredByUsername { get; set; }
}