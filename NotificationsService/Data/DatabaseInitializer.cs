using System.Data;
using Dapper;
using Npgsql;

namespace NotificationsService.Data
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
            CREATE TABLE IF NOT EXISTS appnotifications (
                Id SERIAL PRIMARY KEY,
                UserId INT NOT NULL,
                Message TEXT NOT NULL,
                IsRead BOOLEAN NOT NULL DEFAULT FALSE,
                Url TEXT NULL,
                CreatedAt TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                TriggeredByUserId INT NULL,
                TriggeredByUsername VARCHAR(255) NULL
            );";
            
            await _connection.ExecuteAsync(sql);
        }
    }
}