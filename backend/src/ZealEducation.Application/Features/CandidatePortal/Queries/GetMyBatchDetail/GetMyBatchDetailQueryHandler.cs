using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.CandidatePortal.Common;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyBatchDetail;

public class GetMyBatchDetailQueryHandler(
    ICurrentUser currentUser,
    IRepository<Candidate> candidateRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Batch> batchRepository,
    IRepository<Domain.Entities.Feedback> feedbackRepository) : IRequestHandler<GetMyBatchDetailQuery, MyBatchDetailDto>
{
    public async Task<MyBatchDetailDto> Handle(GetMyBatchDetailQuery request, CancellationToken cancellationToken)
    {
        var candidate = await CandidateResolver.ResolveAsync(currentUser, candidateRepository, cancellationToken);

        var enrollment = await enrollmentRepository.Query()
            .Where(e => e.CandidateId == candidate.Id && e.BatchId == request.BatchId)
            .Select(e => new { e.Id, e.Status })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("You are not enrolled in this batch.");

        var detail = await batchRepository.Query()
            .Where(b => b.Id == request.BatchId)
            .Select(b => new MyBatchDetailDto
            {
                BatchId = b.Id,
                BatchCode = b.BatchCode,
                CourseId = b.CourseId,
                CourseName = b.Course.CourseName,
                CourseDurationWeeks = b.Course.DurationWeeks,
                FacultyId = b.FacultyId,
                FacultyName = b.Faculty != null ? b.Faculty.Staff.UserAccount.FullName : null,
                FacultyCode = b.Faculty != null ? b.Faculty.FacultyCode : null,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                Location = b.Location,
                MaxCapacity = b.MaxCapacity,
                EnrolledCount = b.Enrollments.Count,
                Status = b.Status
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Batch), request.BatchId);

        detail.EnrollmentId = enrollment.Id;
        detail.EnrollmentStatus = enrollment.Status;

        var submitted = await feedbackRepository.Query()
            .Where(f => f.CandidateId == candidate.Id && f.BatchId == request.BatchId)
            .Select(f => new { f.Type, f.TargetFacultyId })
            .ToListAsync(cancellationToken);

        detail.FeedbackState = new MyBatchFeedbackStateDto
        {
            CourseSubmitted = submitted.Any(s => s.Type == FeedbackType.Course),
            GeneralSubmitted = submitted.Any(s => s.Type == FeedbackType.General),
            FacultyTargetsSubmitted = [.. submitted
                .Where(s => s.Type == FeedbackType.Faculty && s.TargetFacultyId.HasValue)
                .Select(s => s.TargetFacultyId!.Value)]
        };

        return detail;
    }
}
