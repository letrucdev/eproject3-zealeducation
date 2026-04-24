using FluentValidation;

namespace ZealEducation.Application.Features.Batches.Commands.AssignFaculty;

public class AssignFacultyCommandValidator : AbstractValidator<AssignFacultyCommand>
{
    public AssignFacultyCommandValidator()
    {
        RuleFor(x => x.BatchId).NotEmpty();
    }
}
