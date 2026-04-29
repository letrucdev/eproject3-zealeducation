using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyBatchAttendance;

public class MyBatchAttendanceDto
{
    public Guid BatchId { get; set; }
    public Guid EnrollmentId { get; set; }
    public decimal TotalPracticalHours { get; set; }
    public int TotalSessions { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
    public PaginatedList<MyAttendanceRowDto> Rows { get; set; } = new([], 0, 1, 10);
}

public class MyAttendanceRowDto
{
    public Guid SessionId { get; set; }
    public DateOnly SessionDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? Topic { get; set; }
    public string? Location { get; set; }
    public ClassSessionStatus SessionStatus { get; set; }
    public AttendanceStatus? AttendanceStatus { get; set; }
    public decimal? PracticalHours { get; set; }
    public string? Remarks { get; set; }
}
