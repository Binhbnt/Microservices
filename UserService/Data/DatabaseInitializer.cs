using System.Data;
using Dapper;
using Npgsql;

namespace UserService.Data
{
    public class DatabaseInitializer
    {
        private readonly IDbConnection _connection;
        public DatabaseInitializer(IDbConnection connection)
        {
            _connection = connection;
        }

        // Chuyển phương thức thành async và trả về Task
        public async Task InitializeAsync()
        {
            var sql = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id SERIAL PRIMARY KEY,
                MaSoNhanVien VARCHAR(50) NOT NULL UNIQUE,
                HoTen VARCHAR(255) NOT NULL,
                ChucVu VARCHAR(255) NULL,
                BoPhan VARCHAR(255) NULL,
                Email VARCHAR(255) UNIQUE,
                MatKhau TEXT NULL,
                Role INT NOT NULL DEFAULT 2,
                NgayTao TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                IsDeleted BOOLEAN NOT NULL DEFAULT FALSE,
                CreatedByUserId INT NULL,
                LastUpdatedByUserId INT NULL,
                LastUpdatedAt TIMESTAMPTZ NULL,
                AvatarUrl TEXT NULL
            );";

            // Dùng ExecuteAsync thay vì Execute
            await _connection.ExecuteAsync(sql);
        }
    }
}