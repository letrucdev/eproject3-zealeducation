using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Batches.Queries.GetBatchById;

public class BatchDetailDto
{
    public Guid BatchId { get; set; }
    public string BatchCode { get; set; } = default!;
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = default!;
    public Guid? FacultyId { get; set; }
    public string? FacultyName { get; set; }
    public string? FacultyCode { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Location { get; set; }
    public int MaxCapacity { get; set; }
    public int EnrolledCount { get; set; }
    public BatchStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
