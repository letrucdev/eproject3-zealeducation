using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Examinations.Commands.CreateExamination;

public class CreateExaminationCommandHandler(
    IRepository<Batch> batchRepository,
    IRepository<Examination> examinationRepository,
    IRepository<Staff> staffRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateExaminationCommand, Guid>
{
    public async Task<Guid> Handle(CreateExaminationCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
            throw new UnauthorizedException("Current user could not be resolved.");

        var batch = await batchRepository.GetByIdAsync(request.BatchId, cancellationToken)
            ?? throw new NotFoundException(nameof(Batch), request.BatchId);

        if (batch.Status is BatchStatus.Completed or BatchStatus.Cancelled)
            throw new ConflictException("Cannot add examinations to a completed or cancelled batch.");

        if (request.ExamDate < batch.StartDate || request.ExamDate > batch.EndDate)
            throw new ConflictException(
                $"Exam date must fall within the batch period ({batch.StartDate:yyyy-MM-dd} to {batch.EndDate:yyyy-MM-dd}).");

        var staff = await staffRepository.Query()
            .FirstOrDefaultAsync(s => s.UserAccountId == currentUser.UserId.Value, cancellationToken)
            ?? throw new ConflictException("Current user is not registered as staff.");

        var trimmedName = request.ExamName.Trim();
        var duplicate = await examinationRepository.Query()
            .AnyAsync(e => e.BatchId == batch.Id
                && e.ExamName == trimmedName
                && e.ExamDate == request.ExamDate, cancellationToken);

        if (duplicate)
            throw new ConflictException("An examination with this name already exists on the same date for this batch.");

        var examination = new Examination
        {
            Id = Guid.NewGuid(),
            BatchId = batch.Id,
            ExamName = trimmedName,
            ExamDate = request.ExamDate,
            Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim(),
            MaxScore = request.MaxScore,
            PassScore = request.PassScore,
            ScheduledById = staff.Id,
        };

        await examinationRepository.AddAsync(examination, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return examination.Id;
    }
}
