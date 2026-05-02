using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Batches.Queries.GetBatchCreationTrend;

public class GetBatchCreationTrendQueryHandler(
    IRepository<Batch> batchRepository) : IRequestHandler<GetBatchCreationTrendQuery, List<BatchCreationTrendPointDto>>
{
    public async Task<List<BatchCreationTrendPointDto>> Handle(
        GetBatchCreationTrendQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var fromDate = today.AddDays(-(request.Days - 1));
        var fromDateTime = fromDate.ToDateTime(TimeOnly.MinValue);
        var toDateTimeExclusive = today.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var rows = await batchRepository.Query()
            .Where(b => b.CreatedAt >= fromDateTime && b.CreatedAt < toDateTimeExclusive)
            .Select(b => new { b.CreatedAt, b.Status })
            .ToListAsync(cancellationToken);

        var groupedByDay = rows
            .GroupBy(r => DateOnly.FromDateTime(r.CreatedAt))
            .ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<BatchCreationTrendPointDto>(request.Days);
        for (var i = 0; i < request.Days; i++)
        {
            var day = fromDate.AddDays(i);
            var bucket = groupedByDay.GetValueOrDefault(day);

            if (bucket is null)
            {
                result.Add(new BatchCreationTrendPointDto { Date = day });
                continue;
            }

            result.Add(new BatchCreationTrendPointDto
            {
                Date = day,
                Total = bucket.Count,
                Active = bucket.Count(r => r.Status == BatchStatus.Active),
                Completed = bucket.Count(r => r.Status == BatchStatus.Completed),
                Cancelled = bucket.Count(r => r.Status == BatchStatus.Cancelled)
            });
        }

        return result;
    }
}
