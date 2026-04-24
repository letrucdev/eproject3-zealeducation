using MediatR;

namespace ZealEducation.Application.Features.Courses.Queries.GetCourseStatistics;

public record GetCourseStatisticsQuery() : IRequest<CourseStatisticsDto>;
