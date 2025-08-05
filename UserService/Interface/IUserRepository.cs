using UserService.DTOs;
using UserService.Enums;
using UserService.Models;

namespace UserService.Repositories
{
    public interface IUserRepository
    {
        // Hợp đồng này yêu cầu phải có một phương thức GetAllAsync
        // để lấy tất cả user đang hoạt động.
        Task<IEnumerable<User>> GetAllAsync(bool? isDeleted = false);
        Task<User> CreateAsync(User user);
        Task<User?> GetByIdAsync(int id, bool includeDeleted = false);
        Task<List<User>> GetByIdsAsync(List<int> ids);
        //Task UpdateAsync(User user);
        Task SoftDeleteAsync(int id);
        Task DeletePermanentAsync(int id);

        // Kiểm tra xem một user có tồn tại trong bảng không, bất kể trạng thái IsDeleted
        Task<bool> DoesUserExistAsync(int id);
        Task RestoreAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByMaSoNhanVienAsync(string maSoNhanVien);
        Task<User?> FindUserForLoginAsync(string username);
        Task UpdateProfileAsync(User user); // Chỉ cập nhật thông tin cá nhân
        Task UpdatePasswordAsync(int userId, string newHashedPassword); // Chỉ cập nhật mật khẩu
        Task<User?> FindSuperUserInDepartmentAsync(string department);
        Task<IEnumerable<User>> GetByRoleAsync(UserRole role);
        Task<IEnumerable<User>> GetByDepartmentAsync(string department);

    }
}