using System.Collections.Generic;

namespace NotificationsService.Helpers;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; }
    public PaginationMetadata PaginationMetadata { get; set; }
}