// File: LeaveRequestService/DTOs/PaginatedResultDto.cs
namespace LeaveRequestService.DTOs;

public class PaginatedResultDto<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }
}