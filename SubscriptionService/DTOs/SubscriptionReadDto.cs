// File: Dtos/SubscriptionReadDto.cs
namespace SubscriptionService.Dtos;

public class SubscriptionReadDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string ExpiryDate { get; set; } // Format "dd/MM/yyyy"
    public int DaysRemaining { get; set; } // Số ngày còn lại (sẽ được tính toán)
    public string? Provider { get; set; }
    public string? Note { get; set; }
}