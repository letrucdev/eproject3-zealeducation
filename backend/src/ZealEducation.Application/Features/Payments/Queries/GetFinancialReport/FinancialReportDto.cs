using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Payments.Queries.GetFinancialReport;

public class FinancialReportDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }

    public decimal MonthlyProfit { get; set; }
    public decimal YearlyIncome { get; set; }
    public decimal Outstanding { get; set; }
    public int TransactionCount { get; set; }

    public List<RevenueTrendPointDto> RevenueTrend { get; set; } = [];
    public List<PaymentStatusBucketDto> PaymentStatusDistribution { get; set; } = [];
    public List<FeeTypeRevenueDto> RevenueByFeeType { get; set; } = [];
    public List<CourseRevenueDto> TopCoursesByRevenue { get; set; } = [];
}

public class RevenueTrendPointDto
{
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    public int Count { get; set; }
}

public class PaymentStatusBucketDto
{
    public PaymentStatus Status { get; set; }
    public int Count { get; set; }
    public decimal OutstandingAmount { get; set; }
}

public class FeeTypeRevenueDto
{
    public FeeType FeeType { get; set; }
    public decimal Amount { get; set; }
    public int Count { get; set; }
}

public class CourseRevenueDto
{
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = default!;
    public decimal Amount { get; set; }
    public int TransactionCount { get; set; }
}
