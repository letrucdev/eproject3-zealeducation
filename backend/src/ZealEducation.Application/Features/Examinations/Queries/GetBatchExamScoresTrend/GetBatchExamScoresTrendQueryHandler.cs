using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Examinations.Queries.GetBatchExamScoresTrend;

public class GetBatchExamScoresTrendQueryHandler(
    IRepository<Batch> batchRepository,
    IRepository<Examination> examinationRepository,
    IRepository<ExamResult> examResultRepository) : IRequestHandler<GetBatchExamScoresTrendQuery, List<BatchExamScoresTrendPointDto>>
{
    public async Task<List<BatchExamScoresTrendPointDto>> Handle(
        GetBatchExamScoresTrendQuery request,
        CancellationToken cancellationToken)
    {
        var batchExists = await batchRepository.ExistsAsync(request.BatchId, cancellationToken);
        if (!batchExists)
            throw new NotFoundException(nameof(Batch), request.BatchId);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var fromDate = today.AddDays(-(request.Days - 1));

        var rows = await (from result in examResultRepository.Query()
                          join exam in examinationRepository.Query() on result.ExamId equals exam.Id
                          where exam.BatchId == request.BatchId
                                && exam.ExamDate >= fromDate
                                && exam.ExamDate <= today
                          select new { exam.ExamDate, result.Score })
            .ToListAsync(cancellationToken);

        return [.. rows
            .GroupBy(r => r.ExamDate)
            .OrderBy(g => g.Key)
            .Select(g => new BatchExamScoresTrendPointDto
            {
                Date = g.Key,
                AverageScore = Math.Round(g.Average(r => r.Score), 2),
                HighestScore = g.Max(r => r.Score),
                LowestScore = g.Min(r => r.Score),
                Count = g.Count()
            })];
    }
}
