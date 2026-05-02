using FluentValidation;

namespace ZealEducation.Application.Features.Examinations.Queries.GetBatchGradeDistribution;

public class GetBatchGradeDistributionQueryValidator : AbstractValidator<GetBatchGradeDistributionQuery>
{
    public GetBatchGradeDistributionQueryValidator()
    {
        RuleFor(x => x.BatchId).NotEmpty();
    }
}
