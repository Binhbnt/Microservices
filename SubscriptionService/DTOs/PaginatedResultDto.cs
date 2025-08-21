// File: Dtos/PaginatedResultDto.cs

namespace SubscriptionService.Dtos;

public class PaginatedResultDto<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }
}