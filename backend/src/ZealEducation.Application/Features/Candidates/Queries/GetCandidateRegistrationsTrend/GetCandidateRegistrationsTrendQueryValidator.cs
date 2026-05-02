using FluentValidation;

namespace ZealEducation.Application.Features.Candidates.Queries.GetCandidateRegistrationsTrend;

public class GetCandidateRegistrationsTrendQueryValidator : AbstractValidator<GetCandidateRegistrationsTrendQuery>
{
    private static readonly int[] AllowedDays = [7, 30, 90];

    public GetCandidateRegistrationsTrendQueryValidator()
    {
        RuleFor(x => x.Days)
            .Must(d => AllowedDays.Contains(d))
            .WithMessage("Days must be one of: 7, 30, 90.");
    }
}
