using System.Globalization;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Common.Models;

namespace ZealEducation.Infrastructure.Pdf;

public class QuestPdfCertificateGenerator : ICertificatePdfGenerator
{
    private const string TemplateRelativePath = "Pdf/Templates/certificate-template.png";
    private const string FontsRelativePath = "Pdf/Fonts";
    private const string CertificateFontFamily = "SVN-Graphitel";
    private static readonly CultureInfo IssuedDateCulture = new("en-US");
    private static readonly Lazy<byte[]> TemplateBytes = new(LoadTemplate);
    private static readonly Lazy<bool> FontsRegistered = new(RegisterFonts);

    public byte[] Generate(CertificatePdfModel model)
    {
        var template = TemplateBytes.Value;
        _ = FontsRegistered.Value;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(0);
                page.Size(PageSizes.A4.Landscape());
                page.PageColor(Colors.White);
                page.DefaultTextStyle(t => t.FontFamily("Times New Roman").FontColor("#1f4380"));

                page.Content().Layers(layers =>
                {
                    // Background: certificate template image, stretched to fill the page.
                    layers.PrimaryLayer().Image(template).FitArea();

                    layers.Layer().PaddingTop(250).AlignCenter().Text(model.CandidateName)
                        .FontFamily(CertificateFontFamily).FontSize(40).FontColor("#1f4380");

                    layers.Layer().PaddingTop(370).AlignCenter().Text(model.CourseName)
                        .FontFamily("Times New Roman").FontSize(20).SemiBold().FontColor("#1f4380");

                    layers.Layer().PaddingTop(465).PaddingLeft(125).PaddingRight(536).AlignCenter().Text(
                        model.IssuedDate.ToString("MMMM d, yyyy", IssuedDateCulture))
                        .FontFamily("Times New Roman").FontSize(13).FontColor("#333333");

                    // Certificate number — small, bottom-right corner.
                    layers.Layer().PaddingTop(560).PaddingLeft(620).Text($"No. {model.CertificateNumber}")
                        .FontFamily("Times New Roman").FontSize(9).FontColor("#666666");
                });
            });
        });

        return document.GeneratePdf();
    }

    private static bool RegisterFonts()
    {
        var fontDir = Path.Combine(AppContext.BaseDirectory, FontsRelativePath);
        if (!Directory.Exists(fontDir))
        {
            return false;
        }

        foreach (var file in Directory.EnumerateFiles(fontDir, "*.*")
                     .Where(f => f.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase)
                                 || f.EndsWith(".otf", StringComparison.OrdinalIgnoreCase)))
        {
            using var stream = File.OpenRead(file);
            FontManager.RegisterFont(stream);
        }

        return true;
    }

    private static byte[] LoadTemplate()
    {
        var path = Path.Combine(AppContext.BaseDirectory, TemplateRelativePath);
        if (!File.Exists(path))
        {
            throw new InvalidOperationException(
                $"Certificate template image not found at '{path}'. " +
                "Place the template PNG at backend/src/ZealEducation.Infrastructure/Pdf/Templates/certificate-template.png and rebuild.");
        }
        return File.ReadAllBytes(path);
    }
}
