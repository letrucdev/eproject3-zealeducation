using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.Batches.Notifications;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Batches.Commands.AssignFaculty;

public class AssignFacultyCommandHandler(
    IRepository<Batch> batchRepository,
    IRepository<Faculty> facultyRepository,
    IRepository<Course> courseRepository,
    IRepository<ClassSession> classSessionRepository,
    IBatchFacultyAssignedNotificationService notificationService,
    IUnitOfWork unitOfWork) : IRequestHandler<AssignFacultyCommand, Unit>
{
    public async Task<Unit> Handle(AssignFacultyCommand request, CancellationToken cancellationToken)
    {
        var batch = await batchRepository.GetByIdAsync(request.BatchId, cancellationToken)
            ?? throw new NotFoundException(nameof(Batch), request.BatchId);

        if (batch.Status is BatchStatus.Completed or BatchStatus.Cancelled)
            throw new ConflictException("Cannot assign faculty to a completed or cancelled batch.");

        if (request.FacultyId.HasValue)
        {
            var hasSchedule = await classSessionRepository.Query()
                .AnyAsync(s => s.BatchId == batch.Id, cancellationToken);
            if (!hasSchedule)
                throw new ConflictException("This batch has no class schedule yet. Add at least one class session before assigning faculty.");

            var facultyExists = await facultyRepository.ExistsAsync(request.FacultyId.Value, cancellationToken);
            if (!facultyExists)
                throw new NotFoundException(nameof(Faculty), request.FacultyId.Value);

            var conflicts = await batchRepository.FindAsync(
                b => b.Id != batch.Id
                    && b.FacultyId == request.FacultyId.Value
                    && b.Status != BatchStatus.Completed
                    && b.Status != BatchStatus.Cancelled
                    && b.StartDate <= batch.EndDate
                    && batch.StartDate <= b.EndDate,
                cancellationToken);

            if (conflicts.Count > 0)
                throw new ConflictException("Faculty already has another batch scheduled within this date range.");
        }

        var previousFacultyId = batch.FacultyId;
        batch.FacultyId = request.FacultyId;

        if (request.FacultyId.HasValue)
        {
            if (batch.Status == BatchStatus.NeedsInstructor)
                batch.Status = BatchStatus.Active;
        }
        else
        {
            if (batch.Status == BatchStatus.Active)
                batch.Status = BatchStatus.NeedsInstructor;
        }

        batchRepository.Update(batch);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.FacultyId.HasValue && previousFacultyId != request.FacultyId)
        {
            await QueueFacultyAssignedEmailAsync(batch, request.FacultyId.Value, cancellationToken);
        }

        return Unit.Value;
    }

    private async Task QueueFacultyAssignedEmailAsync(Batch batch, Guid facultyId, CancellationToken cancellationToken)
    {
        var facultyInfo = await facultyRepository.Query()
            .Where(f => f.Id == facultyId)
            .Select(f => new
            {
                f.FacultyCode,
                Email = f.Staff.UserAccount.Email,
                FullName = f.Staff.UserAccount.FullName,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (facultyInfo is null || string.IsNullOrWhiteSpace(facultyInfo.Email))
            return;

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

        await notificationService.QueueAsync(new BatchFacultyAssignedEmailModel
        {
            RecipientEmail = facultyInfo.Email,
            RecipientName = facultyInfo.FullName,
            FacultyCode = facultyInfo.FacultyCode,
            BatchCode = batch.BatchCode,
            CourseName = courseName,
            BatchStartDate = batch.StartDate,
            BatchEndDate = batch.EndDate,
            BatchLocation = batch.Location,
            Sessions = sessions,
        }, cancellationToken);
    }
}
