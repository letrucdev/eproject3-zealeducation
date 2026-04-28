using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Batches.Queries.GetBatchStatistics;

public class GetBatchStatisticsQueryHandler(
    IRepository<Batch> batchRepository) : IRequestHandler<GetBatchStatisticsQuery, BatchStatisticsDto>
{
    public async Task<BatchStatisticsDto> Handle(GetBatchStatisticsQuery request, CancellationToken cancellationToken)
    {
        var statuses = await batchRepository.Query()
            .Select(b => b.Status)
            .ToListAsync(cancellationToken);

        return new BatchStatisticsDto
        {
            Total = statuses.Count,
            NeedsInstructor = statuses.Count(s => s == BatchStatus.NeedsInstructor),
            Active = statuses.Count(s => s == BatchStatus.Active),
            Completed = statuses.Count(s => s == BatchStatus.Completed),
            Cancelled = statuses.Count(s => s == BatchStatus.Cancelled)
        };
    }
}
