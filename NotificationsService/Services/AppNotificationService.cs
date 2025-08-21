using AutoMapper;
using NotificationsService.DTOs;
using NotificationsService.Models;
using NotificationsService.Repositories;
using NotificationsService.Services;
using NotificationsService.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using NotificationsService.Hubs;
using NotificationsService.Helpers;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace NotificationsService.Services;

public class AppNotificationService : IAppNotificationService
{
    private readonly IAppNotificationRepository _repository;
    private readonly IMapper _mapper;
    private readonly IHttpClientFactory _httpClientFactory; // Để gọi sang UserService
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<AppNotificationService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AppNotificationService(IAppNotificationRepository repository, IMapper mapper,
    IHttpClientFactory httpClientFactory, ILogger<AppNotificationService> logger,
    IHubContext<NotificationHub> hubContext, IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _mapper = mapper;
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task CreateNotificationAsync(AppNotificationCreateDto dto)
    {
        // 3. Logic tạo thông báo
        var notification = _mapper.Map<AppNotification>(dto);
        notification.IsRead = false;
        notification.CreatedAt = DateTime.UtcNow;

        var createdNotification = await _repository.CreateAsync(notification);

        // 4. ĐẨY THÔNG BÁO REAL-TIME XUỐNG CHO USER CỤ THỂ
        var notificationDto = _mapper.Map<AppNotificationReadDto>(createdNotification);

        // Gửi đến user có UserId trùng với UserId trong DTO
        // Cần thêm UserIdProvider để SignalR biết User ID là gì (xem Bước 4)
        await _hubContext.Clients.User(dto.UserId.ToString())
                         .SendAsync("ReceiveNotification", notificationDto);
    }

    public async Task<AppNotificationStateDto> GetNotificationStateAsync(ClaimsPrincipal user)
    {
        var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        IEnumerable<AppNotification> notifications; // Dùng IEnumerable để linh hoạt

        if (userRoles.Contains("Admin"))
        {
            // Admin thấy tất cả thông báo
            notifications = await _repository.GetAllAsync(); // <-- GỌI HÀM MỚI
        }
        else if (userRoles.Contains("SuperUser"))
        {
            var department = user.FindFirstValue("Department");
            // Lấy danh sách user trong cùng bộ phận
            var userIdsToFetch = await GetUserIdsInDepartment(department);
            // Luôn bao gồm cả chính SuperUser
            if (!userIdsToFetch.Contains(userId))
            {
                userIdsToFetch.Add(userId);
            }
            notifications = await _repository.GetByUserIdsAsync(userIdsToFetch);
        }
        else
        {
            // User thường chỉ thấy của mình
            var userIdsToFetch = new List<int> { userId };
            notifications = await _repository.GetByUserIdsAsync(userIdsToFetch);
        }

        return new AppNotificationStateDto
        {
            UnreadCount = notifications.Count(n => !n.IsRead),
            // Lấy 10 thông báo mới nhất để hiển thị trong dropdown
            RecentNotifications = _mapper.Map<List<AppNotificationReadDto>>(
                     notifications.OrderByDescending(n => n.CreatedAt).Take(10))
        };
    }

    public async Task MarkAllAsReadAsync(ClaimsPrincipal user)
    {
        var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _repository.MarkAllAsReadAsync(userId);
    }

    // Hàm giả định, bạn cần triển khai logic thực tế để gọi sang UserService
    private async Task<List<int>> GetUserIdsInDepartment(string department)
    {
        // 1. Luôn kiểm tra đầu vào để tránh lỗi không cần thiết
        if (string.IsNullOrWhiteSpace(department))
        {
            return new List<int>();
        }

        try
        {
            // 2. Tạo HttpClient đã được cấu hình sẵn để gọi sang UserService
            var client = _httpClientFactory.CreateClient("UserClient");

            // 3. Lấy và chuyển tiếp token xác thực từ request gốc
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
            {
                // Đảm bảo chỉ lấy phần token, loại bỏ chữ "Bearer " nếu có
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Split(" ").Last());
            }

            // 4. Mã hóa tên bộ phận để đảm bảo URL luôn hợp lệ
            var encodedDepartment = Uri.EscapeDataString(department);

            // 5. Gọi chính xác đến endpoint mới trong UserService và lấy về danh sách ID
            var userIds = await client.GetFromJsonAsync<List<int>>($"/api/Users/by-department?department={encodedDepartment}");

            // Nếu service trả về null thì trả về một danh sách rỗng
            return userIds ?? new List<int>();
        }
        catch (Exception ex)
        {
            // 6. Ghi lại log nếu có bất kỳ lỗi nào xảy ra trong quá trình gọi API
            _logger.LogError(ex, "Lỗi khi gọi UserService để lấy User IDs cho bộ phận {Department}", department);

            // Luôn trả về danh sách rỗng khi có lỗi để ứng dụng không bị dừng đột ngột
            return new List<int>();
        }
    }

    public async Task<PagedResult<AppNotificationReadDto>> GetAllForUserAsync(ClaimsPrincipal user, int pageNumber, int pageSize)
    {
        var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var userDepartment = user.FindFirstValue("Department");

        List<int> userIdsToFetch = new List<int>();

        // BƯỚC 1: XÁC ĐỊNH DANH SÁCH USER ID CẦN LẤY THÔNG BÁO DỰA TRÊN VAI TRÒ
        if (userRoles.Contains("Admin"))
        {
            // Admin: Lấy ID của tất cả người dùng từ UserService
            try
            {
                var client = _httpClientFactory.CreateClient("UserClient");
                var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(token);
                }
                // Giả sử có endpoint "/api/users/all-ids" để lấy tất cả ID
                var allUserIds = await client.GetFromJsonAsync<List<int>>("/api/Users/all-ids");
                if (allUserIds != null)
                {
                    userIdsToFetch = allUserIds;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách tất cả user ID cho Admin.");
                // Nếu lỗi, chỉ lấy thông báo của chính Admin
                userIdsToFetch.Add(userId);
            }
        }
        else if (userRoles.Contains("SuperUser"))
        {
            // SuperUser: Lấy ID của user trong cùng bộ phận
            try
            {
                var client = _httpClientFactory.CreateClient("UserClient");
                // Giả sử có endpoint "/api/users/ids-by-department"
                var departmentUserIds = await client.GetFromJsonAsync<List<int>>($"/api/Users/ids-by-department?department={userDepartment}");
                if (departmentUserIds != null)
                {
                    userIdsToFetch = departmentUserIds;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách user ID theo bộ phận cho SuperUser.");
            }

            // Luôn đảm bảo SuperUser thấy được thông báo của chính mình
            if (!userIdsToFetch.Contains(userId))
            {
                userIdsToFetch.Add(userId);
            }
        }
        else
        {
            // User thường: Chỉ thấy thông báo của chính mình
            userIdsToFetch.Add(userId);
        }

        // Nếu không có user nào để fetch, trả về kết quả rỗng
        if (!userIdsToFetch.Any())
        {
            return new PagedResult<AppNotificationReadDto>
            {
                Items = new List<AppNotificationReadDto>(),
                PaginationMetadata = new PaginationMetadata { TotalItemCount = 0, PageSize = pageSize, CurrentPage = pageNumber, TotalPageCount = 0 }
            };
        }

        // BƯỚC 2: GỌI REPOSITORY VỚI DANH SÁCH ID ĐÃ LỌC
        var (notifications, totalCount) = await _repository.GetAllPaginatedForUserIdsAsync(userIdsToFetch, pageNumber, pageSize);

        var paginationMetadata = new PaginationMetadata
        {
            TotalItemCount = totalCount,
            PageSize = pageSize,
            CurrentPage = pageNumber,
            TotalPageCount = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return new PagedResult<AppNotificationReadDto>
        {
            Items = _mapper.Map<IEnumerable<AppNotificationReadDto>>(notifications),
            PaginationMetadata = paginationMetadata
        };
    }
    public async Task<bool> MarkAsReadAsync(int notificationId, ClaimsPrincipal user)
    {
        // Thêm logic kiểm tra xem user có quyền đọc thông báo này không (tùy chọn)
        return await _repository.MarkAsReadAsync(notificationId);
    }
}