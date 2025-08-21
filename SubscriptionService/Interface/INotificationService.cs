using SubscriptionService.Dtos;

namespace SubscriptionService.Interface;

public interface INotificationService
{
    Task SendSubscriptionNotificationAsync(SubscriptionNotificationDto notification);
}