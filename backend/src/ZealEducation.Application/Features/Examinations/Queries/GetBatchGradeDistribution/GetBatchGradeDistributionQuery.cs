using MediatR;

namespace ZealEducation.Application.Features.Examinations.Queries.GetBatchGradeDistribution;

public record GetBatchGradeDistributionQuery(Guid BatchId) : IRequest<BatchGradeDistributionDto>;
