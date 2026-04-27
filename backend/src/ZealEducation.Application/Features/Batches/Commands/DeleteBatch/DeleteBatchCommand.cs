using MediatR;

namespace ZealEducation.Application.Features.Batches.Commands.DeleteBatch;

public record DeleteBatchCommand(Guid BatchId) : IRequest<Unit>;
