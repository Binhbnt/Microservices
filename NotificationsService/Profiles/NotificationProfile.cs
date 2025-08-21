using AutoMapper;
using NotificationsService.DTOs;
using NotificationsService.Models;

namespace NotificationsService.Profiles;

public class NotificationProfile : Profile
{
    public NotificationProfile()
    {
         // Map từ Model sang DTO để gửi cho SignalR/API
        CreateMap<AppNotification, AppNotificationReadDto>();

        // Map từ DTO tạo mới sang Model để lưu vào DB
        CreateMap<AppNotificationCreateDto, AppNotification>();
        
    }
}