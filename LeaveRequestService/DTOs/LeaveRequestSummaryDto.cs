namespace LeaveRequestService.DTO;
public class LeaveRequestSummaryDto
{
    public int Id { get; set; }
    public string UserFullName { get; set; }
    public string LoaiPhep { get; set; }
    public DateTime NgayTu { get; set; }
}