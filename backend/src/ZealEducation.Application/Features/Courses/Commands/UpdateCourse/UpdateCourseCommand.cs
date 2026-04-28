using MediatR;

namespace ZealEducation.Application.Features.Courses.Commands.UpdateCourse;

public record UpdateCourseCommand(
    Guid CourseId,
    string CourseName,
    string? Description,
    int DurationWeeks,
    decimal BaseFee,
    bool IsActive) : IRequest<Unit>;
