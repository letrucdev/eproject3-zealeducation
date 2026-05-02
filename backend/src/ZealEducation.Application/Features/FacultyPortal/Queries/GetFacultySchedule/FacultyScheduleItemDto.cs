using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultySchedule;

public class FacultyScheduleItemDto
{
    public Guid SessionId { get; set; }
    public Guid BatchId { get; set; }
    public string BatchCode { get; set; } = default!;
    public string CourseName { get; set; } = default!;
    public DateOnly SessionDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? Topic { get; set; }
    public string? Location { get; set; }
    public ClassSessionStatus Status { get; set; }
}
