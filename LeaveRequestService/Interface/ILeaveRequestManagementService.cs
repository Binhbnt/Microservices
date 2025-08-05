using System.Security.Claims;
using LeaveRequestService.DTOs;

namespace LeaveRequestService.Interface;

public interface ILeaveRequestManagementService
{
    Task<LeaveRequestReadDto> CreateAsync(LeaveRequestCreateDto dto, ClaimsPrincipal requester);
    Task<IEnumerable<LeaveRequestReadDto>> GetAllAsync(ClaimsPrincipal requester, string? searchTerm, string? status);
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
    Task<IEnumerable<UserDetailDto>> GetAllUsersWithLeaveDetailsAsync(ClaimsPrincipal requester, bool isDeleted, string? searchTerm, string? role);}