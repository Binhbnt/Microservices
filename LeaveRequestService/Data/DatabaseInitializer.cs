using System.Data;
using Dapper;
using Npgsql;

namespace LeaveRequestService.Data
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
            CREATE TABLE IF NOT EXISTS leaverequests (
                Id SERIAL PRIMARY KEY,
                UserId INT NOT NULL,
                LoaiPhep VARCHAR(100) NOT NULL,
                LyDo TEXT,
                NgayTu TIMESTAMPTZ NOT NULL,
                NgayDen TIMESTAMPTZ NOT NULL,
                GioTu VARCHAR(10),
                GioDen VARCHAR(10),
                CongViecBanGiao TEXT,
                TrangThai INT NOT NULL,
                LyDoXuLy TEXT,
                DaGuiN8n BOOLEAN DEFAULT FALSE,
                IsDeleted BOOLEAN DEFAULT FALSE,
                NgayTao TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                LastUpdatedAt TIMESTAMPTZ,
                CreatedByUserId INT,
                LastUpdatedByUserId INT,
                CreatorRole INT,
                ApprovalToken TEXT,              
                ApprovalTokenExpires TIMESTAMPTZ,
                RevocationToken TEXT,
                RevocationTokenExpires TIMESTAMPTZ
            );";
            
            await _connection.ExecuteAsync(sql);
        }
    }
}