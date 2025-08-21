// File: Models/SubscribedService.cs
namespace SubscriptionService.Models;

// Class này có cấu trúc tương ứng với các cột trong bảng subscribed_services
public class SubscribedService
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ServiceType Type { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string? Provider { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
     public int SortOrder { get; set; }
}