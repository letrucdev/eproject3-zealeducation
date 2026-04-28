using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Courses.Commands.CreateCourse;

public class CreateCourseCommandHandler(
    IRepository<Course> courseRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateCourseCommand, CreateCourseResponse>
{
    public async Task<CreateCourseResponse> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        var courseName = request.CourseName.Trim();

        var duplicates = await courseRepository.FindAsync(c => c.CourseName == courseName, cancellationToken);
        if (duplicates.Count > 0)
            throw new ConflictException("Course name is already in use.");

        var course = new Course
        {
            Id = Guid.NewGuid(),
            CourseName = courseName,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            DurationWeeks = request.DurationWeeks,
            BaseFee = request.BaseFee,
            IsActive = request.IsActive
        };

        await courseRepository.AddAsync(course, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateCourseResponse
        {
            CourseId = course.Id,
            CourseName = course.CourseName,
            DurationWeeks = course.DurationWeeks,
            BaseFee = course.BaseFee,
            IsActive = course.IsActive
        };
    }
}
