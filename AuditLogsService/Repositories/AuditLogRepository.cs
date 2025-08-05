using System.Data;
using AuditLogService.Interface;
using AuditLogService.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace AuditLogService.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly string _connectionString;

        public AuditLogRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

        public async Task AddAsync(AuditLog log)
        {
            using var connection = CreateConnection();
            var sql = @"
                INSERT INTO auditlogs (Timestamp, UserId, Username, ActionType, EntityType, EntityId, Details, IsSuccess, ErrorMessage, RequesterIpAddress) 
                VALUES (@Timestamp, @UserId, @Username, @ActionType, @EntityType, @EntityId, @Details, @IsSuccess, @ErrorMessage, @RequesterIpAddress);";
            
            await connection.ExecuteAsync(sql, log);
        }

        public async Task<IEnumerable<AuditLog>> GetAllAsync()
        {
            using var connection = CreateConnection();
            var sql = "SELECT * FROM auditlogs ORDER BY Timestamp DESC";
            return await connection.QueryAsync<AuditLog>(sql);
        }
    }
}