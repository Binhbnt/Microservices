
using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization; // Thêm để sử dụng ILogger

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/Auth")] // Route sẽ là api/Auth
    public class AuthController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;
        private readonly ILogger<AuthController> _logger; // Khai báo logger

        public AuthController(IUserManagementService userManagementService, ILogger<AuthController> logger) // Inject ILogger
        {
            _userManagementService = userManagementService;
            _logger = logger; // Gán logger
        }

        [HttpPost("login")] // Endpoint sẽ là POST /api/Auth/login
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            _logger.LogInformation("Login attempt for user: {Username}", loginDto.Username);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login attempt failed due to invalid model state for user: {Username}", loginDto.Username);
                return BadRequest(ModelState);
            }

            var loginResponse = await _userManagementService.AuthenticateUserAsync(loginDto);

            if (loginResponse == null)
            {
                _logger.LogWarning("Login failed: Invalid credentials for user: {Username}", loginDto.Username);
                return Unauthorized(new { Message = "Tên đăng nhập hoặc mật khẩu không đúng." });
            }

            _logger.LogInformation("Login successful for user: {Username}", loginDto.Username);
            return Ok(loginResponse);
        }

        [HttpPost("renew-token")] // Endpoint: POST /api/Auth/renew-token
        [Authorize] // Yêu cầu token hợp lệ để gia hạn
        public async Task<IActionResult> RenewToken()
        {
            // Lấy ID của người dùng từ Claims (từ token hiện tại)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogWarning("Renew token failed: User ID claim not found or invalid.");
                return Unauthorized(new { Message = "Token không hợp lệ hoặc thiếu thông tin." });
            }

            var newToken = await _userManagementService.RenewTokenAsync(userId);

            if (newToken == null)
            {
                _logger.LogWarning("Renew token failed: User {UserId} not found or inactive.", userId);
                return Unauthorized(new { Message = "Người dùng không hợp lệ hoặc không hoạt động." });
            }

            _logger.LogInformation("Token renewed successfully for user: {UserId}", userId);
            return Ok(new { token = newToken }); // Trả về token mới
        }
    }
}