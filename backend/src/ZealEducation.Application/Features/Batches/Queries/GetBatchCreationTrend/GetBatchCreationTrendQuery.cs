using MediatR;

namespace ZealEducation.Application.Features.Batches.Queries.GetBatchCreationTrend;

public record GetBatchCreationTrendQuery(int Days) : IRequest<List<BatchCreationTrendPointDto>>;
