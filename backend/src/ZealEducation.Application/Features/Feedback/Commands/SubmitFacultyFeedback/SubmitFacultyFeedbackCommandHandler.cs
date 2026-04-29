using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.CandidatePortal.Common;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Feedback.Commands.SubmitFacultyFeedback;

public class SubmitFacultyFeedbackCommandHandler(
    ICurrentUser currentUser,
    IRepository<Candidate> candidateRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Batch> batchRepository,
    IRepository<Domain.Entities.Feedback> feedbackRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<SubmitFacultyFeedbackCommand, Guid>
{
    public async Task<Guid> Handle(SubmitFacultyFeedbackCommand request, CancellationToken cancellationToken)
    {
        var candidate = await CandidateResolver.ResolveAsync(currentUser, candidateRepository, cancellationToken);

        var enrolled = await enrollmentRepository.Query()
            .AnyAsync(e => e.CandidateId == candidate.Id && e.BatchId == request.BatchId, cancellationToken);
        if (!enrolled)
            throw new NotFoundException("You are not enrolled in this batch.");

        var batch = await batchRepository.Query()
            .Where(b => b.Id == request.BatchId)
            .Select(b => new { b.FacultyId })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Batch), request.BatchId);

        if (batch.FacultyId != request.FacultyId)
            throw new ConflictException("This faculty does not teach the selected batch.");

        var duplicate = await feedbackRepository.Query()
            .AnyAsync(f =>
                f.CandidateId == candidate.Id &&
                f.BatchId == request.BatchId &&
                f.Type == FeedbackType.Faculty &&
                f.TargetFacultyId == request.FacultyId, cancellationToken);
        if (duplicate)
            throw new ConflictException("You have already submitted feedback for this faculty.");

        var feedback = new Domain.Entities.Feedback
        {
            Id = Guid.NewGuid(),
            CandidateId = candidate.Id,
            BatchId = request.BatchId,
            Type = FeedbackType.Faculty,
            TargetFacultyId = request.FacultyId,
            Rating = request.Rating,
            Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim()
        };

        await feedbackRepository.AddAsync(feedback, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return feedback.Id;
    }
}
