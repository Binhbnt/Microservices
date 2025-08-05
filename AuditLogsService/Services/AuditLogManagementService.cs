using AutoMapper;
using AuditLogService.DTOs;
using AuditLogService.Models;
using AuditLogService.Repositories;
using AuditLogService.Interface;
namespace AuditLogService.Services
{
    public class AuditLogManagementService : IAuditLogManagementService
    {
        private readonly IAuditLogRepository _repository;
        private readonly IMapper _mapper;

        public AuditLogManagementService(IAuditLogRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task CreateLogAsync(AuditLogCreateDto dto)
        {
            // Dùng AutoMapper để chuyển DTO thành Model
            var auditLog = _mapper.Map<AuditLog>(dto);
            // Gán thời gian hiện tại
            auditLog.Timestamp = DateTime.UtcNow;
            // Gọi repository để lưu vào DB
            await _repository.AddAsync(auditLog);
        }

        public async Task<IEnumerable<AuditLogReadDto>> GetLogsAsync()
        {
            // Lấy tất cả log từ DB
            var logs = await _repository.GetAllAsync();
            // Dùng AutoMapper để chuyển danh sách Model thành danh sách DTO và trả về
            return _mapper.Map<IEnumerable<AuditLogReadDto>>(logs);
        }
    }
}