using ZealEducation.Domain.Common;

namespace ZealEducation.Domain.Entities;

public class Course : BaseAuditableEntity
{
    public string CourseName { get; set; } = default!;
    public string? Description { get; set; }
    public int DurationWeeks { get; set; }
    public decimal BaseFee { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<CourseEnquiry> Enquiries { get; set; } = new List<CourseEnquiry>();
}
