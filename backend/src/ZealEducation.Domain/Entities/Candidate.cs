using ZealEducation.Domain.Common;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Domain.Entities;

public class Candidate : BaseAuditableEntity
{
    public Guid UserAccountId { get; set; }
    public string CandidateCode { get; set; } = default!;
    public string? Address { get; set; }
    public string? EmergencyContact { get; set; }
    public string? Notes { get; set; }
    public CandidateStatus Status { get; set; } = CandidateStatus.Active;
    public DateTime RegisteredAt { get; set; }
    public Guid? RegisteredByStaffId { get; set; }

    public UserAccount UserAccount { get; set; } = default!;
    public Staff? RegisteredByStaff { get; set; }
}
