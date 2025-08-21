using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace LeaveRequestService.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LeaveRequestStatus
    {
        [Display(Name = "Chờ duyệt")]
        Pending = 0, //Chờ duyệt
        [Display(Name = "Đã duyệt")]
        Approved = 1, //Duyệt
        [Display(Name = "Từ chối")]
        Rejected = 2, //Từ chối
        [Display(Name = "Hủy")]
        Cancelled = 3, //Hủy khi cấp trên chưa duyệt
        [Display(Name = "Chờ duyệt thu hồi")]
        PendingRevocation = 4 //Chờ duyệt thu hồi
    }
}