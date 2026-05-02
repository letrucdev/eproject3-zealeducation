using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyBatchDetail;

public class MyBatchDetailDto
{
    public Guid BatchId { get; set; }
    public string BatchCode { get; set; } = default!;
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = default!;
    public int CourseDurationWeeks { get; set; }
    public Guid? FacultyId { get; set; }
    public string? FacultyName { get; set; }
    public string? FacultyCode { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Location { get; set; }
    public int MaxCapacity { get; set; }
    public int EnrolledCount { get; set; }
    public BatchStatus Status { get; set; }
    public Guid EnrollmentId { get; set; }
    public EnrollmentStatus EnrollmentStatus { get; set; }
    public MyBatchFeedbackStateDto FeedbackState { get; set; } = new();
}

public class MyBatchFeedbackStateDto
{
    public bool CourseSubmitted { get; set; }
    public bool GeneralSubmitted { get; set; }
    public List<Guid> FacultyTargetsSubmitted { get; set; } = [];
}
