using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Interfaces;

namespace ZealEducation.Application.Features.Courses.Queries.GetCourses;

public class GetCoursesQueryHandler : IRequestHandler<GetCoursesQuery, List<CourseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCoursesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CourseDto>> Handle(GetCoursesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Courses
            .AsNoTracking()
            .Select(c => new CourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Price = c.Price,
                IsPublished = c.IsPublished,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
