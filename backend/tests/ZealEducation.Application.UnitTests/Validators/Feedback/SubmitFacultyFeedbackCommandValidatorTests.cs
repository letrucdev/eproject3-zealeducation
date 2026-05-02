using FluentValidation.TestHelper;
using ZealEducation.Application.Features.Feedback.Commands.SubmitFacultyFeedback;

namespace ZealEducation.Application.UnitTests.Validators.Feedback;

public class SubmitFacultyFeedbackCommandValidatorTests
{
    private readonly SubmitFacultyFeedbackCommandValidator _validator = new();

    private static SubmitFacultyFeedbackCommand Valid(
        Guid? batchId = null,
        Guid? facultyId = null,
        int rating = 4,
        string? comment = "Helpful instructor") => new(
            batchId ?? Guid.NewGuid(),
            facultyId ?? Guid.NewGuid(),
            rating,
            comment);

    [Fact]
    public void Should_pass_for_a_fully_valid_command()
    {
        var result = _validator.TestValidate(Valid());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_pass_when_comment_is_null()
    {
        var result = _validator.TestValidate(Valid(comment: null));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_fail_when_batch_id_is_empty()
    {
        var result = _validator.TestValidate(Valid(batchId: Guid.Empty));
        result.ShouldHaveValidationErrorFor(c => c.BatchId);
    }

    [Fact]
    public void Should_fail_when_faculty_id_is_empty()
    {
        var result = _validator.TestValidate(Valid(facultyId: Guid.Empty));
        result.ShouldHaveValidationErrorFor(c => c.FacultyId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    [InlineData(6)]
    [InlineData(10)]
    public void Should_fail_when_rating_outside_1_to_5(int rating)
    {
        var result = _validator.TestValidate(Valid(rating: rating));
        result.ShouldHaveValidationErrorFor(c => c.Rating);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void Should_pass_for_rating_within_inclusive_range(int rating)
    {
        var result = _validator.TestValidate(Valid(rating: rating));
        result.ShouldNotHaveValidationErrorFor(c => c.Rating);
    }

    [Fact]
    public void Should_fail_when_comment_exceeds_1000_chars()
    {
        var result = _validator.TestValidate(Valid(comment: new string('z', 1001)));
        result.ShouldHaveValidationErrorFor(c => c.Comment);
    }

    [Fact]
    public void Should_pass_when_comment_is_exactly_1000_chars()
    {
        var result = _validator.TestValidate(Valid(comment: new string('z', 1000)));
        result.ShouldNotHaveValidationErrorFor(c => c.Comment);
    }
}
