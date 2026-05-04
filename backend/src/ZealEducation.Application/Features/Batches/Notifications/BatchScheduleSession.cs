namespace ZealEducation.Application.Features.Batches.Notifications;

public class BatchScheduleSession
{
    public DateOnly SessionDate { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public string? Topic { get; init; }
    public string? Location { get; init; }
}
