using System.Data;
using System.Text;
using Dapper;
using LeaveRequestService.DTOs;
using LeaveRequestService.Enums;
using LeaveRequestService.Models;
using Microsoft.Extensions.Configuration; // Thêm using
using Npgsql; // Thêm using

namespace LeaveRequestService.Repositories;

public class LeaveRequestRepository : ILeaveRequestRepository
{
    private readonly string _connectionString;

    public LeaveRequestRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    public async Task<LeaveRequest> CreateAsync(LeaveRequest request)
    {
        using var connection = CreateConnection();
        var sql = @"
            INSERT INTO leaverequests (UserId, LoaiPhep, LyDo, NgayTu, NgayDen, GioTu, GioDen, CongViecBanGiao, TrangThai, NgayTao, CreatedByUserId, CreatorRole)
            VALUES (@UserId, @LoaiPhep, @LyDo, @NgayTu, @NgayDen, @GioTu, @GioDen, @CongViecBanGiao, @TrangThai, @NgayTao, @CreatedByUserId, @CreatorRole)
            RETURNING Id;";

        var newId = await connection.QuerySingleAsync<int>(sql, request);
        request.Id = newId;
        return request;
    }

    public async Task<(IEnumerable<LeaveRequest> Requests, int TotalCount)> GetAllAsync(
        string? status, List<int>? userIds, int? singleUserId,
        int pageNumber, int pageSize)
    {
        using var connection = CreateConnection();
        var parameters = new DynamicParameters();
        var whereConditions = new List<string> { "IsDeleted = FALSE" };

        if (singleUserId.HasValue)
        {
            whereConditions.Add("UserId = @UserId");
            parameters.Add("UserId", singleUserId.Value);
        }
        else if (userIds != null && userIds.Any())
        {
            whereConditions.Add("UserId = ANY(@UserIds)");
            parameters.Add("UserIds", userIds);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LeaveRequestStatus>(status, true, out var statusEnum))
        {
            whereConditions.Add("TrangThai = @Status");
            parameters.Add("Status", (int)statusEnum);
        }

        var whereClause = $"WHERE {string.Join(" AND ", whereConditions)}";

        var countSql = $"SELECT COUNT(*) FROM leaverequests {whereClause};";
        var selectSql = $@"SELECT * FROM leaverequests {whereClause} 
                           ORDER BY NgayTao DESC 
                           OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

        var combinedSql = countSql + selectSql;

        parameters.Add("Offset", (pageNumber - 1) * pageSize);
        parameters.Add("PageSize", pageSize);

        using (var multi = await connection.QueryMultipleAsync(combinedSql, parameters))
        {
            var totalCount = await multi.ReadSingleAsync<int>();
            var requests = await multi.ReadAsync<LeaveRequest>();
            return (requests, totalCount);
        }
    }

    public async Task<IEnumerable<LeaveRequest>> GetByUserIdAsync(int userId)
    {
        using var connection = CreateConnection();
        var sql = "SELECT * FROM leaverequests WHERE UserId = @UserId AND IsDeleted = FALSE ORDER BY NgayTao DESC";
        return await connection.QueryAsync<LeaveRequest>(sql, new { UserId = userId });
    }

    public async Task<IEnumerable<LeaveRequest>> GetByUserIdListAsync(List<int> userIds)
    {
        using var connection = CreateConnection();
        if (userIds == null || !userIds.Any())
        {
            return new List<LeaveRequest>();
        }

        var sql = "SELECT * FROM leaverequests WHERE UserId = ANY(@UserIds) AND IsDeleted = FALSE";
        return await connection.QueryAsync<LeaveRequest>(sql, new { UserIds = userIds });
    }

    public async Task<LeaveRequest?> GetByIdAsync(int id)
    {
        using var connection = CreateConnection();
        var sql = "SELECT * FROM leaverequests WHERE Id = @Id AND IsDeleted = FALSE";
        return await connection.QuerySingleOrDefaultAsync<LeaveRequest>(sql, new { Id = id });
    }

    public async Task UpdateAsync(LeaveRequest request)
    {
        using var connection = CreateConnection();
        var sql = @"
            UPDATE leaverequests SET
                TrangThai = @TrangThai, LyDoXuLy = @LyDoXuLy, DaGuiN8n = @DaGuiN8n,
                ApprovalToken = @ApprovalToken, ApprovalTokenExpires = @ApprovalTokenExpires,
                RevocationToken = @RevocationToken, RevocationTokenExpires = @RevocationTokenExpires,
                LastUpdatedAt = @LastUpdatedAt, LastUpdatedByUserId = @LastUpdatedByUserId
            WHERE Id = @Id;";
        await connection.ExecuteAsync(sql, request);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = CreateConnection();
        var sql = "UPDATE leaverequests SET IsDeleted = TRUE, LastUpdatedAt = @LastUpdatedAt WHERE Id = @Id";
        await connection.ExecuteAsync(sql, new { Id = id, LastUpdatedAt = DateTime.UtcNow });
    }

    public async Task<LeaveRequest?> FindByApprovalTokenAsync(string token)
    {
        using var connection = CreateConnection();
        var sql = "SELECT * FROM leaverequests WHERE ApprovalToken = @Token";
        return await connection.QuerySingleOrDefaultAsync<LeaveRequest>(sql, new { Token = token });
    }

    public async Task<LeaveRequest?> FindByRevocationTokenAsync(string token)
    {
        using var connection = CreateConnection();
        var sql = "SELECT * FROM leaverequests WHERE RevocationToken = @Token";
        return await connection.QuerySingleOrDefaultAsync<LeaveRequest>(sql, new { Token = token });
    }

    public async Task<IEnumerable<LeaveRequest>> GetByUserIdListAsync(List<int> userIds, string? status)
    {
        using var connection = CreateConnection();
        var sql = new StringBuilder("SELECT * FROM leaverequests WHERE UserId = ANY(@UserIds) AND IsDeleted = FALSE");
        var parameters = new DynamicParameters();
        parameters.Add("UserIds", userIds);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LeaveRequestStatus>(status, true, out var statusEnum))
        {
            sql.Append(" AND TrangThai = @Status");
            parameters.Add("Status", statusEnum);
        }

        return await connection.QueryAsync<LeaveRequest>(sql.ToString(), parameters);
    }

    public async Task<IEnumerable<LeaveRequest>> GetApprovedLeaveForUsersAsync(List<int> userIds, int year)
    {
        if (userIds == null || !userIds.Any())
        {
            return Enumerable.Empty<LeaveRequest>();
        }
        using var connection = CreateConnection();
        var sql = @"SELECT * FROM leaverequests 
                WHERE UserId = ANY(@UserIds) 
                AND TrangThai = @Status 
                AND EXTRACT(YEAR FROM NgayTu) = @Year";
        return await connection.QueryAsync<LeaveRequest>(sql, new
        {
            UserIds = userIds,
            Status = (int)LeaveRequestStatus.Approved,
            Year = year
        });
    }
}