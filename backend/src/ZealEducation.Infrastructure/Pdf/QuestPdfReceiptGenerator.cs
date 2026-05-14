using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Infrastructure.Pdf;

public class QuestPdfReceiptGenerator : IReceiptPdfGenerator
{
    private static readonly CultureInfo VndCulture = new("vi-VN");
    private static readonly TimeZoneInfo VietnamTimeZone = ResolveVietnamTimeZone();

    public byte[] Generate(ReceiptPdfModel model)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A5);
                page.DefaultTextStyle(t => t.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().Text("ZEAL EDUCATION").FontSize(18).Bold().AlignCenter();
                    col.Item().Text("Payment Receipt").FontSize(13).AlignCenter();
                    col.Item().PaddingVertical(6).LineHorizontal(1);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Spacing(6);

                    KeyValue(col, "Receipt No.", model.ReceiptNumber);
                    KeyValue(col, "Date", FormatVietnamTime(model.PaymentDate));
                    KeyValue(col, "Candidate", $"{model.CandidateFullName} ({model.CandidateCode})");
                    if (!string.IsNullOrWhiteSpace(model.CourseTitle))
                        KeyValue(col, "Course", model.CourseTitle!);
                    KeyValue(col, "Fee Type", model.FeeType.ToString());
                    KeyValue(col, "Payment Method", FormatPaymentMethod(model.PaymentMethod));
                    if (model.InstallmentNo.HasValue)
                        KeyValue(col, "Installment No.", model.InstallmentNo.Value.ToString());

                    col.Item().PaddingTop(10).LineHorizontal(0.5f);

                    KeyValue(col, "Base Amount", FormatMoney(model.BaseAmount));
                    if (model.PenaltyAmount > 0)
                        KeyValue(col, "Late Penalty (5%)", FormatMoney(model.PenaltyAmount));
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Total Paid").Bold();
                        row.ConstantItem(120).AlignRight().Text(FormatMoney(model.TotalAmount)).Bold();
                    });

                    col.Item().PaddingTop(10).LineHorizontal(0.5f);

                    KeyValue(col, "Outstanding (after)", FormatMoney(model.OutstandingBalanceAfter));
                    KeyValue(col, "Processed By", model.ProcessedByStaffName);
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Thank you. — Generated ").FontSize(9);
                    t.Span(FormatVietnamTime(DateTime.UtcNow)).FontSize(9);
                });
            });
        });

        return document.GeneratePdf();
    }

    private static void KeyValue(ColumnDescriptor col, string label, string value)
    {
        col.Item().Row(row =>
        {
            row.RelativeItem().Text(label);
            row.ConstantItem(160).AlignRight().Text(value);
        });
    }

    private static string FormatMoney(decimal value) => value.ToString("C0", VndCulture);

    private static string FormatPaymentMethod(PaymentMethod method) => method switch
    {
        PaymentMethod.Cash => "Cash",
        PaymentMethod.BankTransfer => "Bank Transfer",
        _ => method.ToString(),
    };

    // Receipt la chung tu chinh thuc cua doanh nghiep VN — luon hien thi gio VN
    // bat ke server chay o TZ nao, bat ke client o dau.
    private static string FormatVietnamTime(DateTime value)
    {
        // EF tra DateTime voi Kind = Unspecified; UtcDateTimeConverter chi tac dong tang JSON.
        // O day phai ep Utc thu cong truoc khi convert sang Asia/Ho_Chi_Minh.
        var utc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };
        var local = TimeZoneInfo.ConvertTimeFromUtc(utc, VietnamTimeZone);
        return local.ToString("yyyy-MM-dd HH:mm");
    }

    private static TimeZoneInfo ResolveVietnamTimeZone()
    {
        // .NET 8+ ho tro IANA tren ca Linux va Windows, nhung fallback de chac chan.
        try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
    }
}
