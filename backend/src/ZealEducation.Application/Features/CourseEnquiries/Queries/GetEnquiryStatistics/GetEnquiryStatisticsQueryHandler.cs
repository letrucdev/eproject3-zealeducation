using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.CourseEnquiries.Queries.GetEnquiryStatistics;

public class GetEnquiryStatisticsQueryHandler(
    IRepository<CourseEnquiry> enquiryRepository) : IRequestHandler<GetEnquiryStatisticsQuery, EnquiryStatisticsDto>
{
    private static readonly EnquiryStatus[] OpenStatuses =
    [
        EnquiryStatus.New,
        EnquiryStatus.Contacted,
        EnquiryStatus.InFollowUp,
        EnquiryStatus.Interested
    ];

    public async Task<EnquiryStatisticsDto> Handle(GetEnquiryStatisticsQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var rows = await enquiryRepository.Query()
            .Select(e => new { e.Status, e.NextFollowUpDate })
            .ToListAsync(cancellationToken);

        return new EnquiryStatisticsDto
        {
            Total = rows.Count,
            New = rows.Count(x => x.Status == EnquiryStatus.New),
            InFollowUp = rows.Count(x => x.Status == EnquiryStatus.InFollowUp),
            Converted = rows.Count(x => x.Status == EnquiryStatus.Converted),
            Overdue = rows.Count(x =>
                x.NextFollowUpDate.HasValue &&
                x.NextFollowUpDate.Value <= today &&
                OpenStatuses.Contains(x.Status))
        };
    }
}
