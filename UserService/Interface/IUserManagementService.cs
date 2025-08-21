using System.Security.Claims;
using UserService.DTOs;
using UserService.Enums;
using UserService.Models;

namespace UserService.Services
{

    public interface IUserManagementService
    {
        // Service sẽ trả về một danh sách các DTO an toàn, không phải Model.
        Task<PaginatedResultDto<UserReadDto>> GetAllAsync(
        string? searchTerm, string? role, bool? isDeleted,
        string? requesterRole, string? requesterDepartment,
        int pageNumber, int pageSize);
        Task<UserReadDto> CreateAsync(UserCreateDto userCreateDto, ClaimsPrincipal requester);
        Task<UserReadDto?> GetByIdAsync(int id);
        Task<List<UserDetailDto>> GetByIdsAsync(List<int> ids);
        // Trả về true nếu cập nhật thành công, false nếu không tìm thấy user.
        Task<bool> UpdateAsync(int id, UserUpdateDto userUpdateDto, ClaimsPrincipal requester);
        Task<bool> SoftDeleteAsync(int id, ClaimsPrincipal requester);
        Task<bool> DeletePermanentAsync(int id, ClaimsPrincipal requester);
        Task<bool> RestoreAsync(int id, ClaimsPrincipal requester);
        Task<byte[]> ExportUsersToExcelAsync(string? searchTerm = null, string? role = null, bool? isDeleted = false);
        Task<User?> ValidateUserCredentialsAsync(string username, string password);
        Task<string> GenerateJwtToken(User user);
        Task<LoginResponseDto?> AuthenticateUserAsync(LoginDto loginDto); // Phương thức tổng hợp
        Task<string?> RenewTokenAsync(int userId); // Nhận userId từ token hiện tại
        Task ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<string> UpdateAvatarAsync(int userId, IFormFile avatarFile);
        Task<IEnumerable<UserReadDto>> GetByRoleAsync(UserRole role);
        Task<UserReadDto?> FindSuperUserInDepartmentAsync(string department);
	Task<IEnumerable<UserReadDto>> GetUsersByDepartmentAsync(string department);

    }
}