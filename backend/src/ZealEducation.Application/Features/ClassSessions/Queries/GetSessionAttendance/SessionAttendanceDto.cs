using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.ClassSessions.Queries.GetSessionAttendance;

public class SessionAttendanceDto
{
    public Guid SessionId { get; set; }
    public Guid BatchId { get; set; }
    public string BatchCode { get; set; } = default!;
    public DateOnly SessionDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? Topic { get; set; }
    public string? Location { get; set; }
    public ClassSessionStatus Status { get; set; }
    public List<AttendanceRowDto> Rows { get; set; } = [];
}

public class AttendanceRowDto
{
    public Guid EnrollmentId { get; set; }
    public Guid CandidateId { get; set; }
    public string CandidateCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public AttendanceStatus? Status { get; set; }
    public string? Remarks { get; set; }
}
