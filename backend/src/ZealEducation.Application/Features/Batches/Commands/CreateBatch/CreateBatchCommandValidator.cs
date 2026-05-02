using FluentValidation;

namespace ZealEducation.Application.Features.Batches.Commands.CreateBatch;

public class CreateBatchCommandValidator : AbstractValidator<CreateBatchCommand>
{
    public CreateBatchCommandValidator()
    {
        RuleFor(x => x.BatchCode)
            .NotEmpty().WithMessage("Batch code is required")
            .MaximumLength(30);

        RuleFor(x => x.CourseId).NotEmpty().WithMessage("Course is required");

        RuleFor(x => x.Location).MaximumLength(100);

        RuleFor(x => x.MaxCapacity)
            .GreaterThan(0).WithMessage("Max capacity must be greater than 0")
            .LessThanOrEqualTo(500).WithMessage("Max capacity is unrealistic");

        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("Start date must be today or later.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date");
    }
}
