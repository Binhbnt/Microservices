// File: Enum/ServiceType.cs
using System.ComponentModel.DataAnnotations;
namespace SubscriptionService.Models;

// Dùng enum để quản lý loại dịch vụ một cách nhất quán
public enum ServiceType
{
    [Display(Name = "Tên Miền")]
    Domain = 0,

    [Display(Name = "Hosting")]
    Hosting = 1,

    [Display(Name = "SSL")]
    SSL = 2,

    [Display(Name = "VPN")]
    VPN = 3,

    [Display(Name = "Bản quyền Phần mềm")]
    SoftwareLicense = 4,

    [Display(Name = "Khác")]
    Other = 99
}