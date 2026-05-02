using FluentValidation;

namespace ZealEducation.Application.Features.Examinations.Queries.GetBatchExamScoresTrend;

public class GetBatchExamScoresTrendQueryValidator : AbstractValidator<GetBatchExamScoresTrendQuery>
{
    private static readonly int[] AllowedDays = [7, 30, 90];

    public GetBatchExamScoresTrendQueryValidator()
    {
        RuleFor(x => x.BatchId).NotEmpty();
        RuleFor(x => x.Days)
            .Must(d => AllowedDays.Contains(d))
            .WithMessage("Days must be one of: 7, 30, 90.");
    }
}
