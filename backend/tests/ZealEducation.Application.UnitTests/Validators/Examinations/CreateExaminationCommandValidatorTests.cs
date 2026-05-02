using FluentValidation.TestHelper;
using ZealEducation.Application.Features.Examinations.Commands.CreateExamination;

namespace ZealEducation.Application.UnitTests.Validators.Examinations;

public class CreateExaminationCommandValidatorTests
{
    private readonly CreateExaminationCommandValidator _validator = new();

    private static CreateExaminationCommand Valid(
        Guid? batchId = null,
        string examName = "Midterm Exam",
        DateOnly? examDate = null,
        string? location = "Hall 1",
        int maxScore = 100,
        int passScore = 50) => new(
            batchId ?? Guid.NewGuid(),
            examName,
            examDate ?? new DateOnly(2030, 3, 15),
            location,
            maxScore,
            passScore);

    [Fact]
    public void Should_pass_for_a_fully_valid_command()
    {
        var result = _validator.TestValidate(Valid());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_pass_when_location_is_null()
    {
        var result = _validator.TestValidate(Valid(location: null));
        result.ShouldNotHaveValidationErrorFor(c => c.Location);
    }

    [Fact]
    public void Should_fail_when_batch_id_is_empty()
    {
        var result = _validator.TestValidate(Valid(batchId: Guid.Empty));
        result.ShouldHaveValidationErrorFor(c => c.BatchId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_fail_when_exam_name_is_empty_or_whitespace(string name)
    {
        var result = _validator.TestValidate(Valid(examName: name));
        result.ShouldHaveValidationErrorFor(c => c.ExamName);
    }

    [Fact]
    public void Should_fail_when_exam_name_exceeds_100_chars()
    {
        var result = _validator.TestValidate(Valid(examName: new string('A', 101)));
        result.ShouldHaveValidationErrorFor(c => c.ExamName);
    }

    [Fact]
    public void Should_pass_when_exam_name_is_exactly_100_chars()
    {
        var result = _validator.TestValidate(Valid(examName: new string('A', 100)));
        result.ShouldNotHaveValidationErrorFor(c => c.ExamName);
    }

    [Fact]
    public void Should_fail_when_location_exceeds_100_chars()
    {
        var result = _validator.TestValidate(Valid(location: new string('x', 101)));
        result.ShouldHaveValidationErrorFor(c => c.Location);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Should_fail_when_max_score_is_not_greater_than_zero(int maxScore)
    {
        var result = _validator.TestValidate(Valid(maxScore: maxScore, passScore: 0));
        result.ShouldHaveValidationErrorFor(c => c.MaxScore)
            .WithErrorMessage("Max score must be greater than 0.");
    }

    [Fact]
    public void Should_fail_when_pass_score_is_negative()
    {
        var result = _validator.TestValidate(Valid(passScore: -1));
        result.ShouldHaveValidationErrorFor(c => c.PassScore);
    }

    [Fact]
    public void Should_pass_when_pass_score_is_zero()
    {
        var result = _validator.TestValidate(Valid(passScore: 0));
        result.ShouldNotHaveValidationErrorFor(c => c.PassScore);
    }

    [Fact]
    public void Should_fail_when_pass_score_is_greater_than_max_score()
    {
        var result = _validator.TestValidate(Valid(maxScore: 50, passScore: 60));
        result.ShouldHaveValidationErrorFor(c => c)
            .WithErrorMessage("Pass score must be less than or equal to max score.");
    }

    [Fact]
    public void Should_pass_when_pass_score_equals_max_score()
    {
        var result = _validator.TestValidate(Valid(maxScore: 100, passScore: 100));
        result.ShouldNotHaveAnyValidationErrors();
    }
}
