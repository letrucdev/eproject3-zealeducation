using FluentValidation;

namespace ZealEducation.Application.Features.Batches.Commands.AssignCandidatesToBatch;

public class AssignCandidatesToBatchCommandValidator : AbstractValidator<AssignCandidatesToBatchCommand>
{
    public AssignCandidatesToBatchCommandValidator()
    {
        RuleFor(x => x.BatchId).NotEmpty();

        RuleFor(x => x.EnrollmentIds)
            .NotEmpty().WithMessage("At least one enrollment must be selected.");

        RuleForEach(x => x.EnrollmentIds).NotEmpty();
    }
}
