using MediatR;

namespace ZealEducation.Application.Features.Batches.Queries.GetBatchStatistics;

public record GetBatchStatisticsQuery() : IRequest<BatchStatisticsDto>;
