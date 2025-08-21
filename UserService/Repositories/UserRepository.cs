using System.Data;
using Dapper;
using UserService.DTOs;
using UserService.Enums;
using UserService.Models;
using Npgsql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging; 

namespace UserService.Repositories
{
    public class UserRepository : IUserRepository
    {
        // 1. Thay thế IDbConnection bằng chuỗi kết nối
        private readonly string _connectionString;
        private readonly ILogger<UserRepository> _logger;

        // 2. Sửa constructor để nhận IConfiguration
        public UserRepository(IConfiguration configuration, ILogger<UserRepository> logger)
        {
            // Lấy chuỗi kết nối từ appsettings.json
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _logger = logger;
        }

        // 3. Tạo một hàm private để tạo kết nối mới khi cần
        private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

        // Mỗi hàm bây giờ sẽ tự tạo và hủy kết nối riêng
        
        public async Task<(IEnumerable<User> Users, int TotalCount)> GetAllAsync(
            string? searchTerm, string? role, bool? isDeleted,
            string? requesterRole, string? requesterDepartment,
            int pageNumber, int pageSize)
        {
            // === LOGGING BẮT ĐẦU ===
            _logger.LogInformation("--- Starting UserRepository.GetAllAsync ---");
            _logger.LogInformation("Parameters: searchTerm={searchTerm}, role={role}, isDeleted={isDeleted}, requesterRole={requesterRole}, requesterDepartment={requesterDepartment}, pageNumber={pageNumber}, pageSize={pageSize}", 
                searchTerm, role, isDeleted, requesterRole, requesterDepartment, pageNumber, pageSize);
            // === LOGGING KẾT THÚC ===

            using var connection = CreateConnection();
            var parameters = new DynamicParameters();
            var whereConditions = new List<string>();
            
            if (isDeleted.HasValue)
            {
                whereConditions.Add("isdeleted = @IsDeleted");
                parameters.Add("IsDeleted", isDeleted.Value);
            }

            if (requesterRole == "SuperUser" && !string.IsNullOrEmpty(requesterDepartment))
            {
                whereConditions.Add("bophan = @Department");
                parameters.Add("Department", requesterDepartment);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                whereConditions.Add("(masonhanvien ILIKE @SearchTerm OR hoten ILIKE @SearchTerm OR email ILIKE @SearchTerm)");
                parameters.Add("SearchTerm", $"%{searchTerm}%");
            }

            if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<UserRole>(role, true, out var userRoleEnum))
            {
                whereConditions.Add("role = @Role");
                parameters.Add("Role", (int)userRoleEnum);
            }

            var whereClause = whereConditions.Any() ? $"WHERE {string.Join(" AND ", whereConditions)}" : "";

            var countSql = $"SELECT COUNT(*) FROM users {whereClause};";

            var selectSql = $@"
                SELECT 
                    id::integer AS Id, 
                    masonhanvien AS MaSoNhanVien,
                    hoten AS HoTen,
                    chucvu AS ChucVu,
                    bophan AS BoPhan,
                    email AS Email,
                    matkhau AS MatKhau,
                    role::integer AS Role,
                    ngaytao AS NgayTao,
                    isdeleted AS IsDeleted,
                    createdbyuserid::integer AS CreatedByUserId,
                    lastupdatedbyuserid::integer AS LastUpdatedByUserId,
                    lastupdatedat AS LastUpdatedAt,
                    avatarurl AS AvatarUrl
                FROM users {whereClause} 
                ORDER BY id 
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";
            
            var combinedSql = countSql + selectSql;

            parameters.Add("Offset", (pageNumber - 1) * pageSize);
            parameters.Add("PageSize", pageSize);
            
            _logger.LogInformation("Generated SQL Query: \n{sql}", combinedSql);

            using (var multi = await connection.QueryMultipleAsync(combinedSql, parameters))
            {
                var totalCount = await multi.ReadSingleAsync<int>();
                var users = await multi.ReadAsync<User>();
                _logger.LogInformation("Query Result: TotalCount = {totalCount}, Fetched User Count = {userCount}", totalCount, users.Count());
                _logger.LogInformation("--- Finished UserRepository.GetAllAsync ---");
                return (users, totalCount);
            }
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