using FluentValidation;

namespace ZealEducation.Application.Features.ClassSessions.Commands.CreateBulkSessions;

public class CreateBulkSessionsCommandValidator : AbstractValidator<CreateBulkSessionsCommand>
{
    public CreateBulkSessionsCommandValidator()
    {
        RuleFor(x => x.BatchId).NotEmpty();

        RuleFor(x => x.DaysOfWeek)
            .NotNull()
            .Must(d => d.Count > 0).WithMessage("Select at least one day of the week.")
            .Must(d => d.Distinct().Count() == d.Count).WithMessage("Days of week must not repeat.");

        RuleForEach(x => x.DaysOfWeek).IsInEnum();

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("End time must be after start time.");

        RuleFor(x => x.Topic).MaximumLength(200);
        RuleFor(x => x.Location).MaximumLength(100);
    }
}
