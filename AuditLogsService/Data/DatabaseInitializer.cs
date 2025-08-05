using System.Data;
using Dapper;
using Npgsql;

namespace AuditLogService.Data
{
    public class DatabaseInitializer
    {
        private readonly IDbConnection _connection;
        public DatabaseInitializer(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task InitializeAsync()
        {
            var sql = @"
            CREATE TABLE IF NOT EXISTS auditlogs (
                Id SERIAL PRIMARY KEY,
                Timestamp TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                UserId INT NULL,
                Username VARCHAR(255) NULL,
                ActionType VARCHAR(100) NOT NULL,
                EntityType VARCHAR(100) NULL,
                EntityId INT NULL,
                Details TEXT NULL,
                IsSuccess BOOLEAN NOT NULL,
                ErrorMessage TEXT NULL,
                RequesterIpAddress VARCHAR(50) NULL
            );";
            
            await _connection.ExecuteAsync(sql);
        }
    }
}