using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Courses.Queries.GetCourseStatistics;

public class GetCourseStatisticsQueryHandler(
    IRepository<Course> courseRepository) : IRequestHandler<GetCourseStatisticsQuery, CourseStatisticsDto>
{
    public async Task<CourseStatisticsDto> Handle(GetCourseStatisticsQuery request, CancellationToken cancellationToken)
    {
        var flags = await courseRepository.Query()
            .Select(c => c.IsActive)
            .ToListAsync(cancellationToken);

        var active = flags.Count(x => x);
        return new CourseStatisticsDto
        {
            Total = flags.Count,
            Active = active,
            Inactive = flags.Count - active
        };
    }
}
