using AutoMapper;
using UserService.DTOs;
using UserService.Repositories;
using BCrypt.Net;
using UserService.Models;
using UserService.Enums;
using OfficeOpenXml; // Import chính của EPPlus
using OfficeOpenXml.Style; // Cần cho định dạng style
using System.IO; // Vẫn cần cho MemoryStream
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace UserService.Services
{
    public class UserManagementService : IUserManagementService
    {
        // UserService phụ thuộc vào IUserRepository và IMapper.
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserManagementService> _logger;

        // Hệ thống DI sẽ tự động "tiêm" các đối tượng cần thiết vào đây.
        public UserManagementService(IUserRepository userRepository, IWebHostEnvironment webHostEnvironment,
        IConfiguration configuration, IMapper mapper, IHttpContextAccessor httpContextAccessor,
        IHttpClientFactory httpClientFactory, ILogger<UserManagementService> logger)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _mapper = mapper;
        }

        private async Task LogActionAsync(ClaimsPrincipal requester, string actionType, string? entityType, int? entityId, object details)
        {
            // Lấy thông tin từ người thực hiện request
            var userIdStr = requester.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = requester.FindFirstValue(ClaimTypes.Name);
            int.TryParse(userIdStr, out int userId);

            // TẠO OPTION CHO JSON SERIALIZER
            var serializerOptions = new JsonSerializerOptions
            {
                // Dòng này sẽ giữ lại ký tự tiếng Việt
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            };

            // Tạo object DTO để gửi đi
            var logData = new
            {
                UserId = userId,
                Username = username,
                ActionType = actionType,
                EntityType = entityType,
                EntityId = entityId,
                Details = JsonSerializer.Serialize(details, serializerOptions),
                IsSuccess = true,
                ErrorMessage = (string)null,
                // Lấy IP, nhưng cần cấu hình thêm ở Controller và Program.cs, tạm thời để null
                RequesterIpAddress = (string)null
            };

            try
            {
                var client = _httpClientFactory.CreateClient("AuditLogClient");

                //LẤY TOKEN TỪ REQUEST GỐC MÀ FRONTEND GỬI LÊN
                var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
                //GẮN TOKEN VÀO REQUEST MỚI GỬI ĐẾN AUDITLOGSERVICE
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(token);
                }

                var content = new StringContent(JsonSerializer.Serialize(logData, serializerOptions), System.Text.Encoding.UTF8, "application/json");
                // Gửi request POST đến AuditLogService
                var response = await client.PostAsync("/api/AuditLogs", content);

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FAILED_TO_SEND_AUDIT_LOG. Data: {@LogData}", logData);
            }
        }

        public async Task<IEnumerable<UserReadDto>> GetAllAsync(
        string? searchTerm, string? role, bool? isDeleted,
        string? requesterRole, string? requesterDepartment)
        {
            var users = await _userRepository.GetAllAsync(isDeleted);
            var filteredUsers = users.AsQueryable();
            // === LOGIC PHÂN QUYỀN LỌC DỮ LIỆU ===
            // Nếu người yêu cầu là SuperUser, chỉ lọc ra những user trong cùng phòng ban
            if (requesterRole == "SuperUser" && !string.IsNullOrEmpty(requesterDepartment))
            {
                filteredUsers = filteredUsers.Where(u => u.BoPhan == requesterDepartment);
            }
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                filteredUsers = filteredUsers.Where(u =>
                    (u.MaSoNhanVien != null && u.MaSoNhanVien.ToLower().Contains(lowerSearchTerm)) ||
                    (u.HoTen != null && u.HoTen.ToLower().Contains(lowerSearchTerm)) ||
                    (u.Email != null && u.Email.ToLower().Contains(lowerSearchTerm))
                );
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                if (Enum.TryParse(role, true, out UserRole userRoleEnum))
                {
                    filteredUsers = filteredUsers.Where(u => u.Role == userRoleEnum);
                }
            }

            var userDtos = _mapper.Map<IEnumerable<UserReadDto>>(filteredUsers.ToList());

            return userDtos;
        }

        public async Task<UserReadDto> CreateAsync(UserCreateDto userCreateDto, ClaimsPrincipal requester)
        {
            // Lấy thông tin người thực hiện yêu cầu từ token
            var requesterRole = requester.FindFirstValue(ClaimTypes.Role);
            var requesterDepartment = requester.FindFirstValue("Department");

            // === ÁP DỤNG CÁC QUY TẮC BẢO MẬT KHI TẠO MỚI ===

            // QUY TẮC 1: Không một ai được phép tạo tài khoản có vai trò Admin
            if (userCreateDto.Role == UserRole.Admin)
            {
                throw new InvalidOperationException("Không thể tạo mới tài khoản với vai trò Admin.");
            }

            // QUY TẮC 2: SuperUser có giới hạn khi tạo mới
            if (requesterRole == "SuperUser")
            {
                // SuperUser không được tạo SuperUser khác
                if (userCreateDto.Role == UserRole.SuperUser)
                {
                    throw new InvalidOperationException("SuperUser không có quyền tạo SuperUser khác.");
                }
                // SuperUser chỉ được tạo User trong cùng phòng ban của mình
                if (userCreateDto.BoPhan != requesterDepartment)
                {
                    throw new InvalidOperationException("SuperUser chỉ có thể tạo người dùng trong cùng bộ phận.");
                }
            }

            // Nếu qua được các kiểm tra trên, mới tiến hành validate và tạo user
            var user = _mapper.Map<User>(userCreateDto);
            await _ValidateUserAndPermissionsAsync(user); // Validate trùng email, mã NV, SuperUser/dept

            user.MatKhau = BCrypt.Net.BCrypt.HashPassword(userCreateDto.MatKhau);
            user.NgayTao = DateTime.UtcNow;
            user.IsDeleted = false;

            // Lấy ID của người tạo để lưu lại (tùy chọn)
            var createdByUserId = int.Parse(requester.FindFirstValue(ClaimTypes.NameIdentifier));
            user.CreatedByUserId = createdByUserId;

            var createdUser = await _userRepository.CreateAsync(user);
            _ = LogActionAsync(requester, "CREATE_USER", "User", createdUser.Id, new { CreatedUser = createdUser });
            return _mapper.Map<UserReadDto>(createdUser);
        }

        public async Task<UserReadDto?> GetByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;
            return _mapper.Map<UserReadDto>(user);
        }

        public async Task<List<UserDetailDto>> GetByIdsAsync(List<int> ids)
        {
            var users = await _userRepository.GetByIdsAsync(ids);
            return users.Select(u => new UserDetailDto
            {
                Id = u.Id,
                HoTen = u.HoTen,
                BoPhan = u.BoPhan,
                ChucVu = u.ChucVu,
                Role = u.Role.ToString(),
                MaSoNhanVien = u.MaSoNhanVien
            }).ToList();
        }

        public async Task<bool> UpdateAsync(int id, UserUpdateDto userUpdateDto, ClaimsPrincipal requester)
        {

            var existingUser = await _userRepository.GetByIdAsync(id);

            if (existingUser == null)
            {

                return false;
            }

            // Chụp lại trạng thái cũ trước khi thay đổi
            var oldUserData = new UserReadDto // Chỉ log những thông tin cần thiết
            {
                Id = existingUser.Id,
                HoTen = existingUser.HoTen,
                Email = existingUser.Email,
                Role = existingUser.Role.ToString(),
                BoPhan = existingUser.BoPhan,
                ChucVu = existingUser.ChucVu
            };

            if (existingUser.Role == UserRole.Admin && userUpdateDto.Role != UserRole.Admin)
            {

                throw new InvalidOperationException("Admin không thể tự thay đổi vai trò của chính mình.");
            }
            if (userUpdateDto.Role == UserRole.Admin && existingUser.Role != UserRole.Admin)
            {

                throw new InvalidOperationException("Không thể nâng cấp người dùng khác thành Admin.");
            }
            // Cập nhật thông tin từ DTO vào user hiện tại

            _mapper.Map(userUpdateDto, existingUser);

            // Hàm này sẽ kiểm tra quy tắc "1 SuperUser mỗi phòng ban"

            await _ValidateUserAndPermissionsAsync(existingUser, id);

            existingUser.LastUpdatedAt = DateTime.UtcNow;

            // Gọi hàm repository mới, chỉ cập nhật profile
            await _userRepository.UpdateProfileAsync(existingUser);

            // Chụp lại trạng thái mới
            var newUserData = new UserReadDto
            {
                Id = existingUser.Id,
                HoTen = existingUser.HoTen,
                Email = existingUser.Email,
                Role = existingUser.Role.ToString(),
                BoPhan = existingUser.BoPhan,
                ChucVu = existingUser.ChucVu
            };
            _ = LogActionAsync(requester, "UPDATE_USER", "User", id, new { OldData = oldUserData, NewData = newUserData });
            return true;
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("Không tìm thấy người dùng.");
            }

            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.OldPassword, user.MatKhau))
            {
                throw new InvalidOperationException("Mật khẩu cũ không chính xác.");
            }

            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);

            // Gọi hàm repository mới, chỉ cập nhật mật khẩu
            await _userRepository.UpdatePasswordAsync(userId, newPasswordHash);
        }

        public async Task<bool> SoftDeleteAsync(int id, ClaimsPrincipal requester)
        {

            var existingUser = await _userRepository.GetByIdAsync(id);

            if (existingUser == null)
            {
                return false;
            }
            // KIỂM TRA MỚI
            if (existingUser.Role == UserRole.Admin)
            {
                throw new InvalidOperationException("Không thể xóa tài khoản Admin.");
            }
            await _userRepository.SoftDeleteAsync(id);
            _ = LogActionAsync(requester, "SOFT_DELETE_USER", "User", id, new { DeletedUserId = id, DeletedUsername = existingUser.MaSoNhanVien });
            return true;
        }

        public async Task<bool> DeletePermanentAsync(int id, ClaimsPrincipal requester)
        {
            var userExists = await _userRepository.GetByIdAsync(id, true);
            if (userExists == null)
            {
                return false;
            }
            await _userRepository.DeletePermanentAsync(id);
            _ = LogActionAsync(requester, "DELETE_PERMANENT_USER", "User", id, new { DeletedUserId = id, DeletedUsername = userExists.MaSoNhanVien });
            return true;
        }

        public async Task<bool> RestoreAsync(int id, ClaimsPrincipal requester)
        {
            // Đầu tiên, kiểm tra xem user có tồn tại trong database (kể cả đã xóa mềm)
            var userExists = await _userRepository.GetByIdAsync(id, true);

            if (userExists == null)
            {

                return false;
            }

            // Sau đó, gọi Repository để thực hiện khôi phục
            await _userRepository.RestoreAsync(id);

            _ = LogActionAsync(requester, "RESTORE_USER", "User", id, new { RestoredUserId = id, RestoredUsername = userExists.MaSoNhanVien });

            return true;
        }

        // Đặt hàm này vào trong file UserManagementService.cs

        private async Task _ValidateUserAndPermissionsAsync(User user, int? userToExcludeId = null)
        {
            // === KIỂM TRA EMAIL VÀ MÃ SỐ NHÂN VIÊN (Giữ nguyên logic cũ) ===
            var existingUserByEmail = await _userRepository.GetUserByEmailAsync(user.Email);
            if (existingUserByEmail != null && existingUserByEmail.Id != userToExcludeId)
            {
                throw new InvalidOperationException("Email đã tồn tại.");
            }

            var existingUserByMaSoNV = await _userRepository.GetUserByMaSoNhanVienAsync(user.MaSoNhanVien);
            if (existingUserByMaSoNV != null && existingUserByMaSoNV.Id != userToExcludeId)
            {
                throw new InvalidOperationException("Mã số nhân viên đã tồn tại.");
            }

            // === KIỂM TRA MỚI: Logic cho SuperUser ===
            // Nếu vai trò đang được set là SuperUser và có phòng ban
            if (user.Role == UserRole.SuperUser && !string.IsNullOrEmpty(user.BoPhan))
            {
                // Tìm xem đã có SuperUser nào khác trong phòng ban này chưa
                var existingSuperUser = await _userRepository.FindSuperUserInDepartmentAsync(user.BoPhan);

                // Nếu tìm thấy một SuperUser khác (có ID khác với user đang được xét)
                if (existingSuperUser != null && existingSuperUser.Id != userToExcludeId)
                {
                    // Ném ra lỗi với thông báo đúng như bạn yêu cầu
                    throw new InvalidOperationException($"Phòng ban này đã có user: {existingSuperUser.HoTen} đang là SuperUser.");
                }
            }
        }

        public async Task<byte[]> ExportUsersToExcelAsync(string? searchTerm = null, string? role = null, bool? isDeleted = false)
        {
            // Bước 1: Lấy danh sách người dùng dựa trên các tiêu chí lọc.
            var usersToExport = (await _userRepository.GetAllAsync(isDeleted))
                                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                usersToExport = usersToExport.Where(u =>
                    (u.MaSoNhanVien != null && u.MaSoNhanVien.ToLower().Contains(lowerSearchTerm)) ||
                    (u.HoTen != null && u.HoTen.ToLower().Contains(lowerSearchTerm)) ||
                    (u.Email != null && u.Email.ToLower().Contains(lowerSearchTerm))
                );
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                if (Enum.TryParse(role, true, out UserRole userRoleEnum))
                {
                    usersToExport = usersToExport.Where(u => u.Role == userRoleEnum);
                }
            }
            var finalUsersToExport = usersToExport.OrderBy(u => u.Id).ToList(); // Chuyển về List

            // Bước 2: Tạo file Excel bằng EPPlus
            // Đặt giấy phép cho EPPlus (chỉ cần thiết cho phiên bản 5.x trở lên, cho NonCommercial License)
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Hoặc LicenseContext.Commercial nếu bạn có giấy phép thương mại

            using (var package = new ExcelPackage()) // Tạo một package Excel mới
            {
                var worksheet = package.Workbook.Worksheets.Add("Users"); // Thêm một sheet tên là "Users"

                // Thêm tiêu đề cột (Header Row)
                int col = 1;
                worksheet.Cells[1, col++].Value = "Mã số NV";
                worksheet.Cells[1, col++].Value = "Họ tên";
                worksheet.Cells[1, col++].Value = "Email";
                worksheet.Cells[1, col++].Value = "Chức vụ";
                worksheet.Cells[1, col++].Value = "Bộ phận";
                worksheet.Cells[1, col++].Value = "Vai trò";
                worksheet.Cells[1, col++].Value = "Ngày tạo";
                // worksheet.Cells[1, col++].Value = "Trạng thái xóa";
                // Nếu có cột "Phép Được Hưởng"
                // worksheet.Cells[1, col++].Value = "Phép Được Hưởng";

                // Định dạng header (tùy chọn)
                using (var range = worksheet.Cells[1, 1, 1, col - 1])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray); // System.Drawing.Color
                }

                // Thêm dữ liệu từ danh sách người dùng vào Excel
                for (int i = 0; i < finalUsersToExport.Count; i++)
                {
                    var user = finalUsersToExport[i];
                    int row = i + 2; // Bắt đầu từ hàng thứ 2 sau header

                    col = 1; // Reset cột
                    worksheet.Cells[row, col++].Value = user.MaSoNhanVien;
                    worksheet.Cells[row, col++].Value = user.HoTen;
                    worksheet.Cells[row, col++].Value = user.Email;
                    worksheet.Cells[row, col++].Value = user.ChucVu;
                    worksheet.Cells[row, col++].Value = user.BoPhan;
                    worksheet.Cells[row, col++].Value = user.Role.ToString();
                    // Định dạng ngày giờ, EPPlus có thể tự hiểu DateTime
                    worksheet.Cells[row, col++].Value = user.NgayTao.ToLocalTime();
                    worksheet.Cells[row, col - 1].Style.Numberformat.Format = "dd/MM/yyyy HH:mm"; // Áp dụng định dạng hiển thị
                    // worksheet.Cells[row, col++].Value = user.LicensePoints; // Nếu có cột này
                }

                // Tự động điều chỉnh độ rộng cột (AutoFit)
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Bước 3: Lưu package vào MemoryStream và chuyển thành mảng byte
                using (var stream = new MemoryStream())
                {
                    package.SaveAs(stream); // Lưu package vào stream
                    return stream.ToArray();
                }
            }
        }

        // Phương thức xác thực thông tin đăng nhập
        public async Task<User?> ValidateUserCredentialsAsync(string username, string password)
        {
            // Tìm user theo username (email hoặc mã số NV)
            var user = await _userRepository.FindUserForLoginAsync(username);

            // Kiểm tra user có tồn tại và chưa bị xóa mềm
            // (Nếu user bị IsDeleted = true thì không được đăng nhập)
            if (user == null || user.IsDeleted)
            {
                return null;
            }

            // Xác minh mật khẩu (BCrypt)
            // user.MatKhau ở đây là mật khẩu đã băm trong DB
            // password là mật khẩu plain text từ người dùng
            if (!BCrypt.Net.BCrypt.Verify(password, user.MatKhau))
            {
                return null; // Mật khẩu không khớp
            }

            return user; // Xác thực thành công, trả về đối tượng user
        }

        // Phương thức tạo JWT Token
        public Task<string> GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured.");
            var key = Encoding.ASCII.GetBytes(secretKey);

            // Thêm Claims vào token (thông tin về người dùng)
            var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.MaSoNhanVien),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                    new Claim("Department", user.BoPhan ?? string.Empty),
                    
                    // THÊM CÁC CLAIM NÀY VÀO
                    new Claim("FullName", user.HoTen ?? string.Empty),
                    new Claim("Position", user.ChucVu ?? string.Empty)
                };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(4), // Token hết hạn sau 4 giờ
                //Expires = DateTime.UtcNow.AddMinutes(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Task.FromResult(tokenHandler.WriteToken(token)); // Trả về chuỗi token
        }

        public async Task<LoginResponseDto?> AuthenticateUserAsync(LoginDto loginDto)
        {
            // 1. Xác thực thông tin đăng nhập
            var validatedUser = await ValidateUserCredentialsAsync(loginDto.Username, loginDto.Password);

            if (validatedUser == null)
            {
                return null;
            }

            // ***SỰ THAY ĐỔI NẰM Ở ĐÂY***
            // 2. LẤY LẠI thông tin MỚI NHẤT của user từ database bằng ID
            var latestUserDetails = await _userRepository.GetByIdAsync(validatedUser.Id);

            if (latestUserDetails == null)
            {
                return null;
            }

            // 3. Tạo token với thông tin MỚI NHẤT
            var token = await GenerateJwtToken(latestUserDetails);

            // 4. Ánh xạ từ thông tin MỚI NHẤT, đảm bảo `AvatarUrl` là mới nhất
            var loginResponse = _mapper.Map<LoginResponseDto>(latestUserDetails);
            loginResponse.Token = token;

            return loginResponse;
        }

        public async Task<string?> RenewTokenAsync(int userId)
        {
            // Bước 1: Kiểm tra xem user có tồn tại và đang hoạt động không
            var user = await _userRepository.GetByIdAsync(userId); // GetByIdAsync chỉ lấy user IsDeleted = 0

            if (user == null)
            {
                return null; // User không tồn tại hoặc đã bị xóa mềm
            }

            // Bước 2: Tạo một token mới cho user này
            var newToken = await GenerateJwtToken(user); // Tái sử dụng hàm GenerateJwtToken

            return newToken;
        }

        public async Task<string> UpdateAvatarAsync(int userId, IFormFile avatarFile)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("Không tìm thấy người dùng.");

            if (avatarFile == null || avatarFile.Length == 0)
            {
                throw new ArgumentException("Vui lòng chọn một file ảnh.");
            }

            // Tạo thư mục lưu trữ nếu chưa có
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "avatars");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Tạo tên file duy nhất để tránh trùng lặp
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetExtension(avatarFile.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Lưu file vào server
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await avatarFile.CopyToAsync(fileStream);
            }

            // Cập nhật đường dẫn avatar trong database
            string avatarUrl = $"avatars/{uniqueFileName}";
            user.AvatarUrl = avatarUrl;
            await _userRepository.UpdateProfileAsync(user); // Tái sử dụng hàm update profile

            return avatarUrl; // Trả về URL mới để frontend cập nhật
        }

        public async Task<IEnumerable<UserReadDto>> GetByRoleAsync(UserRole role)
        {
            var users = await _userRepository.GetByRoleAsync(role);
            return _mapper.Map<IEnumerable<UserReadDto>>(users);
        }

        public async Task<UserReadDto?> FindSuperUserInDepartmentAsync(string department)
        {
        var user = await _userRepository.FindSuperUserInDepartmentAsync(department);
        return _mapper.Map<UserReadDto>(user);
        }

        public async Task<IEnumerable<UserReadDto>> GetUsersByDepartmentAsync(string department)
        {
            if (string.IsNullOrWhiteSpace(department))
            {
                return new List<UserReadDto>();
            }

            var users = await _userRepository.GetByDepartmentAsync(department);
            return _mapper.Map<IEnumerable<UserReadDto>>(users);
        }
    }
}