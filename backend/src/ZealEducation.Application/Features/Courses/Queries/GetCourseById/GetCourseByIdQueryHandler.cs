using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Courses.Queries.GetCourseById;

public class GetCourseByIdQueryHandler(
    IRepository<Course> courseRepository) : IRequestHandler<GetCourseByIdQuery, CourseDetailDto>
{
    public async Task<CourseDetailDto> Handle(GetCourseByIdQuery request, CancellationToken cancellationToken)
    {
        var course = await courseRepository.GetByIdAsync(request.CourseId, cancellationToken)
            ?? throw new NotFoundException(nameof(Course), request.CourseId);

        return new CourseDetailDto
        {
            CourseId = course.Id,
            CourseName = course.CourseName,
            Description = course.Description,
            DurationWeeks = course.DurationWeeks,
            BaseFee = course.BaseFee,
            IsActive = course.IsActive,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt
        };
    }
}
