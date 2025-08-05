using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NotificationsService.Hubs;

[Authorize] // Chỉ những user đã đăng nhập mới có thể kết nối vào Hub này
public class NotificationHub : Hub
{
    // Hub này hiện tại không cần phương thức nào,
    // vì server sẽ chủ động đẩy tin xuống client chứ client không cần gọi lên.
}