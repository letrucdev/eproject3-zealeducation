using MediatR;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Courses.Queries.GetCourses;

public class GetCoursesQueryHandler(IRepository<Course> courseRepository) : IRequestHandler<GetCoursesQuery, List<CourseDto>>
{
    public async Task<List<CourseDto>> Handle(GetCoursesQuery request, CancellationToken cancellationToken)
    {
        var courses = await courseRepository.GetAllAsync(cancellationToken);

        return [.. courses.Select(c => new CourseDto
        {
            Id = c.Id,
            Title = c.Title,
            Description = c.Description,
            Price = c.Price,
            IsPublished = c.IsPublished,
            CreatedAt = c.CreatedAt
        })];
    }
}
