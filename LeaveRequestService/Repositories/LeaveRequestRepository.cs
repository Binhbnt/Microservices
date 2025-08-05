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

    public async Task<IEnumerable<LeaveRequest>> GetAllAsync(string? searchTerm, string? status)
    {
        using var connection = CreateConnection();
        var sqlBuilder = new StringBuilder("SELECT * FROM leaverequests WHERE IsDeleted = FALSE");
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (Enum.TryParse<LeaveRequestStatus>(status, true, out var statusEnum))
            {
                sqlBuilder.Append(" AND TrangThai = @Status");
                parameters.Add("Status", statusEnum);
            }
        }

        sqlBuilder.Append(" ORDER BY NgayTao DESC");

        return await connection.QueryAsync<LeaveRequest>(sqlBuilder.ToString(), parameters);
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
}