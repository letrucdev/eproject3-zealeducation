using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Payments.Queries.GetFeeStructureDetail;

public class FeeStructureDetailDto
{
    public Guid FeeId { get; set; }
    public Guid CandidateId { get; set; }
    public string CandidateCode { get; set; } = default!;
    public string CandidateFullName { get; set; } = default!;
    public string? CandidateEmail { get; set; }
    public string? CandidatePhone { get; set; }
    public bool CandidateIsActive { get; set; }
    public Guid? EnrollmentId { get; set; }
    public DateOnly? EnrollmentDate { get; set; }
    public Guid? CourseId { get; set; }
    public string? CourseName { get; set; }
    public int? DurationWeeks { get; set; }
    public FeeType FeeType { get; set; }
    public decimal TotalFee { get; set; }
    public decimal PenaltyApplied { get; set; }
    public decimal ProjectedPenalty { get; set; }
    public decimal AdjustedTotalFee { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal OutstandingBalance { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public PaymentType PaymentType { get; set; }
    public string? Notes { get; set; }
    public List<InstallmentPlanDto> InstallmentPlans { get; set; } = [];
    public List<PaymentTransactionDto> PaymentTransactions { get; set; } = [];
}

public class InstallmentPlanDto
{
    public Guid Id { get; set; }
    public int InstallmentNo { get; set; }
    public decimal AmountDue { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal AmountPaid { get; set; }
    public DateOnly? PaidDate { get; set; }
    public InstallmentStatus Status { get; set; }
    public decimal PenaltyAmount { get; set; }
}

public class PaymentTransactionDto
{
    public Guid Id { get; set; }
    public Guid? InstallmentPlanId { get; set; }
    public int? InstallmentNo { get; set; }
    public string ReceiptNumber { get; set; } = default!;
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? ProcessedByStaffName { get; set; }
    public bool HasBankTransferProof { get; set; }
}
