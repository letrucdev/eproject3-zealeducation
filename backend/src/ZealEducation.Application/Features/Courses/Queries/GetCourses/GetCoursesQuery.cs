using MediatR;

namespace ZealEducation.Application.Features.Courses.Queries.GetCourses;

public record GetCoursesQuery : IRequest<List<CourseDto>>;

public record CourseDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = default!;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public bool IsPublished { get; init; }
    public DateTime CreatedAt { get; init; }
}
