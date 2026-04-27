using ZealEducation.Domain.Common;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Domain.Entities;

public class InstallmentPlan : BaseAuditableEntity
{
    public Guid FeeId { get; set; }
    public int InstallmentNo { get; set; }
    public decimal AmountDue { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal AmountPaid { get; set; }
    public DateOnly? PaidDate { get; set; }
    public InstallmentStatus Status { get; set; } = InstallmentStatus.Pending;
    public decimal PenaltyAmount { get; set; }

    public FeeStructure FeeStructure { get; set; } = default!;
}
