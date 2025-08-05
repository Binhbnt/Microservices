using System.ComponentModel.DataAnnotations;

namespace LeaveRequestService.DTOs;
public class ProcessRevocationDto
{
    [Required]
    public string Token { get; set; }
    // Có thể thêm các trường khác nếu cần, ví dụ lý do duyệt thu hồi
}