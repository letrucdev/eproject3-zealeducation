namespace ZealEducation.Application.Features.Courses.Queries.GetCourses;

public class CourseListItemDto
{
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = default!;
    public string? Description { get; set; }
    public int DurationWeeks { get; set; }
    public decimal BaseFee { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
