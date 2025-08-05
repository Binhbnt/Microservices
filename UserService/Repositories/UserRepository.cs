using System.Data;
using Dapper;
using UserService.DTOs;
using UserService.Enums;
using UserService.Models;
using Npgsql;
using Microsoft.Extensions.Configuration;

namespace UserService.Repositories
{
    public class UserRepository : IUserRepository
    {
        // 1. Thay thế IDbConnection bằng chuỗi kết nối
        private readonly string _connectionString;

        // 2. Sửa constructor để nhận IConfiguration
        public UserRepository(IConfiguration configuration)
        {
            // Lấy chuỗi kết nối từ appsettings.json
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        // 3. Tạo một hàm private để tạo kết nối mới khi cần
        private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

        // Mỗi hàm bây giờ sẽ tự tạo và hủy kết nối riêng
        
        public async Task<IEnumerable<User>> GetAllAsync(bool? isDeleted = false)
        {
            using var connection = CreateConnection(); // Tạo kết nối
            string sql = "SELECT * FROM users";
            if (isDeleted.HasValue)
            {
                sql += " WHERE isdeleted = @IsDeleted";
                return await connection.QueryAsync<User>(sql, new { IsDeleted = isDeleted.Value });
            }
            return await connection.QueryAsync<User>(sql);
        }

        public async Task<User> CreateAsync(User user)
        {
            using var connection = CreateConnection(); // Tạo kết nối
            var sql = @"
            INSERT INTO users (masonhanvien, hoten, chucvu, bophan, email, matkhau, role, ngaytao, isdeleted, createdbyuserid, lastupdatedbyuserid, lastupdatedat, avatarurl)
            VALUES (@MaSoNhanVien, @HoTen, @ChucVu, @BoPhan, @Email, @MatKhau, @Role, @NgayTao, @IsDeleted, @CreatedByUserId, @LastUpdatedByUserId, @LastUpdatedAt, @AvatarUrl)
            RETURNING id;";

            var newId = await connection.QuerySingleAsync<int>(sql, user);
            user.Id = newId;
            return user;
        }

        public async Task<User?> GetByIdAsync(int id, bool includeDeleted = false)
        {
            using var connection = CreateConnection(); // Tạo kết nối
            var sql = "SELECT * FROM users WHERE id = @Id";
            if (!includeDeleted)
            {
                sql += " AND isdeleted = FALSE";
            }
            return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<List<User>> GetByIdsAsync(List<int> ids)
        {
            using var connection = CreateConnection(); // Tạo kết nối
            var sql = "SELECT * FROM users WHERE id = ANY(@Ids) AND isdeleted = FALSE"; // Dùng ANY() tốt hơn cho PostgreSQL
            return (await connection.QueryAsync<User>(sql, new { Ids = ids })).ToList();
        }

        public async Task UpdateProfileAsync(User user)
        {
            using var connection = CreateConnection(); // Tạo kết nối
            var sql = @"
            UPDATE users SET masonhanvien = @MaSoNhanVien, hoten = @HoTen, chucvu = @ChucVu,
                bophan = @BoPhan, email = @Email, role = @Role, lastupdatedbyuserid = @LastUpdatedByUserId, 
                lastupdatedat = @LastUpdatedAt, avatarurl = @AvatarUrl
            WHERE id = @Id";
            await connection.ExecuteAsync(sql, user);
        }

        public async Task UpdatePasswordAsync(int userId, string newHashedPassword)
        {
            using var connection = CreateConnection(); // Tạo kết nối
            var sql = "UPDATE users SET matkhau = @MatKhau, lastupdatedat = @LastUpdatedAt WHERE id = @Id";
            await connection.ExecuteAsync(sql, new { MatKhau = newHashedPassword, LastUpdatedAt = DateTime.UtcNow, Id = userId });
        }

        public async Task SoftDeleteAsync(int id)
        {
            using var connection = CreateConnection(); // Tạo kết nối
            var sql = "UPDATE users SET isdeleted = TRUE, lastupdatedat = @LastUpdatedAt WHERE id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id, LastUpdatedAt = DateTime.UtcNow });
        }

        public async Task DeletePermanentAsync(int id)
        {
            using var connection = CreateConnection(); // Tạo kết nối
            var sql = "DELETE FROM users WHERE id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task RestoreAsync(int id)
        {
            using var connection = CreateConnection(); // Tạo kết nối
            var sql = "UPDATE users SET isdeleted = FALSE, lastupdatedat = @LastUpdatedAt WHERE id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id, LastUpdatedAt = DateTime.UtcNow });
        }
        
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            using var connection = CreateConnection(); // Tạo kết nối
            var sql = "SELECT * FROM users WHERE email = @Email AND isdeleted = FALSE LIMIT 1";
            return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<User?> GetUserByMaSoNhanVienAsync(string maSoNhanVien)
        {
            using var connection = CreateConnection(); // Tạo kết nối
            var sql = "SELECT * FROM users WHERE masonhanvien = @MaSoNhanVien AND isdeleted = FALSE LIMIT 1";
            return await connection.QuerySingleOrDefaultAsync<User>(sql, new { MaSoNhanVien = maSoNhanVien });
        }

        public async Task<User?> FindUserForLoginAsync(string username)
        {
            using var connection = CreateConnection(); // Tạo kết nối
            var sql = "SELECT * FROM users WHERE (email = @Username OR masonhanvien = @Username) AND isdeleted = FALSE LIMIT 1";
            return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Username = username });
        }

        public async Task<User?> FindSuperUserInDepartmentAsync(string department)
        {
            using var connection = CreateConnection(); // Tạo kết nối
            var sql = "SELECT * FROM users WHERE bophan = @Department AND role = @Role AND isdeleted = FALSE LIMIT 1";
            return await connection.QuerySingleOrDefaultAsync<User>(sql, new
            {
                Department = department,
                Role = (int)UserRole.SuperUser
            });
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
        {
            using var connection = CreateConnection(); // Tạo kết nối
            var sql = "SELECT * FROM users WHERE role = @Role AND isdeleted = FALSE";
            return await connection.QueryAsync<User>(sql, new { Role = (int)role });
        }

        public async Task<IEnumerable<User>> GetByDepartmentAsync(string department)
        {
            using var connection = CreateConnection(); // Tạo kết nối
            var sql = "SELECT * FROM users WHERE bophan = @Department AND isdeleted = FALSE";
            return await connection.QueryAsync<User>(sql, new { Department = department });
        }
        
        public async Task<bool> DoesUserExistAsync(int id)
        {
            using var connection = CreateConnection(); // Tạo kết nối
            var sql = "SELECT 1 FROM users WHERE id = @Id";
            var result = await connection.QueryFirstOrDefaultAsync<int?>(sql, new { Id = id }); // Dùng FirstOrDefault để không lỗi nếu không tìm thấy
            return result.HasValue;
        }
    }
}