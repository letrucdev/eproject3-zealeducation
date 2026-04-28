using MediatR;

namespace ZealEducation.Application.Features.Batches.Commands.AssignCandidatesToBatch;

public record AssignCandidatesToBatchCommand(
    Guid BatchId,
    List<Guid> EnrollmentIds) : IRequest<Unit>;
