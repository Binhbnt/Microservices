using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using SubscriptionService.Enums;
using SubscriptionService.DTOs;
using SubscriptionService.Models;

namespace SubscriptionService.Controllers
{
    [ApiController]
    [Route("api/enums")]
    public class EnumController : ControllerBase
    {
        [HttpGet("service-types")]
        public IActionResult GetServiceTypes()
        {
            var list = EnumHelper.GetEnumDisplayList<ServiceType>();
            return Ok(list);
        }
    }
    
}
