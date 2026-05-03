using FluentValidation.TestHelper;
using ZealEducation.Application.Features.ExamResults.Commands.CreateExamResult;

namespace ZealEducation.Application.UnitTests.Validators.ExamResults;

public class CreateExamResultCommandValidatorTests
{
    private readonly CreateExamResultCommandValidator _validator = new();

    private static CreateExamResultCommand Valid(
        Guid? examinationId = null,
        Guid? enrollmentId = null,
        decimal score = 75m,
        bool isFinalized = false) => new(
            examinationId ?? Guid.NewGuid(),
            enrollmentId ?? Guid.NewGuid(),
            score,
            isFinalized);

    [Fact]
    public void Should_pass_for_a_fully_valid_command()
    {
        var result = _validator.TestValidate(Valid());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_fail_when_examination_id_is_empty()
    {
        var result = _validator.TestValidate(Valid(examinationId: Guid.Empty));
        result.ShouldHaveValidationErrorFor(c => c.ExaminationId);
    }

    [Fact]
    public void Should_fail_when_enrollment_id_is_empty()
    {
        var result = _validator.TestValidate(Valid(enrollmentId: Guid.Empty));
        result.ShouldHaveValidationErrorFor(c => c.EnrollmentId);
    }

    [Fact]
    public void Should_fail_when_score_is_negative()
    {
        var result = _validator.TestValidate(Valid(score: -0.01m));
        result.ShouldHaveValidationErrorFor(c => c.Score);
    }

    [Fact]
    public void Should_pass_when_score_is_zero()
    {
        var result = _validator.TestValidate(Valid(score: 0m));
        result.ShouldNotHaveValidationErrorFor(c => c.Score);
    }

    [Fact]
    public void Should_pass_when_score_is_a_large_positive_value()
    {
        var result = _validator.TestValidate(Valid(score: 1000m));
        result.ShouldNotHaveValidationErrorFor(c => c.Score);
    }

    [Fact]
    public void Should_pass_when_is_finalized_is_true()
    {
        var result = _validator.TestValidate(Valid(isFinalized: true));
        result.ShouldNotHaveAnyValidationErrors();
    }
}
