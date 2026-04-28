using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.ClassSessions.Queries.GetBatchSessions;

public class ClassSessionDto
{
    public Guid SessionId { get; set; }
    public Guid BatchId { get; set; }
    public DateOnly SessionDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? Topic { get; set; }
    public string? Location { get; set; }
    public ClassSessionStatus Status { get; set; }
    public int AttendanceMarkedCount { get; set; }
}
