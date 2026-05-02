using ZealEducation.Domain.Common;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Domain.Entities;

public class UserAccount : BaseAuditableEntity
{
    public string Username { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public DateOnly Dob { get; set; }
    public Gender Gender { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; }
    public int FailedLoginCount { get; set; }
    public DateTime? LastLogin { get; set; }
}
