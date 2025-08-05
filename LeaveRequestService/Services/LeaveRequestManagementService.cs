using AutoMapper;
using LeaveRequestService.DTOs;
using LeaveRequestService.Enums;
using LeaveRequestService.Interface;
using LeaveRequestService.Models;
using LeaveRequestService.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;
using System.Drawing;
using LeaveRequestService.DTO;
using System.Reflection;

namespace LeaveRequestService.Services
{
    public class LeaveRequestManagementService : ILeaveRequestManagementService
    {
        private readonly ILeaveRequestRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<LeaveRequestManagementService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUserServiceClient _userService;

        public LeaveRequestManagementService(
            ILeaveRequestRepository repository,
            IMapper mapper,
            IHttpClientFactory httpClientFactory,
            ILogger<LeaveRequestManagementService> logger,
            IConfiguration configuration,
            IUserServiceClient userService,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
            _configuration = configuration;
        }

        private async Task<UserDetailDto?> GetUserDetailAsync(int userId)
        {
            var client = _httpClientFactory.CreateClient("UserClient");
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(token);
            }
            try
            {
                return await client.GetFromJsonAsync<UserDetailDto>($"/api/Users/{userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user details for UserId {UserId}", userId);
                return null;
            }
        }

        public async Task<LeaveRequestReadDto> CreateAsync(LeaveRequestCreateDto dto, ClaimsPrincipal requester)
        {
            var requesterId = int.Parse(requester.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var existingRequests = await _repository.GetByUserIdAsync(requesterId);
            bool hasOverlappingRequest = existingRequests.Any(r =>
                r.TrangThai == LeaveRequestStatus.Pending &&
                dto.NgayTu < r.NgayDen && dto.NgayDen > r.NgayTu
            );

            if (hasOverlappingRequest)
            {
                throw new InvalidOperationException("Bạn đã có một đơn xin phép khác đang chờ duyệt trong khoảng thời gian này.");
            }

            var leaveRequest = _mapper.Map<LeaveRequest>(dto);

            leaveRequest.UserId = requesterId;
            leaveRequest.CreatedByUserId = requesterId;
            leaveRequest.NgayTao = DateTime.UtcNow;
            leaveRequest.TrangThai = LeaveRequestStatus.Pending;
            leaveRequest.CongViecBanGiao = dto.CongViecBanGiao;

            var createdRequest = await _repository.CreateAsync(leaveRequest);

            _ = LogActionAsync(requester, "CREATE_LEAVE_REQUEST", "LeaveRequest", createdRequest.Id, new { NewData = createdRequest });

            // BẮT ĐẦU LOGIC TẠO THÔNG BÁO CHO QUẢN LÝ
            try
            {
                var requesterDetail = await GetUserDetailAsync(requesterId);
                if (requesterDetail == null) throw new Exception("Không tìm thấy thông tin người tạo đơn.");

                var client = _httpClientFactory.CreateClient("UserClient");
                var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(token);
                }

                var adminsResult = await client.GetFromJsonAsync<List<UserDetailDto>>("/api/Users/by-role?role=Admin");

                UserDetailDto superUserResult = null;
                try
                {
                    superUserResult = await client.GetFromJsonAsync<UserDetailDto>($"/api/users/superuser-by-department?department={requesterDetail.BoPhan}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Không tìm thấy SuperUser cho phòng ban {Department}", requesterDetail.BoPhan);
                }

                var recipients = new Dictionary<int, UserDetailDto>();

                if (adminsResult != null)
                {
                    foreach (var admin in adminsResult) { recipients[admin.Id] = admin; }
                }
                if (superUserResult != null)
                {
                    recipients[superUserResult.Id] = superUserResult;
                }

                if (recipients.Any())
                {
                    var notificationClient = _httpClientFactory.CreateClient("NotificationClient");
                    if (!string.IsNullOrEmpty(token))
                    {
                        notificationClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(token);
                    }
                    var message = $"{requesterDetail.HoTen} vừa tạo một đơn xin phép mới.";

                    foreach (var recipient in recipients.Values)
                    {
                        if (recipient.Id == requesterId && recipient.Role != "Admin")
                        {
                            continue;
                        }

                        var notificationDto = new
                        {
                            UserId = recipient.Id,
                            Message = message,
                            Url = $"/leave-requests?requestId={createdRequest.Id}",
                            TriggeredByUserId = requesterId,
                            TriggeredByUsername = requesterDetail.HoTen
                        };

                        await notificationClient.PostAsJsonAsync("/api/AppNotifications", notificationDto);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra khi gửi thông báo cho quản lý.");
            }

            var resultDto = _mapper.Map<LeaveRequestReadDto>(createdRequest);
            resultDto.HoTen = requester.FindFirstValue("FullName");
            resultDto.MaSoNhanVien = requester.FindFirstValue(ClaimTypes.Name);
            resultDto.BoPhan = requester.FindFirstValue("Department");
            resultDto.ChucVu = requester.FindFirstValue("Position");
            resultDto.GioTu = dto.GioTu;
            resultDto.GioDen = dto.GioDen;

            return resultDto;
        }

        public async Task<IEnumerable<LeaveRequestReadDto>> GetAllAsync(ClaimsPrincipal requester, string? searchTerm, string? status)
        {
            var requesterId = int.Parse(requester.FindFirstValue(ClaimTypes.NameIdentifier)!);
            IEnumerable<LeaveRequest> leaveRequests;

            // ================== LOGIC PHÂN QUYỀN MỚI TẠI ĐÂY ==================
            if (requester.IsInRole("Admin"))
            {
                // 1. Admin lấy tất cả đơn như cũ
                leaveRequests = await _repository.GetAllAsync(null, status);
            }
            else if (requester.IsInRole("SuperUser"))
            {
                var superUserDepartment = requester.FindFirstValue("Department");
                List<int> userIdsInDepartment = new List<int>();

                try
                {
                    // 2. SuperUser sẽ gọi sang UserService để lấy ID của các user trong bộ phận
                    var userClient = _httpClientFactory.CreateClient("UserClient");
                    var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
                    if (!string.IsNullOrEmpty(token))
                    {
                        userClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(token);
                    }
                    var encodedDepartment = Uri.EscapeDataString(superUserDepartment ?? "");
                    userIdsInDepartment = await userClient.GetFromJsonAsync<List<int>>($"/api/Users/by-department?department={encodedDepartment}") ?? new List<int>();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi SuperUser lấy danh sách user từ bộ phận {Department}", superUserDepartment);
                }

                // Luôn bao gồm cả chính SuperUser trong danh sách
                if (!userIdsInDepartment.Contains(requesterId))
                {
                    userIdsInDepartment.Add(requesterId);
                }

                // 3. Chỉ lấy đơn của những User ID đã được lọc
                leaveRequests = await _repository.GetByUserIdListAsync(userIdsInDepartment, status);
            }
            else
            {
                // 4. User thường chỉ lấy đơn của chính mình
                leaveRequests = await _repository.GetByUserIdAsync(requesterId);
            }
            // ====================================================================

            if (!leaveRequests.Any())
            {
                return new List<LeaveRequestReadDto>();
            }

            var userIds = leaveRequests.Select(r => r.UserId).Distinct().ToList();
            var userDetailsMap = new Dictionary<int, UserDetailDto>();

            if (userIds.Any())
            {
                try
                {
                    // Logic gọi get-by-ids để lấy thông tin user vẫn giữ nguyên
                    var client = _httpClientFactory.CreateClient("UserClient");
                    var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
                    if (!string.IsNullOrEmpty(token))
                    {
                        client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(token);
                    }
                    var response = await client.PostAsJsonAsync("/api/Users/get-by-ids", userIds);
                    if (response.IsSuccessStatusCode)
                    {
                        var userList = await response.Content.ReadFromJsonAsync<List<UserDetailDto>>();
                        if (userList != null)
                        {
                            userDetailsMap = userList.ToDictionary(u => u.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi gọi UserService để lấy chi tiết user.");
                }
            }

            var resultDtos = new List<LeaveRequestReadDto>();
            foreach (var req in leaveRequests)
            {
                var dto = _mapper.Map<LeaveRequestReadDto>(req);
                if (userDetailsMap.TryGetValue(req.UserId, out var userDetail))
                {
                    dto.HoTen = userDetail.HoTen;
                    dto.MaSoNhanVien = userDetail.MaSoNhanVien;
                    dto.BoPhan = userDetail.BoPhan;
                    dto.ChucVu = userDetail.ChucVu;
                    dto.Email = userDetail.Email;
                    dto.Role = userDetail.Role;
                }
                resultDtos.Add(dto);
            }

            // Logic tìm kiếm vẫn giữ nguyên
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLowerInvariant();
                return resultDtos.Where(dto =>
                    (dto.HoTen != null && dto.HoTen.ToLowerInvariant().Contains(lowerSearchTerm)) ||
                    (dto.MaSoNhanVien != null && dto.MaSoNhanVien.ToLowerInvariant().Contains(lowerSearchTerm)) ||
                    (dto.LyDo != null && dto.LyDo.ToLowerInvariant().Contains(lowerSearchTerm))
                ).ToList();
            }

            return resultDtos;
        }

        public async Task<LeaveRequestReadDto?> GetByIdAsync(int id, ClaimsPrincipal requester)
        {
            var leaveRequest = await _repository.GetByIdAsync(id);
            if (leaveRequest == null) return null;

            var requesterId = int.Parse(requester.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isAdminOrSuperUser = requester.IsInRole("Admin") || requester.IsInRole("SuperUser");

            if (!isAdminOrSuperUser && leaveRequest.UserId != requesterId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xem đơn này.");
            }

            var resultDto = _mapper.Map<LeaveRequestReadDto>(leaveRequest);
            var userDetail = await GetUserDetailAsync(leaveRequest.UserId);
            if (userDetail != null)
            {
                resultDto.MaSoNhanVien = userDetail.MaSoNhanVien;
                resultDto.HoTen = userDetail.HoTen;
                resultDto.BoPhan = userDetail.BoPhan;
                resultDto.ChucVu = userDetail.ChucVu;
                resultDto.Role = userDetail.Role;
                resultDto.Email = userDetail.Email;
            }
            return resultDto;
        }

        public async Task<bool> UpdateStatusAsync(int id, LeaveRequestUpdateStatusDto dto, ClaimsPrincipal requester)
        {
            var leaveRequest = await _repository.GetByIdAsync(id);
            if (leaveRequest == null) return false;

            if (leaveRequest.TrangThai != LeaveRequestStatus.Pending)
            {
                throw new InvalidOperationException($"Không thể thay đổi trạng thái của đơn đã được xử lý.");
            }

            var requesterId = int.Parse(requester.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var requesterRole = requester.FindFirstValue(ClaimTypes.Role);

            bool isAuthorized = false;

            if (requesterRole == "Admin")
            {
                isAuthorized = true;
            }
            else if (requesterRole == "SuperUser")
            {
                var client = _httpClientFactory.CreateClient("UserClient");
                var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(token);
                }

                var requesterInfoTask = client.GetFromJsonAsync<UserDetailDto>($"/api/Users/{requesterId}");
                var creatorInfoTask = client.GetFromJsonAsync<UserDetailDto>($"/api/Users/{leaveRequest.UserId}");
                await Task.WhenAll(requesterInfoTask, creatorInfoTask);

                if (requesterInfoTask.Result?.BoPhan == creatorInfoTask.Result?.BoPhan)
                {
                    isAuthorized = true;
                }
            }

            if (!isAuthorized)
            {
                throw new InvalidOperationException("Bạn không có quyền duyệt đơn xin phép này.");
            }

            var oldStatus = leaveRequest.TrangThai;
            leaveRequest.TrangThai = dto.TrangThaiMoi;
            leaveRequest.LyDoXuLy = dto.LyDoXuLy;
            leaveRequest.LastUpdatedAt = DateTime.UtcNow;
            leaveRequest.LastUpdatedByUserId = requesterId;

            await _repository.UpdateAsync(leaveRequest);

            try
            {
                var notificationClient = _httpClientFactory.CreateClient("NotificationClient");
                var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(token))
                {
                    notificationClient.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse(token);
                }

                var statusText = dto.TrangThaiMoi == LeaveRequestStatus.Approved ? "đã được duyệt" : "đã bị từ chối";
                var message = $"Đơn xin phép của bạn tạo ngày {leaveRequest.NgayTao:dd/MM/yyyy} {statusText}.";

                var approverName = requester.FindFirstValue("FullName") ?? requester.FindFirstValue(ClaimTypes.Name) ?? "Quản trị viên";

                var notificationDto = new
                {
                    UserId = leaveRequest.UserId,
                    Message = message,
                    Url = $"/leave-requests?requestId={leaveRequest.Id}",
                    TriggeredByUserId = requesterId,
                    TriggeredByUsername = approverName
                };

                _logger.LogWarning("CHUẨN BỊ GỬI PUSH NOTIFICATION CHO USER ID: {TargetUserId}", leaveRequest.UserId);

                // ================== SỬA LỖI Ở ĐÂY ==================
                var response = await notificationClient.PostAsJsonAsync("/api/AppNotifications", notificationDto);
                // ===================================================

                _logger.LogWarning("Đã gửi request đến NotificationService, Status Code trả về: {StatusCode}", response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra khi gửi thông báo duyệt đơn cho người dùng UserId {UserId}", leaveRequest.UserId);
            }

            _ = LogActionAsync(requester, "UPDATE_LEAVE_STATUS", "LeaveRequest", id,
                new { OldStatus = oldStatus.ToString(), NewStatus = dto.TrangThaiMoi.ToString(), Reason = dto.LyDoXuLy });

            return true;
        }

        public async Task<bool> CancelAsync(int id, ClaimsPrincipal requester)
        {
            var leaveRequest = await _repository.GetByIdAsync(id);
            if (leaveRequest == null) return false;

            var requesterId = int.Parse(requester.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (leaveRequest.UserId != requesterId)
            {
                throw new InvalidOperationException("Bạn không phải là người tạo đơn này để có thể hủy.");
            }
            if (leaveRequest.TrangThai != LeaveRequestStatus.Pending)
            {
                throw new InvalidOperationException("Chỉ có thể hủy đơn khi đang ở trạng thái 'Chờ duyệt'.");
            }

            leaveRequest.TrangThai = LeaveRequestStatus.Cancelled;
            leaveRequest.DaGuiN8n = false;
            leaveRequest.LastUpdatedAt = DateTime.UtcNow;
            leaveRequest.LastUpdatedByUserId = requesterId;

            await _repository.UpdateAsync(leaveRequest);

            _ = LogActionAsync(requester, "CANCEL_LEAVE_REQUEST", "LeaveRequest", id, new { RequestId = id });

            return true;
        }

        private async Task LogActionAsync(ClaimsPrincipal requester, string actionType, string? entityType, int? entityId, object details)
        {
            var userIdStr = requester.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = requester.FindFirstValue(ClaimTypes.Name);
            int.TryParse(userIdStr, out int userId);
            var serializerOptions = new JsonSerializerOptions { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };
            var logData = new { UserId = userId, Username = username, ActionType = actionType, EntityType = entityType, EntityId = entityId, Details = JsonSerializer.Serialize(details, serializerOptions), IsSuccess = true, ErrorMessage = (string)null, RequesterIpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() };
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
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FAILED_TO_SEND_AUDIT_LOG from LeaveRequestService. Data: {@LogData}", logData);
            }
        }

        public async Task<bool> SendForApprovalAsync(int id, ClaimsPrincipal requester)
        {
            var leaveRequest = await _repository.GetByIdAsync(id);
            if (leaveRequest == null || leaveRequest.TrangThai != LeaveRequestStatus.Pending)
            {
                throw new InvalidOperationException("Chỉ có thể gửi duyệt các đơn đang ở trạng thái 'Chờ duyệt'.");
            }

            leaveRequest.ApprovalToken = Guid.NewGuid().ToString("N");
            leaveRequest.ApprovalTokenExpires = DateTime.UtcNow.AddDays(7);
            leaveRequest.DaGuiN8n = true;

            await _repository.UpdateAsync(leaveRequest);

            var client = _httpClientFactory.CreateClient("n8nClient");
            var n8nWebhookUrl = _configuration["N8nSettings:WebhookUrl"];

            if (string.IsNullOrEmpty(n8nWebhookUrl))
            {
                _logger.LogError("N8N_WEBHOOK_URL is not configured.");
                return false;
            }

            var userDetail = await GetUserDetailAsync(leaveRequest.UserId);
            var frontendApprovalPageUrl = $"{_configuration["ClientUrl"]}/approve-leave";

            var payload = new
            {
                employeeName = userDetail?.HoTen ?? "Không rõ",
                leaveType = leaveRequest.LoaiPhep.ToString(),
                fromDate = leaveRequest.NgayTu.ToString("dd/MM/yyyy"),
                toDate = leaveRequest.NgayDen.ToString("dd/MM/yyyy"),
                fromTime = leaveRequest.GioTu ?? "N/A",
                toTime = leaveRequest.GioDen ?? "N/A",
                reason = leaveRequest.LyDo,
                approveLink = $"{frontendApprovalPageUrl}?token={leaveRequest.ApprovalToken}&action=approve",
                rejectLink = $"{frontendApprovalPageUrl}?token={leaveRequest.ApprovalToken}&action=reject"
            };

            var response = await client.PostAsJsonAsync(n8nWebhookUrl, payload);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to call n8n webhook for request ID {RequestId}", id);
            }

            _ = LogActionAsync(requester, "SEND_APPROVAL_REQUEST", "LeaveRequest", id, new { RequestId = id });

            return true;
        }

        public async Task<bool> ProcessApprovalAsync(ProcessApprovalDto dto)
        {
            var leaveRequest = await _repository.FindByApprovalTokenAsync(dto.Token);
            if (leaveRequest == null)
            {
                return false;
            }

            if (leaveRequest.ApprovalTokenExpires < DateTime.UtcNow)
            {
                leaveRequest.ApprovalToken = null;
                await _repository.UpdateAsync(leaveRequest);
                return false;
            }

            if (leaveRequest.TrangThai != LeaveRequestStatus.Pending)
            {
                throw new InvalidOperationException("Đơn này đã được xử lý trước đó.");
            }

            if (dto.Action == "approve")
            {
                leaveRequest.TrangThai = LeaveRequestStatus.Approved;
            }
            else if (dto.Action == "reject")
            {
                leaveRequest.TrangThai = LeaveRequestStatus.Rejected;
            }
            else
            {
                return false;
            }

            leaveRequest.LyDoXuLy = dto.LyDoXuLy;

            leaveRequest.ApprovalToken = null;
            leaveRequest.ApprovalTokenExpires = null;
            leaveRequest.LastUpdatedAt = DateTime.UtcNow;
            // Vì không có người duyệt cụ thể, có thể gán ID hệ thống (vd: -1)
            leaveRequest.LastUpdatedByUserId = -1;

            await _repository.UpdateAsync(leaveRequest);

            // ================== LOGIC GỬI THÔNG BÁO ĐƯỢC THÊM VÀO ĐÂY ==================
            try
            {
                var notificationClient = _httpClientFactory.CreateClient("NotificationClient");

                // Luồng này không có token người dùng, nên sẽ gọi nặc danh
                // Chúng ta sẽ cần cho phép điều này ở NotificationService (xem bước 2)

                var statusText = leaveRequest.TrangThai == LeaveRequestStatus.Approved ? "đã được duyệt" : "đã bị từ chối";
                var message = $"Đơn xin phép của bạn tạo ngày {leaveRequest.NgayTao:dd/MM/yyyy} {statusText}.";

                var notificationDto = new
                {
                    UserId = leaveRequest.UserId,
                    Message = message,
                    Url = $"/leave-requests?requestId={leaveRequest.Id}",
                    TriggeredByUserId = -1, // ID Hệ thống
                    TriggeredByUsername = "Hệ thống (Email)" // Người gửi là Hệ thống
                };

                _logger.LogWarning("[ProcessApproval] CHUẨN BỊ GỬI PUSH NOTIFICATION CHO USER ID: {TargetUserId}", leaveRequest.UserId);

                var response = await notificationClient.PostAsJsonAsync("/api/AppNotifications", notificationDto);

                _logger.LogWarning("[ProcessApproval] Đã gửi request đến NotificationService, Status Code trả về: {StatusCode}", response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProcessApproval] Lỗi khi gửi thông báo duyệt đơn cho UserId {UserId}", leaveRequest.UserId);
            }
            // =======================================================================

            return true;
        }

        public async Task<LeaveRequestReadDto> ResubmitAsync(int id, LeaveRequestCreateDto dto, ClaimsPrincipal requester)
        {
            var oldRequest = await _repository.GetByIdAsync(id);
            if (oldRequest == null || oldRequest.TrangThai != LeaveRequestStatus.Pending)
            {
                throw new InvalidOperationException("Chỉ có thể sửa đơn đang ở trạng thái 'Chờ duyệt'.");
            }
            await CancelAsync(id, requester);
            return await CreateAsync(dto, requester);
        }

        public async Task<bool> RequestRevocationAsync(int id, ClaimsPrincipal requester)
        {
            var leaveRequest = await _repository.GetByIdAsync(id);
            if (leaveRequest == null)
            {
                throw new KeyNotFoundException("Không tìm thấy đơn xin phép.");
            }
            var requesterId = int.Parse(requester.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isRequesterAdmin = requester.IsInRole("Admin");
            if (!isRequesterAdmin && leaveRequest.UserId != requesterId)
            {
                throw new InvalidOperationException("Bạn không có quyền yêu cầu thu hồi đơn này.");
            }

            if (leaveRequest.TrangThai != LeaveRequestStatus.Approved)
            {
                throw new InvalidOperationException("Chỉ có thể yêu cầu thu hồi đơn đã được duyệt.");
            }
            leaveRequest.TrangThai = LeaveRequestStatus.PendingRevocation;
            leaveRequest.RevocationToken = Guid.NewGuid().ToString("N");
            leaveRequest.RevocationTokenExpires = DateTime.UtcNow.AddDays(3);
            await _repository.UpdateAsync(leaveRequest);

            try
            {
                var client = _httpClientFactory.CreateClient("n8nClient");
                var webhookUrl = _configuration["N8nSettings:RevocationWebhookUrl"];

                if (!string.IsNullOrEmpty(webhookUrl))
                {
                    var userDetail = await GetUserDetailAsync(requesterId);
                    var payload = new
                    {
                        adminName = userDetail?.HoTen ?? "Quản trị viên",
                        leaveRequestId = leaveRequest.Id,
                        processRevocationLink = $"{_configuration["ClientUrl"]}/approve-leave?token={leaveRequest.RevocationToken}&action=process_revocation"
                    };
                    await client.PostAsJsonAsync(webhookUrl, payload);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi webhook n8n cho việc thu hồi đơn {Id}", id);
            }

            _ = LogActionAsync(requester, "REQUEST_REVOCATION", "LeaveRequest", id, null);

            return true;
        }

        public async Task<bool> ProcessRevocationAsync(ProcessRevocationDto dto)
        {
            var leaveRequest = await _repository.FindByRevocationTokenAsync(dto.Token);

            if (leaveRequest == null)
            {
                throw new InvalidOperationException("Yêu cầu thu hồi không hợp lệ hoặc đã được xử lý.");
            }

            if (leaveRequest.RevocationTokenExpires < DateTime.UtcNow)
            {
                leaveRequest.TrangThai = LeaveRequestStatus.Approved;
                leaveRequest.RevocationToken = null;
                leaveRequest.RevocationTokenExpires = null;
                await _repository.UpdateAsync(leaveRequest);
                throw new InvalidOperationException("Yêu cầu thu hồi đã hết hạn. Đơn đã được trả về trạng thái 'Đã duyệt'.");
            }

            leaveRequest.TrangThai = LeaveRequestStatus.Pending;
            leaveRequest.DaGuiN8n = false;
            leaveRequest.RevocationToken = null;
            leaveRequest.RevocationTokenExpires = null;

            await _repository.UpdateAsync(leaveRequest);

            _ = LogAnonymousActionAsync("PROCESS_REVOCATION", "LeaveRequest", leaveRequest.Id,
                new { ProcessedToken = dto.Token, OriginalUserId = leaveRequest.UserId });
            return true;
        }

        private async Task LogAnonymousActionAsync(string actionType, string? entityType, int? entityId, object details)
        {
            var logData = new
            {
                UserId = (int?)null,
                Username = "System (via Email Link)",
                ActionType = actionType,
                EntityType = entityType,
                EntityId = entityId,
                Details = JsonSerializer.Serialize(details, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) }),
                IsSuccess = true,
                ErrorMessage = (string)null,
                RequesterIpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
            };

            try
            {
                var client = _httpClientFactory.CreateClient("AuditLogClient");
                var content = new StringContent(JsonSerializer.Serialize(logData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("/api/AuditLogs", content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FAILED_TO_SEND_ANONYMOUS_AUDIT_LOG. Data: {@LogData}", logData);
            }
        }

        public async Task<byte[]> ExportLeaveRequestsToExcelAsync(ClaimsPrincipal requester, string? searchTerm = null, string? role = null)
        {
            var leaveRequests = await GetAllAsync(requester, searchTerm, role);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("LeaveRequests");

                int col = 1;
                worksheet.Cells[1, col++].Value = "Mã NV";
                worksheet.Cells[1, col++].Value = "Họ tên";
                worksheet.Cells[1, col++].Value = "Email";
                worksheet.Cells[1, col++].Value = "Bộ phận";
                worksheet.Cells[1, col++].Value = "Chức vụ";
                worksheet.Cells[1, col++].Value = "Vai trò";
                worksheet.Cells[1, col++].Value = "Loại phép";
                worksheet.Cells[1, col++].Value = "Từ ngày";
                worksheet.Cells[1, col++].Value = "Đến ngày";
                worksheet.Cells[1, col++].Value = "Giờ từ";
                worksheet.Cells[1, col++].Value = "Giờ đến";
                worksheet.Cells[1, col++].Value = "Lý do";
                worksheet.Cells[1, col++].Value = "Trạng thái";
                worksheet.Cells[1, col++].Value = "Lý do xử lý";
                worksheet.Cells[1, col++].Value = "Ngày tạo";

                using (var range = worksheet.Cells[1, 1, 1, col - 1])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                }

                for (int i = 0; i < leaveRequests.Count(); i++)
                {
                    var item = leaveRequests.ElementAt(i);
                    int row = i + 2;
                    col = 1;

                    worksheet.Cells[row, col++].Value = item.MaSoNhanVien;
                    worksheet.Cells[row, col++].Value = item.HoTen;
                    worksheet.Cells[row, col++].Value = item.Email;
                    worksheet.Cells[row, col++].Value = item.BoPhan;
                    worksheet.Cells[row, col++].Value = item.ChucVu;
                    worksheet.Cells[row, col++].Value = item.Role;
                    worksheet.Cells[row, col++].Value = item.LoaiPhep.ToString();
                    worksheet.Cells[row, col++].Value = item.NgayTu.ToString("dd/MM/yyyy");
                    worksheet.Cells[row, col++].Value = item.NgayDen.ToString("dd/MM/yyyy");
                    worksheet.Cells[row, col++].Value = item.GioTu;
                    worksheet.Cells[row, col++].Value = item.GioDen;
                    worksheet.Cells[row, col++].Value = item.LyDo;
                    worksheet.Cells[row, col++].Value = item.TrangThai.ToString();
                    worksheet.Cells[row, col++].Value = item.LyDoXuLy;
                    worksheet.Cells[row, col++].Value = item.NgayTao.ToLocalTime();
                    worksheet.Cells[row, col - 1].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                using (var stream = new MemoryStream())
                {
                    package.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public async Task<LeaveRequestDashboardStatsDto> GetDashboardStatsAsync(ClaimsPrincipal requester)
        {
            const double TotalLeaveEntitlementConst = 12.0;
            var requesterId = int.Parse(requester.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isAdmin = requester.IsInRole("Admin");
            var isSuperUser = requester.IsInRole("SuperUser");

            IEnumerable<LeaveRequest> overviewRequests;
            if (isAdmin || isSuperUser)
            {
                overviewRequests = await _repository.GetAllAsync(null, null);
            }
            else
            {
                overviewRequests = await _repository.GetByUserIdAsync(requesterId);
            }

            if (!overviewRequests.Any())
            {
                return new LeaveRequestDashboardStatsDto
                {
                    CurrentUserTotalEntitlement = TotalLeaveEntitlementConst,
                    CurrentUserDaysTaken = 0,
                    CurrentUserDaysRemaining = TotalLeaveEntitlementConst,
                    PendingList = new List<LeaveRequestSummaryDto>(),
                    ApprovedList = new List<LeaveRequestSummaryDto>(),
                    RejectedOrCancelledList = new List<LeaveRequestSummaryDto>(),
                    DailyCounts = new List<LeaveRequestByDateDto>()
                };
            }

            var overviewUserIds = overviewRequests.Select(r => r.UserId).Distinct().ToList();
            var userDetailsMap = new Dictionary<int, UserDetailDto>();
            if (overviewUserIds.Any())
            {
                try
                {
                    var client = _httpClientFactory.CreateClient("UserClient");
                    var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
                    if (!string.IsNullOrEmpty(token))
                    {
                        client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(token);
                    }
                    var userDetailsResponse = await client.PostAsJsonAsync("/api/Users/get-by-ids", overviewUserIds);
                    if (userDetailsResponse.IsSuccessStatusCode)
                    {
                        var userDetailsList = await userDetailsResponse.Content.ReadFromJsonAsync<List<UserDetailDto>>();
                        if (userDetailsList != null)
                        {
                            userDetailsMap = userDetailsList.ToDictionary(u => u.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi gọi UserService cho dashboard overview.");
                }
            }

            if (isSuperUser && !isAdmin)
            {
                var superUserDepartment = requester.FindFirstValue("Department");
                if (!string.IsNullOrEmpty(superUserDepartment))
                {
                    overviewRequests = overviewRequests.Where(req =>
                        userDetailsMap.TryGetValue(req.UserId, out var user) && user.BoPhan == superUserDepartment
                    ).ToList();
                }
            }

            var currentUserApprovedRequests = overviewRequests
                .Where(r => r.UserId == requesterId && r.TrangThai == LeaveRequestStatus.Approved);

            double daysTaken = currentUserApprovedRequests.Sum(r => r.DurationInDays);

            var stats = new LeaveRequestDashboardStatsDto
            {
                TotalRequests = overviewRequests.Count(),
                PendingRequests = overviewRequests.Count(r => r.TrangThai == LeaveRequestStatus.Pending),
                ApprovedRequests = overviewRequests.Count(r => r.TrangThai == LeaveRequestStatus.Approved),
                RejectedRequests = overviewRequests.Count(r => r.TrangThai == LeaveRequestStatus.Rejected),
                CancelledRequests = overviewRequests.Count(r => r.TrangThai == LeaveRequestStatus.Cancelled),
                WaitingRevocation = overviewRequests.Count(r => r.TrangThai == LeaveRequestStatus.PendingRevocation),

                CurrentUserTotalEntitlement = TotalLeaveEntitlementConst,
                CurrentUserDaysTaken = daysTaken,
                CurrentUserDaysRemaining = TotalLeaveEntitlementConst - daysTaken,

                DailyCounts = overviewRequests
                    .Where(r => r.NgayTao >= DateTime.UtcNow.AddDays(-30))
                    .GroupBy(r => r.NgayTao.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new LeaveRequestByDateDto { Date = g.Key, Count = g.Count() })
                    .ToList(),

                PendingList = overviewRequests.Where(r => r.TrangThai == LeaveRequestStatus.Pending).OrderByDescending(r => r.NgayTao).Take(5)
                    .Select(r => new LeaveRequestSummaryDto { Id = r.Id, UserFullName = userDetailsMap.TryGetValue(r.UserId, out var u) ? u.HoTen : "...", LoaiPhep = Enums.EnumHelper.GetDisplayName(r.LoaiPhep), NgayTu = r.NgayTu }).ToList(),

                ApprovedList = overviewRequests.Where(r => r.TrangThai == LeaveRequestStatus.Approved).OrderByDescending(r => r.LastUpdatedAt ?? r.NgayTao).Take(5)
                    .Select(r => new LeaveRequestSummaryDto { Id = r.Id, UserFullName = userDetailsMap.TryGetValue(r.UserId, out var u) ? u.HoTen : "...", LoaiPhep = Enums.EnumHelper.GetDisplayName(r.LoaiPhep), NgayTu = r.NgayTu }).ToList(),

                RejectedOrCancelledList = overviewRequests.Where(r => r.TrangThai == LeaveRequestStatus.Rejected || r.TrangThai == LeaveRequestStatus.Cancelled).OrderByDescending(r => r.LastUpdatedAt ?? r.NgayTao).Take(5)
                    .Select(r => new LeaveRequestSummaryDto { Id = r.Id, UserFullName = userDetailsMap.TryGetValue(r.UserId, out var u) ? u.HoTen : "...", LoaiPhep = Enums.EnumHelper.GetDisplayName(r.LoaiPhep), NgayTu = r.NgayTu }).ToList(),
            };

            return stats;
        }

        public async Task<IEnumerable<UserDetailDto>> GetAllUsersWithLeaveDetailsAsync(
                    ClaimsPrincipal requester,
                    bool isDeleted,
                    string? searchTerm,
                    string? role)
        {
            List<UserDetailDto> allUsers;
            try
            {
                var client = _httpClientFactory.CreateClient("UserClient");
                var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(token);
                }

                // Vẫn gọi lấy danh sách user như cũ
                var query = new List<string> { $"isDeleted={isDeleted.ToString().ToLower()}" };
                if (!string.IsNullOrWhiteSpace(searchTerm)) query.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
                if (!string.IsNullOrWhiteSpace(role)) query.Add($"role={Uri.EscapeDataString(role)}");
                var url = $"/api/Users?{string.Join("&", query)}";
                allUsers = await client.GetFromJsonAsync<List<UserDetailDto>>(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi UserService để lấy danh sách user.");
                throw;
            }

            if (allUsers == null || !allUsers.Any())
            {
                return new List<UserDetailDto>();
            }

            // ================== LOGIC LỌC CHO SUPERUSER ĐƯỢC THÊM VÀO ĐÂY ==================
            if (requester.IsInRole("SuperUser") && !requester.IsInRole("Admin"))
            {
                var superUserDepartment = requester.FindFirstValue("Department");
                // Chỉ giữ lại những user trong cùng bộ phận với SuperUser
                allUsers = allUsers.Where(u => u.BoPhan == superUserDepartment).ToList();
            }
            // ==============================================================================

            var userIds = allUsers.Select(u => u.Id).ToList();
            var currentYear = DateTime.UtcNow.Year;
            var allLeaveRequests = await _repository.GetAllAsync(null, null);

            var approvedLeaveRequests = allLeaveRequests
                .Where(r => r.TrangThai == LeaveRequestStatus.Approved &&
                            r.NgayTu.Year == currentYear &&
                            userIds.Contains(r.UserId));

            var leaveDaysTakenByUser = approvedLeaveRequests
                .GroupBy(r => r.UserId)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.DurationInDays));

            foreach (var userDto in allUsers)
            {
                double totalEntitlement = (userDto.TotalLeaveEntitlement > 0) ? userDto.TotalLeaveEntitlement : 12.0;
                var daysTaken = leaveDaysTakenByUser.TryGetValue(userDto.Id, out var taken) ? taken : 0;
                userDto.TotalLeaveEntitlement = (int)totalEntitlement;
                userDto.DaysTaken = daysTaken;
                userDto.RemainingLeaveDays = totalEntitlement - daysTaken;
            }

            return allUsers;
        }

    }
}
