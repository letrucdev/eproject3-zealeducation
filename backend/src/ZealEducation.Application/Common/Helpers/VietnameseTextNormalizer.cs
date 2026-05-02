using System.Text;
using System.Text.RegularExpressions;

namespace ZealEducation.Application.Common.Helpers;

public static partial class VietnameseTextNormalizer
{
    [GeneratedRegex(@"\p{Mn}", RegexOptions.CultureInvariant)]
    private static partial Regex CombiningMarksRegex();

    public static string ToTitleCaseAscii(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var stripped = StripDiacritics(input);
        var tokens = stripped.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var sb = new StringBuilder();
        for (var i = 0; i < tokens.Length; i++)
        {
            if (i > 0) sb.Append(' ');
            var token = tokens[i];
            sb.Append(char.ToUpperInvariant(token[0]));
            if (token.Length > 1)
                sb.Append(token[1..].ToLowerInvariant());
        }
        return sb.ToString();
    }

    private static string StripDiacritics(string input)
    {
        var normalized = input.Normalize(NormalizationForm.FormD);
        var withoutMarks = CombiningMarksRegex().Replace(normalized, string.Empty);
        var ascii = withoutMarks.Normalize(NormalizationForm.FormC);

        // FormD does not decompose Vietnamese đ/Đ into a base letter + mark.
        var sb = new StringBuilder(ascii.Length);
        foreach (var ch in ascii)
        {
            sb.Append(ch switch
            {
                'đ' => 'd',
                'Đ' => 'D',
                _ => ch,
            });
        }
        return sb.ToString();
    }
}
