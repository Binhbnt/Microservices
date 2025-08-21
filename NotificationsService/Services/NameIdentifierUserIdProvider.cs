using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace NotificationsService.Services;

public class NameIdentifierUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        // Lấy User ID từ claim "NameIdentifier" trong token
        return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}