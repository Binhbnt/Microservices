using LeaveRequestService.DTO;

namespace LeaveRequestService.DTOs;

public class LeaveRequestDashboardStatsDto
{
    public int TotalRequests { get; set; }
    public int PendingRequests { get; set; }
    public int ApprovedRequests { get; set; }
    public int RejectedRequests { get; set; }
    public int CancelledRequests { get; set; }
    public int WaitingRevocation { get; set; }

    public List<LeaveRequestByDateDto> DailyCounts { get; set; } = new();
    public List<LeaveRequestSummaryDto> PendingList { get; set; }
    public List<LeaveRequestSummaryDto> ApprovedList { get; set; }
    public List<LeaveRequestSummaryDto> RejectedOrCancelledList { get; set; }

    public double CurrentUserTotalEntitlement { get; set; }
    public double CurrentUserDaysTaken { get; set; }    
    public double CurrentUserDaysRemaining { get; set; }
}

