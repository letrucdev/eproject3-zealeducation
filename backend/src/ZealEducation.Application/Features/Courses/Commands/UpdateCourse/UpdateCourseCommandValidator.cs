using FluentValidation;

namespace ZealEducation.Application.Features.Courses.Commands.UpdateCourse;

public class UpdateCourseCommandValidator : AbstractValidator<UpdateCourseCommand>
{
    public UpdateCourseCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();

        RuleFor(x => x.CourseName)
            .NotEmpty().WithMessage("Course name is required")
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(4000);

        RuleFor(x => x.DurationWeeks)
            .GreaterThan(0).WithMessage("Duration must be greater than 0 weeks")
            .LessThanOrEqualTo(520).WithMessage("Duration is unrealistic");

        RuleFor(x => x.BaseFee)
            .GreaterThanOrEqualTo(0).WithMessage("Base fee cannot be negative");
    }
}
