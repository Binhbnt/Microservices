using LeaveRequestService.DTOs;

namespace LeaveRequestService.Interface;
public interface IUserServiceClient
{
    Task<CurrentUserDto?> GetCurrentUserAsync(HttpContext httpContext);
    Task<List<int>> GetUserIdsByDepartmentAsync(string? department);
    Task<Dictionary<int, UserDetailDto>> GetUserDetailsByIdsAsync(List<int> userIds);
}