using ZealEducation.Application.Common.Helpers;

namespace ZealEducation.Application.UnitTests.Common.Helpers;

public class VietnameseTextNormalizerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n  ")]
    public void Should_return_empty_for_null_or_whitespace(string? input)
    {
        var result = VietnameseTextNormalizer.ToTitleCaseAscii(input);
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("nguyễn văn an", "Nguyen Van An")]
    [InlineData("TRẦN THỊ BÍCH", "Tran Thi Bich")]
    [InlineData("đặng quốc đạt", "Dang Quoc Dat")]
    [InlineData("Lê Hoàng Đức", "Le Hoang Duc")]
    public void Should_strip_diacritics_and_title_case(string input, string expected)
    {
        var result = VietnameseTextNormalizer.ToTitleCaseAscii(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("đ", "D")]
    [InlineData("Đ", "D")]
    [InlineData("đông", "Dong")]
    [InlineData("ĐÔNG", "Dong")]
    public void Should_handle_d_with_stroke_explicitly(string input, string expected)
    {
        var result = VietnameseTextNormalizer.ToTitleCaseAscii(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("  john   doe  ", "John Doe")]
    [InlineData("alice\tbob\ncarol", "Alice Bob Carol")]
    public void Should_collapse_whitespace_separators(string input, string expected)
    {
        var result = VietnameseTextNormalizer.ToTitleCaseAscii(input);
        result.Should().Be(expected);
    }

    [Fact]
    public void Should_handle_single_character_token()
    {
        var result = VietnameseTextNormalizer.ToTitleCaseAscii("a b c");
        result.Should().Be("A B C");
    }
}
