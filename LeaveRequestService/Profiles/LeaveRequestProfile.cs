using AutoMapper;
using LeaveRequestService.DTOs;
using LeaveRequestService.Models;

namespace LeaveRequestService.Profiles;

public class LeaveRequestProfile : Profile
{
    public LeaveRequestProfile()
    {
        // Định nghĩa quy tắc ánh xạ

        // 1. Dùng khi tạo mới: Chuyển dữ liệu từ DTO sang Model để lưu vào DB
        CreateMap<LeaveRequestCreateDto, LeaveRequest>();

        // 2. Dùng khi hiển thị: Chuyển dữ liệu từ Model lấy từ DB sang DTO để trả về
        CreateMap<LeaveRequest, LeaveRequestReadDto>();
        
    }
}