using AutoMapper;
using AuditLogService.DTOs;
using AuditLogService.Models;

namespace AuditLogService.Profiles;

public class AuditLogProfile : Profile
{
    public AuditLogProfile()
    {
        // Ánh xạ từ DTO tạo mới sang Model để lưu vào DB
        CreateMap<AuditLogCreateDto, AuditLog>();

        // Ánh xạ từ Model trong DB sang DTO để trả về cho người dùng xem
        CreateMap<AuditLog, AuditLogReadDto>();
    }
}