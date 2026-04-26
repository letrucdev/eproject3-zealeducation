using ZealEducation.Domain.Common;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Domain.Entities;

public class Batch : BaseAuditableEntity
{
    public Guid CourseId { get; set; }
    public string BatchCode { get; set; } = default!;
    public Guid? FacultyId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Location { get; set; }
    public int MaxCapacity { get; set; } = 30;
    public BatchStatus Status { get; set; } = BatchStatus.NeedsInstructor;

    public Course Course { get; set; } = default!;
    public Faculty? Faculty { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; } = [];
    public ICollection<ClassSession> ClassSessions { get; set; } = [];
}
