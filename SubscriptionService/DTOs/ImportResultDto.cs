namespace SubscriptionService.Dtos;
public class ImportResultDto
{
    public int SuccessCount { get; set; }
    public int FailCount { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}