using MediatR;

namespace ZealEducation.Application.Features.Courses.Queries.GetCourseById;

public record GetCourseByIdQuery(Guid CourseId) : IRequest<CourseDetailDto>;
