using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Examinations.Commands.UpdateExamination;

public class UpdateExaminationCommandHandler(
    IRepository<Examination> examinationRepository,
    IRepository<ExamResult> examResultRepository,
    IRepository<Batch> batchRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateExaminationCommand, Unit>
{
    public async Task<Unit> Handle(UpdateExaminationCommand request, CancellationToken cancellationToken)
    {
        var examination = await examinationRepository.GetByIdAsync(request.ExaminationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Examination), request.ExaminationId);

        var hasResults = await examResultRepository.Query()
            .AnyAsync(r => r.ExamId == examination.Id, cancellationToken);

        if (hasResults)
            throw new ConflictException("Cannot update an examination that already has results recorded.");

        var batch = await batchRepository.GetByIdAsync(examination.BatchId, cancellationToken)
            ?? throw new NotFoundException(nameof(Batch), examination.BatchId);

        if (request.ExamDate < batch.StartDate || request.ExamDate > batch.EndDate)
            throw new ConflictException(
                $"Exam date must fall within the batch period ({batch.StartDate:yyyy-MM-dd} to {batch.EndDate:yyyy-MM-dd}).");

        var trimmedName = request.ExamName.Trim();
        var duplicate = await examinationRepository.Query()
            .AnyAsync(e => e.BatchId == batch.Id
                && e.Id != examination.Id
                && e.ExamName == trimmedName
                && e.ExamDate == request.ExamDate, cancellationToken);

        if (duplicate)
            throw new ConflictException("An examination with this name already exists on the same date for this batch.");

        examination.ExamName = trimmedName;
        examination.ExamDate = request.ExamDate;
        examination.Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim();
        examination.MaxScore = request.MaxScore;
        examination.PassScore = request.PassScore;

        examinationRepository.Update(examination);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
