using FluentValidation.TestHelper;
using ZealEducation.Application.Features.Candidates.Queries.GetCandidateRegistrationsTrend;

namespace ZealEducation.Application.UnitTests.Validators.Candidates;

public class GetCandidateRegistrationsTrendQueryValidatorTests
{
    private readonly GetCandidateRegistrationsTrendQueryValidator _validator = new();

    private static GetCandidateRegistrationsTrendQuery Valid(int days = 7) => new(days);

    [Theory]
    [InlineData(7)]
    [InlineData(30)]
    [InlineData(90)]
    public void Should_pass_when_days_is_one_of_allowed_values(int days)
    {
        var result = _validator.TestValidate(Valid(days));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(8)]
    [InlineData(29)]
    [InlineData(31)]
    [InlineData(89)]
    [InlineData(91)]
    [InlineData(365)]
    [InlineData(-7)]
    public void Should_fail_when_days_is_not_an_allowed_value(int days)
    {
        var result = _validator.TestValidate(Valid(days));
        result.ShouldHaveValidationErrorFor(c => c.Days);
    }
}
