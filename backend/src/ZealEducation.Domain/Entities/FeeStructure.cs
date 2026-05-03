using ZealEducation.Domain.Common;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Domain.Entities;

public class FeeStructure : BaseAuditableEntity
{
    public Guid CandidateId { get; set; }
    public decimal TotalFee { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal PenaltyApplied { get; set; }
    public FeeType FeeType { get; set; } = FeeType.Tuition;
    public decimal OutstandingBalance { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
    public PaymentType PaymentType { get; set; } = PaymentType.NotSet;
    public string? Notes { get; set; }

    public Candidate Candidate { get; set; } = default!;
    public Enrollment? Enrollment { get; set; }
    public ICollection<InstallmentPlan> InstallmentPlans { get; set; } = [];
    public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = [];
    public Fine? Fine { get; set; }
}
