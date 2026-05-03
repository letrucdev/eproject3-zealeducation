using FluentValidation;

namespace ZealEducation.Application.Features.Payments.Queries.GetFinancialReport;

public class GetFinancialReportQueryValidator : AbstractValidator<GetFinancialReportQuery>
{
    public GetFinancialReportQueryValidator()
    {
        RuleFor(x => x.From)
            .LessThanOrEqualTo(x => x.To)
            .WithMessage("'From' must be earlier than or equal to 'To'.");

        RuleFor(x => x)
            .Must(x => (x.To - x.From).TotalDays <= 366)
            .WithMessage("Date range must not exceed 366 days.");
    }
}
