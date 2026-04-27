using MediatR;

namespace ZealEducation.Application.Features.Courses.Commands.CreateCourse;

public record CreateCourseCommand(
    string CourseName,
    string? Description,
    int DurationWeeks,
    decimal BaseFee,
    bool IsActive) : IRequest<CreateCourseResponse>;
