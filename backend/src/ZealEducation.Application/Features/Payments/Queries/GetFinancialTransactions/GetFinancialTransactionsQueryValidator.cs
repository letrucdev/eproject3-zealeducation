using FluentValidation;

namespace ZealEducation.Application.Features.Payments.Queries.GetFinancialTransactions;

public class GetFinancialTransactionsQueryValidator : AbstractValidator<GetFinancialTransactionsQuery>
{
    public GetFinancialTransactionsQueryValidator()
    {
        RuleFor(x => x.From)
            .LessThanOrEqualTo(x => x.To)
            .WithMessage("'From' must be earlier than or equal to 'To'.");

        RuleFor(x => x)
            .Must(x => (x.To - x.From).TotalDays <= 366)
            .WithMessage("Date range must not exceed 366 days.");

        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
