using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using LeaveRequestService.Enums;
using LeaveRequestService.DTOs;

namespace LeaveRequestService.Controllers
{
    [ApiController]
    [Route("api/enums")]
    public class EnumController : ControllerBase
    {
        [HttpGet("leave-request-status")]
        public IActionResult GetLeaveRequestStatuses()
        {
            var list = EnumHelper.GetEnumDisplayList<LeaveRequestStatus>();
            return Ok(list);
        }

        [HttpGet("leave-types")]
        public IActionResult GetLeaveTypes()
        {
            var list = EnumHelper.GetEnumDisplayList<LeaveType>();
            return Ok(list);
        }

        [HttpGet("user-roles")]
        public IActionResult GetUserRoles()
        {
            var list = EnumHelper.GetEnumDisplayList<UserRole>();
            return Ok(list);
        }
    }
    
}
