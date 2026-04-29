using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.CandidatePortal.Common;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Feedback.Commands.SubmitCourseFeedback;

public class SubmitCourseFeedbackCommandHandler(
    ICurrentUser currentUser,
    IRepository<Candidate> candidateRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Domain.Entities.Feedback> feedbackRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<SubmitCourseFeedbackCommand, Guid>
{
    public async Task<Guid> Handle(SubmitCourseFeedbackCommand request, CancellationToken cancellationToken)
    {
        var candidate = await CandidateResolver.ResolveAsync(currentUser, candidateRepository, cancellationToken);

        var enrolled = await enrollmentRepository.Query()
            .AnyAsync(e => e.CandidateId == candidate.Id && e.BatchId == request.BatchId, cancellationToken);
        if (!enrolled)
            throw new NotFoundException("You are not enrolled in this batch.");

        var duplicate = await feedbackRepository.Query()
            .AnyAsync(f =>
                f.CandidateId == candidate.Id &&
                f.BatchId == request.BatchId &&
                f.Type == FeedbackType.Course, cancellationToken);
        if (duplicate)
            throw new ConflictException("You have already submitted course feedback for this batch.");

        var feedback = new Domain.Entities.Feedback
        {
            Id = Guid.NewGuid(),
            CandidateId = candidate.Id,
            BatchId = request.BatchId,
            Type = FeedbackType.Course,
            Rating = request.Rating,
            Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim()
        };

        await feedbackRepository.AddAsync(feedback, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return feedback.Id;
    }
}
