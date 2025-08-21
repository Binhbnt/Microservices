using System.Security.Claims;
using LeaveRequestService.DTOs;

namespace LeaveRequestService.Interface;

public interface ILeaveRequestManagementService
{
    Task<LeaveRequestReadDto> CreateAsync(LeaveRequestCreateDto dto, ClaimsPrincipal requester);
    Task<PaginatedResultDto<LeaveRequestReadDto>> GetAllAsync(
        ClaimsPrincipal requester, string? searchTerm, string? status, 
        int pageNumber, int pageSize);
    Task<LeaveRequestReadDto?> GetByIdAsync(int id, ClaimsPrincipal requester);
    Task<bool> UpdateStatusAsync(int id, LeaveRequestUpdateStatusDto dto, ClaimsPrincipal requester);
    Task<bool> CancelAsync(int id, ClaimsPrincipal requester);
    Task<bool> SendForApprovalAsync(int id, ClaimsPrincipal requester);
    Task<bool> ProcessApprovalAsync(ProcessApprovalDto dto);
    Task<LeaveRequestReadDto> ResubmitAsync(int id, LeaveRequestCreateDto dto, ClaimsPrincipal requester);
    Task<bool> RequestRevocationAsync(int id, ClaimsPrincipal requester);
    Task<bool> ProcessRevocationAsync(ProcessRevocationDto dto);
    Task<byte[]> ExportLeaveRequestsToExcelAsync(ClaimsPrincipal requester, string? searchTerm = null, string? role = null);
    Task<LeaveRequestDashboardStatsDto> GetDashboardStatsAsync(ClaimsPrincipal requester);
    Task<PaginatedResultDto<UserDetailDto>> GetAllUsersWithLeaveDetailsAsync(
        ClaimsPrincipal requester,
        bool isDeleted,
        string? searchTerm,
        string? role,
        int pageNumber,
        int pageSize);
}