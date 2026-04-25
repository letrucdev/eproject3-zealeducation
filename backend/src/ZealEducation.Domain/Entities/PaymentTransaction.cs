using ZealEducation.Domain.Common;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Domain.Entities;

public class PaymentTransaction : BaseAuditableEntity
{
    public Guid FeeId { get; set; }
    public Guid ProcessedByStaffId { get; set; }
    public Guid? InstallmentPlanId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string ReceiptNumber { get; set; } = default!;
    public DateTime PaymentDate { get; set; }
    public string? ReceiptFilePath { get; set; }
    public string? BankTransferProofPath { get; set; }

    public FeeStructure FeeStructure { get; set; } = default!;
    public Staff ProcessedByStaff { get; set; } = default!;
    public InstallmentPlan? InstallmentPlan { get; set; }
}
