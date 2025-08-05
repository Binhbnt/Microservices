using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Mật khẩu cũ là bắt buộc.")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}