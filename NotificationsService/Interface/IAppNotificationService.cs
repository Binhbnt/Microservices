using NotificationsService.DTOs;
using NotificationsService.Helpers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NotificationsService.Interface;

public interface IAppNotificationService
{
    // DTO này sẽ chứa cả số lượng chưa đọc và danh sách thông báo
    Task<AppNotificationStateDto> GetNotificationStateAsync(ClaimsPrincipal user);

    Task MarkAllAsReadAsync(ClaimsPrincipal user);

    // DTO này dùng để nhận dữ liệu khi một service khác muốn tạo thông báo
    Task CreateNotificationAsync(AppNotificationCreateDto dto);
    Task<PagedResult<AppNotificationReadDto>> GetAllForUserAsync(ClaimsPrincipal user, int pageNumber, int pageSize);
    Task<bool> MarkAsReadAsync(int notificationId, ClaimsPrincipal user);
}