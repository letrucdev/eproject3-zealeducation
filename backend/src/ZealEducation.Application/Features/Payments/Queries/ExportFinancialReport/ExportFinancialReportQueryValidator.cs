using FluentValidation;

namespace ZealEducation.Application.Features.Payments.Queries.ExportFinancialReport;

public class ExportFinancialReportQueryValidator : AbstractValidator<ExportFinancialReportQuery>
{
    public ExportFinancialReportQueryValidator()
    {
        RuleFor(x => x.RevenueTrend).SetValidator(new DateRangeFilterValidator("Revenue trend"));
        RuleFor(x => x.PaymentStatus).SetValidator(new DateRangeFilterValidator("Payment status"));
        RuleFor(x => x.RevenueByFeeType).SetValidator(new DateRangeFilterValidator("Revenue by fee type"));
        RuleFor(x => x.TopCourses).SetValidator(new DateRangeFilterValidator("Top courses"));
        RuleFor(x => x.Transactions).SetValidator(new DateRangeFilterValidator("Transactions"));
    }
}

internal class DateRangeFilterValidator : AbstractValidator<DateRangeFilter>
{
    public DateRangeFilterValidator(string sectionName)
    {
        RuleFor(x => x.From)
            .LessThanOrEqualTo(x => x.To)
            .WithMessage($"{sectionName} 'From' must be earlier than or equal to 'To'.");

        RuleFor(x => x)
            .Must(x => (x.To - x.From).TotalDays <= 366)
            .WithMessage($"{sectionName} date range must not exceed 366 days.");
    }
}
