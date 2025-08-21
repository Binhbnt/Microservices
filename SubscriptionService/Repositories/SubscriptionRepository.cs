// File: Repositories/SubscriptionRepository.cs
using System.Data;
using Dapper;
using Npgsql;
using SubscriptionService.Interface;
using SubscriptionService.Models;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using SubscriptionService.Dtos;

namespace SubscriptionService.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly string _connectionString;

    // Lấy chuỗi kết nối từ appsettings.json
    public SubscriptionRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    // Hàm tiện ích để tạo kết nối mới
    private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    // Lấy tất cả dịch vụ, sắp xếp theo ngày hết hạn gần nhất
    public async Task<(IEnumerable<SubscribedService> Services, int TotalCount)> GetAllAsync(string? searchTerm, int? type, int pageNumber, int pageSize)
    {
        using var connection = CreateConnection();
        var sqlBuilder = new StringBuilder();
        var parameters = new DynamicParameters();

        // --- Xây dựng phần WHERE chung cho cả 2 câu query ---
        var whereClause = new StringBuilder();
        var conditions = new List<string>();

        if (type.HasValue)
        {
            conditions.Add("type = @Type");
            parameters.Add("Type", type.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            conditions.Add("(name ILIKE @SearchTerm OR provider ILIKE @SearchTerm)");
            parameters.Add("SearchTerm", $"%{searchTerm}%");
        }

        if (conditions.Any())
        {
            whereClause.Append(" WHERE ").Append(string.Join(" AND ", conditions));
        }

        // --- Tạo 2 câu query: 1 để đếm, 1 để lấy dữ liệu phân trang ---
        sqlBuilder.AppendLine($"SELECT COUNT(*) FROM subscribe {whereClause};");
        sqlBuilder.AppendLine($@"
            SELECT 
                id AS Id, name AS Name, type AS Type, expiry_date AS ExpiryDate,
                provider AS Provider, note AS Note, created_at AS CreatedAt,
                updated_at AS UpdatedAt,
                sort_order as SortOrder
            FROM subscribe
            {whereClause}
            ORDER BY sort_order ASC, name ASC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
        ");

        parameters.Add("Offset", (pageNumber - 1) * pageSize);
        parameters.Add("PageSize", pageSize);

        // --- Thực thi 2 câu query cùng lúc ---
        using (var multi = await connection.QueryMultipleAsync(sqlBuilder.ToString(), parameters))
        {
            var totalCount = await multi.ReadSingleAsync<int>();
            var services = await multi.ReadAsync<SubscribedService>();
            return (services, totalCount);
        }
    }

    // Lấy một dịch vụ theo ID
    public async Task<SubscribedService?> GetByIdAsync(Guid id)
    {
        using var connection = CreateConnection();
        // THAY THẾ BẰNG CÂU LỆNH SELECT ĐẦY ĐỦ VỚI CÁC ALIAS (AS)
        const string sql = @"
        SELECT 
            id AS Id, 
            name AS Name, 
            type AS Type, 
            expiry_date AS ExpiryDate,
            provider AS Provider, 
            note AS Note, 
            created_at AS CreatedAt,
            updated_at AS UpdatedAt
        FROM subscribe 
        WHERE id = @Id";

        return await connection.QuerySingleOrDefaultAsync<SubscribedService>(sql, new { Id = id });
    }

    // Thêm một dịch vụ mới vào database
    public async Task<SubscribedService> CreateAsync(SubscribedService service)
    {
        using var connection = CreateConnection();
        const string sql = @"
    INSERT INTO subscribe (name, type, expiry_date, provider, note, created_at, updated_at, sort_order)
    VALUES (@Name, @Type, @ExpiryDate, @Provider, @Note, @CreatedAt, @UpdatedAt, @SortOrder)
    RETURNING id;";

        var newId = await connection.QuerySingleAsync<Guid>(sql, service);
        service.Id = newId;
        return service;
    }

    // Cập nhật một dịch vụ đã có
    public async Task<bool> UpdateAsync(SubscribedService service)
    {
        using var connection = CreateConnection();
        const string sql = @"
            UPDATE subscribe SET
                name = @Name,
                type = @Type,
                expiry_date = @ExpiryDate,
                provider = @Provider,
                note = @Note,
                updated_at = @UpdatedAt
            WHERE id = @Id";

        var affectedRows = await connection.ExecuteAsync(sql, service);
        return affectedRows > 0;
    }

    // Xóa một dịch vụ khỏi database
    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = CreateConnection();
        const string sql = "DELETE FROM subscribe WHERE id = @Id";
        var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
        return affectedRows > 0;
    }

    public async Task<int> CreateBatchAsync(IEnumerable<SubscribedService> services)
    {
        using var connection = CreateConnection();
        // Dapper rất thông minh, nó sẽ chạy lệnh INSERT cho mỗi item trong list
        const string sql = @"
    INSERT INTO subscribe (name, type, expiry_date, provider, note, created_at, updated_at, sort_order)
    VALUES (@Name, @Type, @ExpiryDate, @Provider, @Note, @CreatedAt, @UpdatedAt, @SortOrder);";

        return await connection.ExecuteAsync(sql, services);
    }

    public async Task<IEnumerable<SubscriptionStatsDto>> GetStatsByTypeAsync()
    {
        using var connection = CreateConnection();
        // Xóa dòng "WHERE isdeleted = FALSE" để thống kê tất cả các bản ghi đang tồn tại
        var sql = @"SELECT type::text AS Type, COUNT(id) AS Count 
                FROM subscribe 
                GROUP BY type;";
        return await connection.QueryAsync<SubscriptionStatsDto>(sql);
    }
    public async Task<IEnumerable<SubscribedService>> GetExpiringSubscriptionsAsync(int daysUntilExpiry)
    {
        using var connection = CreateConnection();
        // Lấy các dịch vụ có ngày hết hạn nằm trong khoảng từ hôm nay đến X ngày tới
        const string sql = @"
            SELECT 
                id AS Id, name AS Name, type AS Type, expiry_date AS ExpiryDate,
                provider AS Provider, note AS Note, created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM subscribe
            WHERE expiry_date BETWEEN NOW() AND NOW() + MAKE_INTERVAL(days => @Days);";

        return await connection.QueryAsync<SubscribedService>(sql, new { Days = daysUntilExpiry });
    }

    public async Task<int> GetMaxSortOrderAsync()
    {
        using var connection = CreateConnection();
        // COALESCE sẽ trả về 0 nếu bảng trống (MAX trả về NULL)
        const string sql = "SELECT COALESCE(MAX(sort_order), 0) FROM subscribe;";
        return await connection.QuerySingleAsync<int>(sql);
    }
}