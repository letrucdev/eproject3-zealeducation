using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Courses.Commands.UpdateCourse;

public class UpdateCourseCommandHandler(
    IRepository<Course> courseRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateCourseCommand, Unit>
{
    public async Task<Unit> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await courseRepository.GetByIdAsync(request.CourseId, cancellationToken)
            ?? throw new NotFoundException(nameof(Course), request.CourseId);

        var courseName = request.CourseName.Trim();

        if (!string.Equals(course.CourseName, courseName, StringComparison.Ordinal))
        {
            var duplicates = await courseRepository.FindAsync(
                c => c.CourseName == courseName && c.Id != course.Id,
                cancellationToken);
            if (duplicates.Count > 0)
                throw new ConflictException("Course name is already in use.");
        }

        course.CourseName = courseName;
        course.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        course.DurationWeeks = request.DurationWeeks;
        course.BaseFee = request.BaseFee;
        course.IsActive = request.IsActive;

        courseRepository.Update(course);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
