using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Staffs.Commands.CreateStaff;

public class CreateStaffResponse
{
    public Guid UserAccountId { get; set; }
    public Guid StaffId { get; set; }
    public string Username { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public UserRole Role { get; set; }
    public string Position { get; set; } = default!;
    public string Department { get; set; } = default!;
    public DateOnly JoinedDate { get; set; }
}
