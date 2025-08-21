using AutoMapper;
using SubscriptionService.Dtos;
using SubscriptionService.Interface;
using SubscriptionService.Models;
using SubscriptionService.Enums;
using System.Security.Claims;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.DataValidation;

namespace SubscriptionService.Services;

public class SubscriptionManagementService : ISubscriptionManagementService
{
    private readonly ISubscriptionRepository _repository;
    private readonly IMapper _mapper;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SubscriptionManagementService> _logger;
    private readonly INotificationService _notificationService;

    public SubscriptionManagementService(
        ISubscriptionRepository repository,
        IMapper mapper,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<SubscriptionManagementService> logger,
        INotificationService notificationService)
    {
        _repository = repository;
        _mapper = mapper;
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<PaginatedResultDto<SubscriptionReadDto>> GetAllAsync(string? searchTerm, int? type, int pageNumber, int pageSize)
    {
        var (services, totalCount) = await _repository.GetAllAsync(searchTerm, type, pageNumber, pageSize);

        var dtos = services.Select(service => new SubscriptionReadDto
        {
            Id = service.Id,
            Name = service.Name,
            Type = service.Type.ToString(),
            Provider = service.Provider,
            ExpiryDate = service.ExpiryDate.ToString("dd/MM/yyyy"),
            DaysRemaining = (service.ExpiryDate.Date - DateTime.UtcNow.Date).Days,
            Note = service.Note
        });

        return new PaginatedResultDto<SubscriptionReadDto>
        {
            Items = dtos,
            TotalCount = totalCount
        };
    }

    public async Task<SubscriptionReadDto?> GetByIdAsync(Guid id)
    {
        var service = await _repository.GetByIdAsync(id);
        if (service == null) return null;
        return _mapper.Map<SubscriptionReadDto>(service);
    }

    public async Task<SubscriptionReadDto> CreateAsync(SubscriptionCreateDto createDto)
    {
        var newService = _mapper.Map<SubscribedService>(createDto);
        newService.CreatedAt = DateTime.UtcNow;
        newService.UpdatedAt = DateTime.UtcNow;

        // 1. Lấy số thứ tự lớn nhất hiện có
        var maxSortOrder = await _repository.GetMaxSortOrderAsync();
        // 2. Gán số thứ tự cho dịch vụ mới = số lớn nhất + 1
        newService.SortOrder = maxSortOrder + 1;

        var createdService = await _repository.CreateAsync(newService);

        var username = GetCurrentUser()?.Identity?.Name ?? "System";
        _ = LogActionAsync("CREATE_SUBSCRIPTION", createdService.Id.ToString(), new { CreatedData = createdService });
        _ = SendNotificationAsync($"Người dùng '{username}' đã tạo gói dịch vụ mới: '{createdService.Name}'.");
        // --- BẮT ĐẦU PHẦN TÍCH HỢP N8N ---
        var notificationDto = new SubscriptionNotificationDto
        {
            Id = createdService.Id,
            ServiceName = createdService.Name,
            Provider = createdService.Provider,
            ExpiryDate = createdService.ExpiryDate.ToString("dd/MM/yyyy"),
            DaysRemaining = (createdService.ExpiryDate.Date - DateTime.UtcNow.Date).Days,
            EventType = "CREATE",
            TriggeredBy = username
        };
        // Gọi service và quên đi (fire-and-forget) để không làm chậm response trả về cho người dùng
        _ = _notificationService.SendSubscriptionNotificationAsync(notificationDto);
        // --- KẾT THÚC PHẦN TÍCH HỢP N8N ---

        return _mapper.Map<SubscriptionReadDto>(createdService);
    }

    public async Task<bool> UpdateAsync(Guid id, SubscriptionUpdateDto updateDto)
    {
        var existingService = await _repository.GetByIdAsync(id);
        if (existingService == null) return false;

        var oldData = _mapper.Map<SubscriptionReadDto>(existingService);
        _mapper.Map(updateDto, existingService);
        existingService.UpdatedAt = DateTime.UtcNow;

        var result = await _repository.UpdateAsync(existingService);

        if (result)
        {
            var username = GetCurrentUser()?.Identity?.Name ?? "System";
            _ = LogActionAsync("UPDATE_SUBSCRIPTION", existingService.Id.ToString(), new { OldData = oldData, NewData = updateDto });
            _ = SendNotificationAsync($"Người dùng '{username}' đã cập nhật gói dịch vụ: '{existingService.Name}'.");

            // --- BẮT ĐẦU PHẦN TÍCH HỢP N8N ---
            var notificationDto = new SubscriptionNotificationDto
            {
                Id = existingService.Id,
                ServiceName = existingService.Name,
                Provider = existingService.Provider,
                ExpiryDate = existingService.ExpiryDate.ToString("dd/MM/yyyy"),
                DaysRemaining = (existingService.ExpiryDate.Date - DateTime.UtcNow.Date).Days,
                EventType = "UPDATE",
                TriggeredBy = username
            };
            _ = _notificationService.SendSubscriptionNotificationAsync(notificationDto);
            // --- KẾT THÚC PHẦN TÍCH HỢP N8N ---

        }
        return result;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var serviceToDelete = await _repository.GetByIdAsync(id);
        if (serviceToDelete == null) return false;

        var result = await _repository.DeleteAsync(id);

        if (result)
        {
            var username = GetCurrentUser()?.Identity?.Name ?? "System";
            _ = LogActionAsync("DELETE_SUBSCRIPTION", serviceToDelete.Id.ToString(), new { DeletedData = serviceToDelete });
            _ = SendNotificationAsync($"Người dùng '{username}' đã xóa gói dịch vụ: '{serviceToDelete.Name}'.");
        }
        return result;
    }

    public async Task<ImportResultDto> ImportFromExcelAsync(Stream stream)
    {
        //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var result = new ImportResultDto();
        var subscriptionsToAdd = new List<SubscribedService>();

        using (var package = new ExcelPackage(stream))
        {
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
            {
                result.Errors.Add("File Excel không có sheet nào.");
                return result;
            }

            var rowCount = worksheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var name = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                    if (string.IsNullOrEmpty(name)) continue;

                    var newSub = new SubscribedService
                    {
                        Name = name,
                        Type = Enum.Parse<ServiceType>(worksheet.Cells[row, 2].Value?.ToString() ?? "Other", true),
                        Provider = worksheet.Cells[row, 3].Value?.ToString()?.Trim(),
                        ExpiryDate = DateTime.Parse(worksheet.Cells[row, 4].Value?.ToString() ?? ""),
                        Note = worksheet.Cells[row, 5].Value?.ToString()?.Trim(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Id = Guid.NewGuid(),
                        SortOrder = row
                    };
                    subscriptionsToAdd.Add(newSub);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.FailCount++;
                    result.Errors.Add($"Lỗi ở dòng {row}: {ex.Message}");
                }
            }
        }

        if (subscriptionsToAdd.Any())
        {
            await _repository.CreateBatchAsync(subscriptionsToAdd);
        }
        return result;
    }

    public async Task<byte[]> GenerateExcelTemplateAsync()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("ImportTemplate");

            var headers = new string[] {
                "Tên Dịch Vụ (Bắt buộc)", "Loại", "Nhà Cung Cấp",
                "Ngày Hết Hạn (Bắt buộc)", "Ghi Chú"
            };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                // --- THIS LINE IS REMOVED TO ENSURE LINUX COMPATIBILITY ---
                // worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                // worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            }

            var serviceTypeNames = Enum.GetNames(typeof(ServiceType));
            var validation = worksheet.DataValidations.AddListValidation("B2:B1000");
            foreach (var name in serviceTypeNames)
            {
                validation.Formula.Values.Add(name);
            }
            validation.ShowErrorMessage = true;
            validation.ErrorTitle = "Giá trị không hợp lệ";
            validation.Error = "Vui lòng chọn một giá trị từ danh sách.";

            worksheet.Cells["A2"].Value = "Tên miền Google";
            worksheet.Cells["B2"].Value = "Domain";
            worksheet.Cells["C2"].Value = "Google";
            worksheet.Cells["D2"].Value = "2025-12-31";
            worksheet.Cells["E2"].Value = "Ghi chú ví dụ";

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            return await package.GetAsByteArrayAsync();
        }
    }

    public async Task<byte[]> GenerateExcelExportAsync(string? searchTerm, int? type)
    {
        //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var (services, _) = await _repository.GetAllAsync(searchTerm, type, 1, int.MaxValue);
        var subscriptions = services.Select(service => new SubscriptionReadDto
        {
            Id = service.Id,
            Name = service.Name,
            Type = service.Type.ToString(),
            Provider = service.Provider,
            ExpiryDate = service.ExpiryDate.ToString("dd/MM/yyyy"),
            DaysRemaining = (service.ExpiryDate.Date - DateTime.UtcNow.Date).Days,
            Note = service.Note
        });
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Subscriptions");

            var headers = new string[] {
                "Tên Dịch Vụ", "Loại", "Nhà Cung Cấp", "Ngày Hết Hạn", "Còn Lại (ngày)", "Ghi Chú"
            };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            var row = 2;
            foreach (var sub in subscriptions)
            {
                worksheet.Cells[row, 1].Value = sub.Name;
                worksheet.Cells[row, 2].Value = sub.Type;
                worksheet.Cells[row, 3].Value = sub.Provider;
                worksheet.Cells[row, 4].Value = sub.ExpiryDate;
                worksheet.Cells[row, 5].Value = sub.DaysRemaining;
                worksheet.Cells[row, 6].Value = sub.Note;
                row++;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            return await package.GetAsByteArrayAsync();
        }
    }

    private ClaimsPrincipal? GetCurrentUser() => _httpContextAccessor.HttpContext?.User;

    private async Task LogActionAsync(string actionType, string entityId, object details)
    {
        var requester = GetCurrentUser();
        if (requester == null) return;

        var userIdStr = requester.FindFirstValue(ClaimTypes.NameIdentifier);
        var username = requester.FindFirstValue(ClaimTypes.Name);
        int.TryParse(userIdStr, out int userId);

        var serializerOptions = new JsonSerializerOptions { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };

        var logData = new
        {
            UserId = userId,
            Username = username,
            ActionType = actionType,
            EntityType = "Subscription",
            EntityId = entityId,
            Details = JsonSerializer.Serialize(details, serializerOptions),
            IsSuccess = true,
        };

        try
        {
            var client = _httpClientFactory.CreateClient("AuditLogClient");
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(token);
            }
            var content = new StringContent(JsonSerializer.Serialize(logData, serializerOptions), System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/AuditLogs", content);
            _logger.LogInformation("Received status code {StatusCode} from AuditLogsService for action {ActionType}", response.StatusCode, actionType);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("Successfully sent audit log for action: {ActionType}", actionType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FAILED_TO_SEND_AUDIT_LOG. Data: {@LogData}", logData);
        }
    }

    private async Task SendNotificationAsync(string message, int? targetUserId = null)
    {
        var requester = GetCurrentUser();
        if (requester == null) return;

        var notificationData = new
        {
            Message = message,
            TargetUserId = targetUserId,
            CreatedBy = requester.FindFirstValue(ClaimTypes.Name)
        };

        try
        {
            var client = _httpClientFactory.CreateClient("NotificationClient");
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(token);
            }
            var content = new StringContent(JsonSerializer.Serialize(notificationData), System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/AppNotifications", content);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FAILED_TO_SEND_NOTIFICATION. Data: {@NotificationData}", notificationData);
        }
    }

    public async Task<IEnumerable<SubscriptionStatsDto>> GetStatsByTypeAsync()
    {
        var stats = await _repository.GetStatsByTypeAsync();

        // Chuyển đổi key enum (số) thành tên hiển thị (string) cho đẹp
        return stats.Select(s =>
        {
            if (Enum.TryParse<ServiceType>(s.Type, out var serviceTypeEnum))
            {
                s.Type = EnumHelper.GetDisplayName(serviceTypeEnum);
            }
            return s;
        });
    }
}