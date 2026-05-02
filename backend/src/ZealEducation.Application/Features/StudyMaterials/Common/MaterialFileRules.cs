using System.Globalization;
using System.Text;

namespace ZealEducation.Application.Features.StudyMaterials.Common;

public static class MaterialFileRules
{
    private const int MaxSlugLength = 100;
    public const long MaxSizeBytes = 50L * 1024 * 1024;

    public static readonly string[] AllowedExtensions =
    [
        ".pdf",
        ".doc",
        ".docx",
        ".xls",
        ".xlsx",
        ".ppt",
        ".pptx",
        ".mp4",
        ".mov",
        ".jpg",
        ".jpeg",
        ".png"
    ];

    public static readonly Dictionary<string, string> ExtensionToContentType = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf",
        [".doc"] = "application/msword",
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        [".xls"] = "application/vnd.ms-excel",
        [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        [".ppt"] = "application/vnd.ms-powerpoint",
        [".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        [".mp4"] = "video/mp4",
        [".mov"] = "video/quicktime",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png"
    };

    public static bool IsAllowedExtension(string fileName)
    {
        var ext = Path.GetExtension(fileName);
        return !string.IsNullOrEmpty(ext) && AllowedExtensions.Contains(ext.ToLowerInvariant());
    }

    public static string ResolveContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ExtensionToContentType.TryGetValue(ext, out var contentType)
            ? contentType
            : "application/octet-stream";
    }

    public static string ToFileType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return string.IsNullOrEmpty(ext) ? string.Empty : ext.TrimStart('.');
    }

    public static string ToSnakeCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "material";

        var decomposed = input.Trim().Normalize(NormalizationForm.FormD);
        var stripped = new StringBuilder(decomposed.Length);
        foreach (var ch in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
            stripped.Append(ch);
        }

        var ascii = stripped.ToString()
            .Replace('đ', 'd').Replace('Đ', 'D')
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant();

        var slug = new StringBuilder(ascii.Length);
        var lastUnderscore = true;
        foreach (var ch in ascii)
        {
            if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9'))
            {
                slug.Append(ch);
                lastUnderscore = false;
            }
            else if (!lastUnderscore)
            {
                slug.Append('_');
                lastUnderscore = true;
            }
        }

        var result = slug.ToString().Trim('_');
        if (result.Length > MaxSlugLength) result = result[..MaxSlugLength].TrimEnd('_');
        return string.IsNullOrEmpty(result) ? "material" : result;
    }

    public static string BuildFileName(string title, string sourceFileName)
    {
        var extension = Path.GetExtension(sourceFileName).ToLowerInvariant();
        return $"{ToSnakeCase(title)}{extension}";
    }
}
