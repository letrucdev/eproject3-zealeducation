using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Examinations.Commands.DeleteExamination;

public class DeleteExaminationCommandHandler(
    IRepository<Examination> examinationRepository,
    IRepository<ExamResult> examResultRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteExaminationCommand, Unit>
{
    public async Task<Unit> Handle(DeleteExaminationCommand request, CancellationToken cancellationToken)
    {
        var examination = await examinationRepository.GetByIdAsync(request.ExaminationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Examination), request.ExaminationId);

        var hasResults = await examResultRepository.Query()
            .AnyAsync(r => r.ExamId == examination.Id, cancellationToken);

        if (hasResults)
            throw new ConflictException("Cannot delete an examination that already has results recorded.");

        examinationRepository.Delete(examination);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
