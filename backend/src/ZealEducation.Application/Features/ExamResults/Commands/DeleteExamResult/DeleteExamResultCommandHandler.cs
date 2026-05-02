using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.ExamResults.Commands.DeleteExamResult;

public class DeleteExamResultCommandHandler(
    IRepository<ExamResult> examResultRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteExamResultCommand, Unit>
{
    public async Task<Unit> Handle(DeleteExamResultCommand request, CancellationToken cancellationToken)
    {
        var result = await examResultRepository.GetByIdAsync(request.ResultId, cancellationToken)
            ?? throw new NotFoundException(nameof(ExamResult), request.ResultId);

        examResultRepository.Delete(result);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
