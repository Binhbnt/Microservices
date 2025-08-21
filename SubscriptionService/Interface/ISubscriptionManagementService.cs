// File: Interface/ISubscriptionManagementService.cs
using SubscriptionService.Dtos;

namespace SubscriptionService.Interface;

public interface ISubscriptionManagementService
{
    Task<PaginatedResultDto<SubscriptionReadDto>> GetAllAsync(string? searchTerm, int? type, int pageNumber, int pageSize);
    Task<SubscriptionReadDto?> GetByIdAsync(Guid id);
    Task<SubscriptionReadDto> CreateAsync(SubscriptionCreateDto createDto);
    Task<bool> UpdateAsync(Guid id, SubscriptionUpdateDto updateDto);
    Task<bool> DeleteAsync(Guid id);
    Task<ImportResultDto> ImportFromExcelAsync(Stream stream);
    Task<byte[]> GenerateExcelExportAsync(string? searchTerm, int? type);
    Task<byte[]> GenerateExcelTemplateAsync();
    Task<IEnumerable<SubscriptionStatsDto>> GetStatsByTypeAsync();
}