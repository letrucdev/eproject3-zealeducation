using MediatR;
using ZealEducation.Application.Common.Models;

namespace ZealEducation.Application.Features.Courses.Queries.GetCourses;

public record GetCoursesQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    bool? IsActive = null) : IRequest<PaginatedList<CourseListItemDto>>;
