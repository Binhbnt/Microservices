// File: Interface/ISubscriptionRepository.cs
using SubscriptionService.Models;
using SubscriptionService.Dtos;
namespace SubscriptionService.Interface;

public interface ISubscriptionRepository
{
    Task<(IEnumerable<SubscribedService> Services, int TotalCount)> GetAllAsync(string? searchTerm, int? type, int pageNumber, int pageSize);
    Task<SubscribedService?> GetByIdAsync(Guid id);
    Task<SubscribedService> CreateAsync(SubscribedService service);
    Task<bool> UpdateAsync(SubscribedService service);
    Task<bool> DeleteAsync(Guid id);
    Task<int> CreateBatchAsync(IEnumerable<SubscribedService> services);
    Task<IEnumerable<SubscriptionStatsDto>> GetStatsByTypeAsync();
    Task<IEnumerable<SubscribedService>> GetExpiringSubscriptionsAsync(int daysUntilExpiry);
    Task<int> GetMaxSortOrderAsync(); 
}