using MediatR;

namespace ZealEducation.Application.Features.Examinations.Queries.GetBatchExamScoresTrend;

public record GetBatchExamScoresTrendQuery(Guid BatchId, int Days)
    : IRequest<List<BatchExamScoresTrendPointDto>>;
