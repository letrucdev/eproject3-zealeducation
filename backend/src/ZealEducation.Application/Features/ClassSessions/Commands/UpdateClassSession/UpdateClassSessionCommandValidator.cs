using FluentValidation;

namespace ZealEducation.Application.Features.ClassSessions.Commands.UpdateClassSession;

public class UpdateClassSessionCommandValidator : AbstractValidator<UpdateClassSessionCommand>
{
    public UpdateClassSessionCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("End time must be after start time.");

        RuleFor(x => x.Topic).MaximumLength(200);
        RuleFor(x => x.Location).MaximumLength(100);
        RuleFor(x => x.Status).IsInEnum();
    }
}
