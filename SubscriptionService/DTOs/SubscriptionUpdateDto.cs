// File: Dtos/SubscriptionUpdateDto.cs
using System.ComponentModel.DataAnnotations;
using SubscriptionService.Models;

namespace SubscriptionService.Dtos;

public class SubscriptionUpdateDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    [Required]
    public ServiceType Type { get; set; }

    [Required]
    public DateTime ExpiryDate { get; set; }

    [MaxLength(100)]
    public string? Provider { get; set; }

    public string? Note { get; set; }
}