using System.Collections.Generic;

namespace NotificationsService.DTOs;

public class AppNotificationStateDto
{
    public int UnreadCount { get; set; }
    public List<AppNotificationReadDto> RecentNotifications { get; set; }
}