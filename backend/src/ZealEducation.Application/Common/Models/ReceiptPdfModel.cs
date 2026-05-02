using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Common.Models;

public class ReceiptPdfModel
{
    public string ReceiptNumber { get; set; } = default!;
    public DateTime PaymentDate { get; set; }
    public string CandidateCode { get; set; } = default!;
    public string CandidateFullName { get; set; } = default!;
    public string? CourseTitle { get; set; }
    public FeeType FeeType { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal PenaltyAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string ProcessedByStaffName { get; set; } = default!;
    public decimal OutstandingBalanceAfter { get; set; }
    public int? InstallmentNo { get; set; }
}
