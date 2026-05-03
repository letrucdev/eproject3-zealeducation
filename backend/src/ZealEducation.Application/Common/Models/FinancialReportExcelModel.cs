using ZealEducation.Application.Features.Payments.Queries.GetFinancialReport;
using ZealEducation.Application.Features.Payments.Queries.GetFinancialTransactions;

namespace ZealEducation.Application.Common.Models;

public record ExcelDateRange(DateTime From, DateTime To);

public class FinancialReportExcelModel
{
    public DateTime GeneratedAt { get; set; }
    public string GeneratedByStaffName { get; set; } = default!;

    public FinancialReportDto StatsSummary { get; set; } = default!;

    public ExcelDateRange RevenueTrendRange { get; set; } = default!;
    public IReadOnlyList<RevenueTrendPointDto> RevenueTrend { get; set; } = [];

    public ExcelDateRange PaymentStatusRange { get; set; } = default!;
    public IReadOnlyList<PaymentStatusBucketDto> PaymentStatusDistribution { get; set; } = [];

    public ExcelDateRange RevenueByFeeTypeRange { get; set; } = default!;
    public IReadOnlyList<FeeTypeRevenueDto> RevenueByFeeType { get; set; } = [];

    public ExcelDateRange TopCoursesRange { get; set; } = default!;
    public IReadOnlyList<CourseRevenueDto> TopCoursesByRevenue { get; set; } = [];

    public ExcelDateRange TransactionsRange { get; set; } = default!;
    public IReadOnlyList<FinancialTransactionListItemDto> Transactions { get; set; } = [];
}
