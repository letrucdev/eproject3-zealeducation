using MediatR;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Payments.Queries.ExportFinancialReport;

public record DateRangeFilter(DateTime From, DateTime To);

public record ExportFinancialReportQuery(
    DateRangeFilter RevenueTrend,
    DateRangeFilter PaymentStatus,
    DateRangeFilter RevenueByFeeType,
    DateRangeFilter TopCourses,
    DateRangeFilter Transactions,
    string? Search = null,
    FeeType? FeeType = null,
    PaymentMethod? Method = null) : IRequest<ExportFinancialReportResultDto>;

public class ExportFinancialReportResultDto
{
    public byte[] Content { get; set; } = [];
    public string FileName { get; set; } = default!;
    public string ContentType { get; set; } = default!;
}
