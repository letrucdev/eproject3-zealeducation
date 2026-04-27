using FluentValidation;

namespace ZealEducation.Application.Features.ClassSessions.Commands.CreateClassSession;

public class CreateClassSessionCommandValidator : AbstractValidator<CreateClassSessionCommand>
{
    public CreateClassSessionCommandValidator()
    {
        RuleFor(x => x.BatchId).NotEmpty();

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("End time must be after start time.");

        RuleFor(x => x.Topic).MaximumLength(200);
        RuleFor(x => x.Location).MaximumLength(100);
    }
}
