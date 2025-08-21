namespace NotificationsService.DTOs;

public class AppNotificationReadDto
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public string? Url { get; set; }
    public DateTime CreatedAt { get; set; }
}