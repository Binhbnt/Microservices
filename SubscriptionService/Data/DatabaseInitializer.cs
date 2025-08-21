// File: Data/DatabaseInitializer.cs
using System.Data;
using Dapper;

namespace SubscriptionService.Data;

public class DatabaseInitializer
{
    private readonly IDbConnection _connection;

    public DatabaseInitializer(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task InitializeAsync()
    {
        // Câu lệnh SQL để tạo bảng 'subscribed_services' nếu nó chưa tồn tại.
        // Đây là nơi bạn định nghĩa cấu trúc của bảng.
        var sql = @"
        CREATE TABLE IF NOT EXISTS subscribe (
            id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            name VARCHAR(200) NOT NULL,
            sort_order INT NOT NULL DEFAULT 0,
            type INT NOT NULL,
            expiry_date TIMESTAMPTZ NOT NULL,
            provider VARCHAR(100),
            note TEXT,
            created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
            updated_at TIMESTAMPTZ
        );";
        
        // Dùng Dapper để thực thi câu lệnh SQL trên
        await _connection.ExecuteAsync(sql);
    }
}