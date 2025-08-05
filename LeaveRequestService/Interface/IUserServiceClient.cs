using LeaveRequestService.DTOs;

namespace LeaveRequestService.Interface;
public interface IUserServiceClient
{
    Task<CurrentUserDto?> GetCurrentUserAsync(HttpContext httpContext);
}