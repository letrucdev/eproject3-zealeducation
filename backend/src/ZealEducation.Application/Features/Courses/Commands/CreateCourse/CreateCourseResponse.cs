namespace ZealEducation.Application.Features.Courses.Commands.CreateCourse;

public class CreateCourseResponse
{
    public Guid CourseId { get; init; }
    public string CourseName { get; init; } = default!;
    public int DurationWeeks { get; init; }
    public decimal BaseFee { get; init; }
    public bool IsActive { get; init; }
}
