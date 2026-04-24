using FluentValidation;

namespace ZealEducation.Application.Features.Batches.Commands.DeleteBatch;

public class DeleteBatchCommandValidator : AbstractValidator<DeleteBatchCommand>
{
    public DeleteBatchCommandValidator()
    {
        RuleFor(x => x.BatchId).NotEmpty();
    }
}
