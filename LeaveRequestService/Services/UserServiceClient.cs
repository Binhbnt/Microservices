using System.Security.Claims;
using System.Text.Json;
using LeaveRequestService.DTOs;
using LeaveRequestService.Interface;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace LeaveRequestService.Services;

public class UserServiceClient : IUserServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserServiceClient> _logger;

    public UserServiceClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, ILogger<UserServiceClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private HttpClient CreateClientWithAuth()
    {
        var client = _httpClientFactory.CreateClient("UserClient");
        var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(token))
        {
            // Tránh thêm lặp lại header nếu đã có
            if (client.DefaultRequestHeaders.Authorization == null)
            {
                client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(token);
            }
        }
        return client;
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(HttpContext httpContext)
    {
        var client = CreateClientWithAuth();
        // Giả sử endpoint này được chuyển đến /api/users/current ở gateway
        var response = await client.GetAsync("/api/users/current"); 
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CurrentUserDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<List<int>> GetUserIdsByDepartmentAsync(string? department)
    {
        if (string.IsNullOrEmpty(department)) return new List<int>();
        try
        {
            var client = CreateClientWithAuth();
            var encodedDepartment = Uri.EscapeDataString(department);
            var result = await client.GetFromJsonAsync<List<int>>($"/api/Users/by-department?department={encodedDepartment}");
            return result ?? new List<int>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách user ID từ bộ phận {Department}", department);
            return new List<int>();
        }
    }

    public async Task<Dictionary<int, UserDetailDto>> GetUserDetailsByIdsAsync(List<int> userIds)
    {
        if (userIds == null || !userIds.Any()) return new Dictionary<int, UserDetailDto>();
        try
        {
            var client = CreateClientWithAuth();
            var response = await client.PostAsJsonAsync("/api/Users/get-by-ids", userIds);
            if (response.IsSuccessStatusCode)
            {
                var userList = await response.Content.ReadFromJsonAsync<List<UserDetailDto>>();
                return userList?.ToDictionary(u => u.Id) ?? new Dictionary<int, UserDetailDto>();
            }
            return new Dictionary<int, UserDetailDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gọi UserService để lấy chi tiết user.");
            return new Dictionary<int, UserDetailDto>();
        }
    }
}
