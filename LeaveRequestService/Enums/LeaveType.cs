using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace LeaveRequestService.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LeaveType
    {
        [Display(Name = "Phép Năm")]
        PhepNam,
        [Display(Name = "Phép Bệnh")]
        PhepBenh,
        [Display(Name = "Nghỉ không lương")]
        NghiKhongLuong,
        [Display(Name = "Nghỉ chế độ")]
        NghiCheDo
    }
}