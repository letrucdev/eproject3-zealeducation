using FluentValidation;

namespace ZealEducation.Application.Features.ClassSessions.Commands.MarkAttendance;

public class MarkAttendanceCommandValidator : AbstractValidator<MarkAttendanceCommand>
{
    public MarkAttendanceCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();

        RuleFor(x => x.Entries)
            .NotEmpty().WithMessage("At least one attendance entry is required.");

        RuleForEach(x => x.Entries).ChildRules(entry =>
        {
            entry.RuleFor(e => e.EnrollmentId).NotEmpty();
            entry.RuleFor(e => e.Status).IsInEnum();
            entry.RuleFor(e => e.Remarks).MaximumLength(500);
        });
    }
}
