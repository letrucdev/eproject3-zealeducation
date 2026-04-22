using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Staffs.Queries.GetStaffById;

public class StaffDetailDto
{
    public Guid StaffId { get; set; }
    public Guid UserAccountId { get; set; }
    public string Username { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public DateOnly Dob { get; set; }
    public Gender Gender { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public string Position { get; set; } = default!;
    public string Department { get; set; } = default!;
    public DateOnly JoinedDate { get; set; }
    public DateTime? LastLogin { get; set; }

    public Guid? FacultyId { get; set; }
    public string? FacultyCode { get; set; }
    public string? Qualification { get; set; }
    public string? Specialization { get; set; }
    public int? ExperienceYears { get; set; }
}
