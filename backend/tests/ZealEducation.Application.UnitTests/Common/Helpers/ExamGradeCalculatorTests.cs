using ZealEducation.Application.Common.Helpers;

namespace ZealEducation.Application.UnitTests.Common.Helpers;

public class ExamGradeCalculatorTests
{
    [Theory]
    [InlineData(95, 100, 50, "A")]
    [InlineData(90, 100, 50, "A")]
    [InlineData(89.99, 100, 50, "B")]
    [InlineData(80, 100, 50, "B")]
    [InlineData(79.99, 100, 50, "C")]
    [InlineData(70, 100, 50, "C")]
    [InlineData(69.99, 100, 50, "D")]
    [InlineData(50, 100, 50, "D")]
    public void Should_return_correct_grade_for_passing_scores(double score, int maxScore, int passScore, string expected)
    {
        var grade = ExamGradeCalculator.Calculate((decimal)score, maxScore, passScore);
        grade.Should().Be(expected);
    }

    [Theory]
    [InlineData(49.99, 100, 50)]
    [InlineData(0, 100, 50)]
    [InlineData(25, 50, 30)]
    public void Should_return_F_when_score_below_pass(double score, int maxScore, int passScore)
    {
        var grade = ExamGradeCalculator.Calculate((decimal)score, maxScore, passScore);
        grade.Should().Be("F");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Should_return_empty_when_max_score_invalid(int maxScore)
    {
        var grade = ExamGradeCalculator.Calculate(50m, maxScore, 30);
        grade.Should().BeEmpty();
    }

    [Theory]
    [InlineData(45, 50, 25, "A")]
    [InlineData(40, 50, 25, "B")]
    [InlineData(35, 50, 25, "C")]
    [InlineData(28, 50, 25, "D")]
    public void Should_calculate_grade_relative_to_max_score(double score, int maxScore, int passScore, string expected)
    {
        var grade = ExamGradeCalculator.Calculate((decimal)score, maxScore, passScore);
        grade.Should().Be(expected);
    }
}
