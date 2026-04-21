using ZealEducation.Domain.Common;

namespace ZealEducation.Domain.Entities;

public class Staff : BaseAuditableEntity
{
    public Guid UserAccountId { get; set; }
    public string Position { get; set; } = default!;
    public string Department { get; set; } = default!;
    public DateOnly JoinedDate { get; set; }
    public bool IsActive { get; set; } = true;

    public UserAccount UserAccount { get; set; } = default!;
    public Faculty? Faculty { get; set; }
}
