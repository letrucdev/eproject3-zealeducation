using MediatR;

namespace ZealEducation.Application.Features.Courses.Commands.CreateCourse;

public record CreateCourseCommand : IRequest<Guid>
{
    public string Title { get; init; } = default!;
    public string? Description { get; init; }
    public decimal Price { get; init; }
}
