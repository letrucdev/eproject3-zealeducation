using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Courses.Queries.GetCourses;

public class GetCoursesQueryHandler(
    IRepository<Course> courseRepository) : IRequestHandler<GetCoursesQuery, PaginatedList<CourseListItemDto>>
{
    public async Task<PaginatedList<CourseListItemDto>> Handle(GetCoursesQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

        var query = courseRepository.Query();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(c => c.CourseName.ToLower().Contains(search));
        }

        if (request.IsActive.HasValue)
        {
            var isActive = request.IsActive.Value;
            query = query.Where(c => c.IsActive == isActive);
        }

        var sortKey = (request.SortBy ?? string.Empty).Trim().ToLower();
        var direction = (request.SortDirection ?? "asc").Trim().ToLower();

        var ordered = (sortKey, direction) switch
        {
            ("coursename", "desc") => query.OrderByDescending(c => c.CourseName),
            ("coursename", _) => query.OrderBy(c => c.CourseName),
            ("basefee", "desc") => query.OrderByDescending(c => c.BaseFee),
            ("basefee", _) => query.OrderBy(c => c.BaseFee),
            ("isactive", "desc") => query.OrderByDescending(c => c.IsActive),
            ("isactive", _) => query.OrderBy(c => c.IsActive),
            ("updatedat", "asc") => query.OrderBy(c => c.UpdatedAt),
            ("updatedat", _) => query.OrderByDescending(c => c.UpdatedAt),
            ("createdat", "asc") => query.OrderBy(c => c.CreatedAt),
            _ => query.OrderByDescending(c => c.CreatedAt),
        };

        var projected = ordered
            .ThenBy(c => c.CourseName)
            .Select(c => new CourseListItemDto
            {
                CourseId = c.Id,
                CourseName = c.CourseName,
                Description = c.Description,
                DurationWeeks = c.DurationWeeks,
                BaseFee = c.BaseFee,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            });

        return await PaginatedList<CourseListItemDto>.CreateAsync(projected, page, pageSize);
    }
}
