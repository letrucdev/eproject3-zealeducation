using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.Batches.Notifications;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Batches.Commands.AssignCandidatesToBatch;

public class AssignCandidatesToBatchCommandHandler(
    IRepository<Batch> batchRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Candidate> candidateRepository,
    IRepository<Course> courseRepository,
    IRepository<ClassSession> classSessionRepository,
    IBatchCandidateEnrolledNotificationService notificationService,
    IUnitOfWork unitOfWork) : IRequestHandler<AssignCandidatesToBatchCommand, Unit>
{
    public async Task<Unit> Handle(AssignCandidatesToBatchCommand request, CancellationToken cancellationToken)
    {
        var batch = await batchRepository.GetByIdAsync(request.BatchId, cancellationToken)
            ?? throw new NotFoundException(nameof(Batch), request.BatchId);

        if (batch.Status is BatchStatus.Completed or BatchStatus.Cancelled)
            throw new ConflictException("Cannot assign candidates to a completed or cancelled batch.");

        var hasSchedule = await classSessionRepository.Query()
            .AnyAsync(s => s.BatchId == batch.Id, cancellationToken);
        if (!hasSchedule)
            throw new ConflictException("This batch has no class schedule yet. Add at least one class session before enrolling candidates.");

        var enrollmentIds = request.EnrollmentIds.Distinct().ToList();

        var currentEnrolledCount = await enrollmentRepository.Query()
            .CountAsync(e => e.BatchId == batch.Id, cancellationToken);

        if (currentEnrolledCount + enrollmentIds.Count > batch.MaxCapacity)
            throw new ConflictException(
                $"Adding {enrollmentIds.Count} candidate(s) would exceed batch capacity ({batch.MaxCapacity}). Currently enrolled: {currentEnrolledCount}.");

        var enrollments = await enrollmentRepository.Query()
            .Where(e => enrollmentIds.Contains(e.Id))
            .ToListAsync(cancellationToken);

        if (enrollments.Count != enrollmentIds.Count)
            throw new NotFoundException("One or more enrollments were not found.");

        foreach (var enrollment in enrollments)
        {
            if (enrollment.CourseId != batch.CourseId)
                throw new ConflictException(
                    $"Enrollment {enrollment.Id} belongs to a different course and cannot be assigned to this batch.");

            if (enrollment.BatchId != null)
                throw new ConflictException(
                    $"Enrollment {enrollment.Id} is already assigned to a batch.");

            if (enrollment.Status != EnrollmentStatus.PendingAssignment)
                throw new ConflictException(
                    $"Enrollment {enrollment.Id} is not in pending assignment state.");

            enrollment.BatchId = batch.Id;
            enrollment.Status = EnrollmentStatus.Enrolled;
            enrollmentRepository.Update(enrollment);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (enrollments.Count > 0)
        {
            await QueueCandidateEnrolledEmailsAsync(batch, enrollments, cancellationToken);
        }

        return Unit.Value;
    }

    private async Task QueueCandidateEnrolledEmailsAsync(
        Batch batch,
        IReadOnlyCollection<Enrollment> enrollments,
        CancellationToken cancellationToken)
    {
        var candidateIds = enrollments.Select(e => e.CandidateId).Distinct().ToList();

        var candidateInfos = await candidateRepository.Query()
            .Where(c => candidateIds.Contains(c.Id))
            .Select(c => new
            {
                c.Id,
                c.CandidateCode,
                Email = c.UserAccount.Email,
                FullName = c.UserAccount.FullName,
            })
            .ToListAsync(cancellationToken);

        if (candidateInfos.Count == 0) return;

        var courseName = await courseRepository.Query()
            .Where(c => c.Id == batch.CourseId)
            .Select(c => c.CourseName)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        var sessions = await classSessionRepository.Query()
            .Where(s => s.BatchId == batch.Id)
            .OrderBy(s => s.SessionDate).ThenBy(s => s.StartTime)
            .Select(s => new BatchScheduleSession
            {
                SessionDate = s.SessionDate,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Topic = s.Topic,
                Location = s.Location,
            })
            .ToListAsync(cancellationToken);

        foreach (var info in candidateInfos)
        {
            if (string.IsNullOrWhiteSpace(info.Email)) continue;

            await notificationService.QueueAsync(new BatchCandidateEnrolledEmailModel
            {
                RecipientEmail = info.Email,
                RecipientName = info.FullName,
                CandidateCode = info.CandidateCode,
                BatchCode = batch.BatchCode,
                CourseName = courseName,
                BatchStartDate = batch.StartDate,
                BatchEndDate = batch.EndDate,
                BatchLocation = batch.Location,
                Sessions = sessions,
            }, cancellationToken);
        }
    }
}
