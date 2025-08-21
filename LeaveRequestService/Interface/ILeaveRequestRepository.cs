using LeaveRequestService.Models;

namespace LeaveRequestService.Repositories;

public interface ILeaveRequestRepository
{
    Task<LeaveRequest> CreateAsync(LeaveRequest request);
    Task<LeaveRequest?> GetByIdAsync(int id);
    Task<(IEnumerable<LeaveRequest> Requests, int TotalCount)> GetAllAsync(
        string? status, 
        List<int>? userIds, // Dùng để lọc cho Admin/SuperUser
        int? singleUserId,   // Dùng để lọc cho User thường
        int pageNumber, 
        int pageSize);
    Task<IEnumerable<LeaveRequest>> GetByUserIdListAsync(List<int> userIds);
    Task<IEnumerable<LeaveRequest>> GetByUserIdListAsync(List<int> userIds, string? status);
    Task<IEnumerable<LeaveRequest>> GetByUserIdAsync(int userId);
    Task<IEnumerable<LeaveRequest>> GetApprovedLeaveForUsersAsync(List<int> userIds, int year);
    Task UpdateAsync(LeaveRequest request);
    Task DeleteAsync(int id); // Xóa mềm
    Task<LeaveRequest?> FindByApprovalTokenAsync(string token);
    Task<LeaveRequest?> FindByRevocationTokenAsync(string token);
}