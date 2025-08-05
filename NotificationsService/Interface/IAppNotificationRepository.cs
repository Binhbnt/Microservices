using NotificationsService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NotificationsService.Interface;

public interface IAppNotificationRepository
{
    Task<AppNotification> CreateAsync(AppNotification notification);

    // Lấy danh sách thông báo cho nhiều user (dùng cho Admin/SuperUser)
    Task<IEnumerable<AppNotification>> GetByUserIdsAsync(List<int> userIds);

    // Đánh dấu tất cả là đã đọc cho một user
    Task<bool> MarkAllAsReadAsync(int userId);
    Task<IEnumerable<AppNotification>> GetAllAsync();
    Task<(IEnumerable<AppNotification>, int)> GetAllPaginatedForUserIdsAsync(List<int> userIds, int pageNumber, int pageSize);
    Task<bool> MarkAsReadAsync(int notificationId);
}