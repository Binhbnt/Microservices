using Microsoft.AspNetCore.Mvc;
using UserService.Services;
using UserService.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AutoMapper;
using UserService.Enums;


namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // URL sẽ là /api/Users
    [Authorize]
    public class UsersController : ControllerBase
    {
        // Controller phụ thuộc vào interface của Service, không phải class.
        private readonly IUserManagementService _userManagementService;
        private readonly IMapper _mapper;

        // Hệ thống DI sẽ "tiêm" UserManagementService vào đây.
        public UsersController(IUserManagementService userManagementService, IMapper mapper)
        {
            _userManagementService = userManagementService;
            _mapper = mapper;
        }

        // Định nghĩa một endpoint cho phương thức GET
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserReadDto>>> GetAllUsers(
           [FromQuery] string? searchTerm = null,
           [FromQuery] string? role = null,
           [FromQuery] bool? isDeleted = false) // THÊM THAM SỐ isDeleted
        {
            // Lấy vai trò và phòng ban của user đang đăng nhập từ Claims trong token
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            var currentUserDepartment = User.FindFirstValue("Department"); // Giả sử bạn đã thêm claim "Department"
                                                                           // Nếu là User thường, cấm truy cập ngay lập tức
            if (currentUserRole == "User")
            {
                return Forbid(); // Trả về lỗi 403 Forbidden
            }
            // Truyền thông tin của người dùng hiện tại xuống service
            var users = await _userManagementService.GetAllAsync(
                searchTerm,
                role,
                isDeleted,
                currentUserRole,      // Prop mới
                currentUserDepartment // Prop mới
            );

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userManagementService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(); // Mã 404
            }
            return Ok(user);
        }

        [HttpPost("get-by-ids")]
        public async Task<IActionResult> GetUsersByIds([FromBody] List<int> userIds)
        {
            if (userIds == null || !userIds.Any())
                return BadRequest("Danh sách ID không được trống.");

            var users = await _userManagementService.GetByIdsAsync(userIds);
            return Ok(users);
        }

        // Đánh dấu đây là một endpoint cho phương thức POST
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDto userCreateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Trả về lỗi validation nếu có
            }
            try
            {
                var newUser = await _userManagementService.CreateAsync(userCreateDto, User);
                return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
            }
            catch (InvalidOperationException ex) // BẮT LỖI InvalidOperationException
            {
                // Trả về HTTP 400 Bad Request với thông báo lỗi từ exception
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex) // Bắt các lỗi chung khác
            {
                // Ghi log lỗi chi tiết hơn ở đây cho production
                return StatusCode(500, new { message = "Lỗi server nội bộ khi thêm người dùng.", detail = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto userUpdateDto)
        {
            // Kiểm tra xem ID từ URL có khớp với ID trong body không.
            // Đây là một bước kiểm tra an toàn, đảm bảo tính nhất quán.
            if (id != userUpdateDto.Id)
            {
                return BadRequest("ID trong URL và trong body không khớp.");
            }
            try // <-- BỌC LOGIC TRONG TRY
            {
                var result = await _userManagementService.UpdateAsync(id, userUpdateDto, User);

                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (InvalidOperationException ex) // <-- BẮT LỖI NGHIỆP VỤ
            {
                // Trả về lỗi 400 Bad Request với message từ exception
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex) // Bắt các lỗi chung khác nếu có
            {
                // Ghi log và trả về lỗi 500
                // _logger.LogError(ex, "An unexpected error occurred while updating user {UserId}", id);
                return StatusCode(500, new { message = "Đã có lỗi server xảy ra." });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteUser(int id)
        {
            try
            {
                var result = await _userManagementService.SoftDeleteAsync(id, User);
                if (!result)
                {
                    return NotFound(); // Trả về mã lỗi 404 Not Found.
                }
                // Nếu xóa thành công, trả về mã 204 No Content.
                // Đây cũng là mã chuẩn cho một yêu cầu DELETE thành công.
                return NoContent();
            }
            catch (InvalidOperationException ex) // <-- BẮT LỖI NGHIỆP VỤ
            {
                return BadRequest(new { message = ex.Message });
            }


        }

        [HttpDelete("{id}/permanent")]
        public async Task<IActionResult> DeleteUserPermanent(int id)
        {
            try
            {
                var result = await _userManagementService.DeletePermanentAsync(id, User);
                if (!result) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex) // <-- BẮT LỖI NGHIỆP VỤ
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/restore")] // Hoặc [HttpPatch("{id}/restore")]
        public async Task<IActionResult> RestoreUser(int id)
        {
            try
            {
                var result = await _userManagementService.RestoreAsync(id, User);
                if (!result) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex) // <-- BẮT LỖI NGHIỆP VỤ
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("export")] // Đường dẫn vẫn là /api/Users/export
        public async Task<IActionResult> ExportUsers(
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? role = null,
        [FromQuery] bool? isDeleted = false)
        {
            try
            {
                var excelBytes = await _userManagementService.ExportUsersToExcelAsync(searchTerm, role, isDeleted);

                // MIME type vẫn giữ nguyên
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"User_List_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi khi xuất Excel: {ex.Message}" });
            }
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                // Lấy ID của người dùng đang đăng nhập từ trong token
                // Đây là cách làm bảo mật, đảm bảo user chỉ có thể đổi mật khẩu của chính mình
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                {
                    return Unauthorized("Token không hợp lệ.");
                }

                // Gọi service để xử lý
                await _userManagementService.ChangePasswordAsync(userId, changePasswordDto);

                return Ok(new { Message = "Đổi mật khẩu thành công." });
            }
            catch (InvalidOperationException ex)
            {
                // Bắt lỗi mật khẩu cũ không đúng
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                // Bắt các lỗi không lường trước khác
                // _logger.LogError(ex, "Error changing password for user {UserId}", userIdString);
                return StatusCode(500, new { Message = "Đã có lỗi xảy ra, vui lòng thử lại." });
            }
        }

        [HttpPost("avatar")] // Route sẽ là: POST /api/Users/avatar
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAvatar([FromForm] UploadAvatarDto dto)
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdString, out int userId))
                {
                    return Unauthorized();
                }

                var file = dto.File;

                if (file == null || file.Length == 0)
                {
                    return BadRequest("Không có file nào được tải lên.");
                }

                var newAvatarUrl = await _userManagementService.UpdateAvatarAsync(userId, file);

                return Ok(new { avatarUrl = newAvatarUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi nội bộ: {ex.Message}");
            }
        }

        [HttpGet("{id}/details")]
        public async Task<ActionResult<UserDetailDto>> GetUserDetailsForService(int id)
        {
            var userReadDto = await _userManagementService.GetByIdAsync(id);
            if (userReadDto == null) return NotFound();

            var userDetails = _mapper.Map<UserDetailDto>(userReadDto); // Dùng AutoMapper
            return Ok(userDetails);
        }

        [HttpGet("by-role")]
        [Authorize] // Chỉ Admin hoặc SuperUser mới được quyền gọi API này
        public async Task<IActionResult> GetUsersByRole([FromQuery] UserRole role)
        {
            // Chỉ cho phép lấy danh sách Admin hoặc SuperUser để bảo mật
            if (role != UserRole.Admin && role != UserRole.SuperUser)
            {
                return BadRequest("Vai trò không hợp lệ.");
            }

            var users = await _userManagementService.GetByRoleAsync(role);
            return Ok(users);
        }

        [HttpGet("superuser-by-department")]
        [Authorize] // Bất kỳ ai đã đăng nhập đều có thể gọi
        public async Task<IActionResult> GetSuperUserByDepartment([FromQuery] string department)
        {
            if (string.IsNullOrWhiteSpace(department))
            {
                return BadRequest("Tên bộ phận không được để trống.");
            }

            var superUser = await _userManagementService.FindSuperUserInDepartmentAsync(department);
            if (superUser == null)
            {
                return NotFound($"Không tìm thấy SuperUser cho bộ phận '{department}'.");
            }
            return Ok(superUser);
        }

        [HttpGet("by-department")]
        public async Task<IActionResult> GetUsersByDepartment([FromQuery] string department)
        {
            if (string.IsNullOrWhiteSpace(department))
            {
                return BadRequest("Tên bộ phận không được để trống.");
            }
        
            var users = await _userManagementService.GetUsersByDepartmentAsync(department);
            
            // Chỉ trả về danh sách ID cho NotificationService
            var userIds = users.Select(u => u.Id).ToList();
        
            return Ok(userIds);
        }
    }
}