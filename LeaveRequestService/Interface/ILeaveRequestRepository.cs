using LeaveRequestService.Models;

namespace LeaveRequestService.Repositories;

public interface ILeaveRequestRepository
{
    Task<LeaveRequest> CreateAsync(LeaveRequest request);
    Task<LeaveRequest?> GetByIdAsync(int id);
    Task<IEnumerable<LeaveRequest>> GetAllAsync(string? searchTerm,string? status);
    Task<IEnumerable<LeaveRequest>> GetByUserIdListAsync(List<int> userIds);
    Task<IEnumerable<LeaveRequest>> GetByUserIdListAsync(List<int> userIds, string? status);
    Task<IEnumerable<LeaveRequest>> GetByUserIdAsync(int userId);
    Task UpdateAsync(LeaveRequest request);
    Task DeleteAsync(int id); // Xóa mềm
    Task<LeaveRequest?> FindByApprovalTokenAsync(string token);
    Task<LeaveRequest?> FindByRevocationTokenAsync(string token);
}