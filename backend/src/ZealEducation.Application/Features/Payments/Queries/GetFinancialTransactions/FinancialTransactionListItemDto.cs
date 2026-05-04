using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Payments.Queries.GetFinancialTransactions;

public class FinancialTransactionListItemDto
{
    public Guid TransactionId { get; set; }
    public Guid FeeId { get; set; }
    public string ReceiptNumber { get; set; } = default!;
    public DateTime PaymentDate { get; set; }
    public string CandidateCode { get; set; } = default!;
    public string CandidateFullName { get; set; } = default!;
    public string? CourseTitle { get; set; }
    public FeeType FeeType { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string ProcessedByStaffName { get; set; } = default!;
}
