using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Candidates.Queries.GetCandidateRegistrationsTrend;

public class GetCandidateRegistrationsTrendQueryHandler(
    IRepository<Candidate> candidateRepository) : IRequestHandler<GetCandidateRegistrationsTrendQuery, List<CandidateRegistrationTrendPointDto>>
{
    public async Task<List<CandidateRegistrationTrendPointDto>> Handle(
        GetCandidateRegistrationsTrendQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var fromDate = today.AddDays(-(request.Days - 1));
        var fromDateTime = fromDate.ToDateTime(TimeOnly.MinValue);
        var toDateTimeExclusive = today.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var rows = await candidateRepository.Query()
            .Where(c => c.RegisteredAt >= fromDateTime && c.RegisteredAt < toDateTimeExclusive)
            .Select(c => new { c.RegisteredAt, c.Status })
            .ToListAsync(cancellationToken);

        var groupedByDay = rows
            .GroupBy(r => DateOnly.FromDateTime(r.RegisteredAt))
            .ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<CandidateRegistrationTrendPointDto>(request.Days);
        for (var i = 0; i < request.Days; i++)
        {
            var day = fromDate.AddDays(i);
            var bucket = groupedByDay.GetValueOrDefault(day);

            if (bucket is null)
            {
                result.Add(new CandidateRegistrationTrendPointDto { Date = day });
                continue;
            }

            result.Add(new CandidateRegistrationTrendPointDto
            {
                Date = day,
                Count = bucket.Count,
                Graduated = bucket.Count(r => r.Status == CandidateStatus.Graduated),
                Dropped = bucket.Count(r => r.Status == CandidateStatus.Dropped)
            });
        }

        return result;
    }
}
