using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyBatches;

public class MyBatchListItemDto
{
    public Guid EnrollmentId { get; set; }
    public Guid BatchId { get; set; }
    public string BatchCode { get; set; } = default!;
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = default!;
    public Guid? FacultyId { get; set; }
    public string? FacultyName { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Location { get; set; }
    public BatchStatus BatchStatus { get; set; }
    public EnrollmentStatus EnrollmentStatus { get; set; }
    public DateOnly EnrollmentDate { get; set; }
}
