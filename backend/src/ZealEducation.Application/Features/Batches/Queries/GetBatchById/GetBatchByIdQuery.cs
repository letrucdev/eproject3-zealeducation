using MediatR;

namespace ZealEducation.Application.Features.Batches.Queries.GetBatchById;

public record GetBatchByIdQuery(Guid BatchId) : IRequest<BatchDetailDto>;
