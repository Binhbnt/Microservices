using System.Security.Claims;
using System.Text.Json;
using LeaveRequestService.DTOs;
using LeaveRequestService.Interface;

namespace LeaveRequestService.Services;

public class UserServiceClient : IUserServiceClient
{
    private readonly HttpClient _httpClient;

    public UserServiceClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("UserClient");
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(HttpContext httpContext)
    {
        var token = httpContext.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(token)) return null;

        var request = new HttpRequestMessage(HttpMethod.Get, "http://api-gateway:8080/api/users/current");
        request.Headers.Add("Authorization", token);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CurrentUserDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
