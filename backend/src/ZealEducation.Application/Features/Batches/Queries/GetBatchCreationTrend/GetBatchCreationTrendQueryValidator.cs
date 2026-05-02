using FluentValidation;

namespace ZealEducation.Application.Features.Batches.Queries.GetBatchCreationTrend;

public class GetBatchCreationTrendQueryValidator : AbstractValidator<GetBatchCreationTrendQuery>
{
    private static readonly int[] AllowedDays = [7, 30, 90];

    public GetBatchCreationTrendQueryValidator()
    {
        RuleFor(x => x.Days)
            .Must(d => AllowedDays.Contains(d))
            .WithMessage("Days must be one of: 7, 30, 90.");
    }
}
