using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NotificationsService.Models;
using NotificationsService.Interface;
using Npgsql; 
using Microsoft.Extensions.Configuration;

namespace NotificationsService.Repositories
{
    public class AppNotificationRepository : IAppNotificationRepository
    {
        private readonly string _connectionString;

        public AppNotificationRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

        public async Task<AppNotification> CreateAsync(AppNotification notification)
        {
            using var connection = CreateConnection();
            var sql = @"
                INSERT INTO appnotifications (UserId, Message, IsRead, Url, CreatedAt, TriggeredByUserId, TriggeredByUsername)
                VALUES (@UserId, @Message, @IsRead, @Url, @CreatedAt, @TriggeredByUserId, @TriggeredByUsername)
                RETURNING Id;";
            
            var newId = await connection.QuerySingleAsync<int>(sql, notification);
            notification.Id = newId;
            return notification;
        }

        public async Task<IEnumerable<AppNotification>> GetByUserIdsAsync(List<int> userIds)
        {
            using var connection = CreateConnection();
            if (userIds == null || !userIds.Any())
            {
                return new List<AppNotification>();
            }

            var sql = "SELECT * FROM appnotifications WHERE UserId = ANY(@UserIds) ORDER BY CreatedAt DESC";
            return await connection.QueryAsync<AppNotification>(sql, new { UserIds = userIds });
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            using var connection = CreateConnection();
            var sql = "UPDATE appnotifications SET IsRead = TRUE WHERE UserId = @UserId AND IsRead = FALSE";
            var affectedRows = await connection.ExecuteAsync(sql, new { UserId = userId });
            return affectedRows > 0;
        }

        public async Task<IEnumerable<AppNotification>> GetAllAsync()
        {
            using var connection = CreateConnection();
            var sql = "SELECT * FROM appnotifications ORDER BY CreatedAt DESC";
            return await connection.QueryAsync<AppNotification>(sql);
        }

        public async Task<(IEnumerable<AppNotification>, int)> GetAllPaginatedForUserIdsAsync(List<int> userIds, int pageNumber, int pageSize)
        {
            using var connection = CreateConnection();
            var sql = "SELECT * FROM appnotifications WHERE UserId = ANY(@UserIds) ORDER BY CreatedAt DESC LIMIT @PageSize OFFSET @Offset";
            var countSql = "SELECT COUNT(*) FROM appnotifications WHERE UserId = ANY(@UserIds)";

            var parameters = new
            {
                UserIds = userIds,
                PageSize = pageSize,
                Offset = (pageNumber - 1) * pageSize
            };

            var notifications = await connection.QueryAsync<AppNotification>(sql, parameters);
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { UserIds = userIds });

            return (notifications, totalCount);
        }
        
        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            using var connection = CreateConnection();
            var sql = "UPDATE appnotifications SET IsRead = TRUE WHERE Id = @Id AND IsRead = FALSE";
            var affectedRows = await connection.ExecuteAsync(sql, new { Id = notificationId });
            return affectedRows > 0;
        }
    }
}