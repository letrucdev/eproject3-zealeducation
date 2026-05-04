using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.Examinations.Notifications;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Examinations.Commands.CreateExamination;

public class CreateExaminationCommandHandler(
    IRepository<Batch> batchRepository,
    IRepository<Examination> examinationRepository,
    IRepository<Staff> staffRepository,
    IRepository<Faculty> facultyRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Course> courseRepository,
    IRepository<ClassSession> classSessionRepository,
    IExaminationCreatedNotificationService notificationService,
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

        var hasSchedule = await classSessionRepository.Query()
            .AnyAsync(s => s.BatchId == batch.Id, cancellationToken);
        if (!hasSchedule)
            throw new ConflictException("This batch has no class schedule yet. Add at least one class session before creating an exam.");

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

        await QueueExaminationCreatedEmailsAsync(batch, examination, cancellationToken);

        return examination.Id;
    }

    private async Task QueueExaminationCreatedEmailsAsync(Batch batch, Examination examination, CancellationToken cancellationToken)
    {
        var courseName = await courseRepository.Query()
            .Where(c => c.Id == batch.CourseId)
            .Select(c => c.CourseName)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        var recipients = new List<(string Email, string FullName)>();

        if (batch.FacultyId.HasValue)
        {
            var facultyInfo = await facultyRepository.Query()
                .Where(f => f.Id == batch.FacultyId.Value)
                .Select(f => new
                {
                    Email = f.Staff.UserAccount.Email,
                    FullName = f.Staff.UserAccount.FullName,
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (facultyInfo is not null && !string.IsNullOrWhiteSpace(facultyInfo.Email))
            {
                recipients.Add((facultyInfo.Email, facultyInfo.FullName));
            }
        }

        var candidateRecipients = await enrollmentRepository.Query()
            .Where(e => e.BatchId == batch.Id && e.Status == EnrollmentStatus.Enrolled)
            .Select(e => new
            {
                Email = e.Candidate.UserAccount.Email,
                FullName = e.Candidate.UserAccount.FullName,
            })
            .ToListAsync(cancellationToken);

        foreach (var c in candidateRecipients)
        {
            if (string.IsNullOrWhiteSpace(c.Email)) continue;
            recipients.Add((c.Email, c.FullName));
        }

        foreach (var recipient in recipients)
        {
            await notificationService.QueueAsync(new ExaminationCreatedEmailModel
            {
                RecipientEmail = recipient.Email,
                RecipientName = recipient.FullName,
                BatchCode = batch.BatchCode,
                CourseName = courseName,
                ExamName = examination.ExamName,
                ExamDate = examination.ExamDate,
                Location = examination.Location,
                MaxScore = examination.MaxScore,
                PassScore = examination.PassScore,
            }, cancellationToken);
        }
    }
}
