using ZealEducation.Domain.Common;

namespace ZealEducation.Domain.Entities;

public class Fine : BaseAuditableEntity
{
    public Guid FeeId { get; set; }
    public Guid CandidateId { get; set; }
    public Guid IssuedByStaffId { get; set; }
    public string ViolationReason { get; set; } = default!;
    public decimal PenaltyAmount { get; set; }
    public DateOnly IssuedDate { get; set; }
    public bool IsPaid { get; set; }
    public DateOnly? PaidDate { get; set; }

    public FeeStructure FeeStructure { get; set; } = default!;
    public Candidate Candidate { get; set; } = default!;
    public Staff IssuedByStaff { get; set; } = default!;
}
