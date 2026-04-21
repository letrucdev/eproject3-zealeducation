using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Faculties.Commands.CreateFaculty;

public class CreateFacultyResponse
{
    public Guid UserAccountId { get; set; }
    public Guid StaffId { get; set; }
    public Guid FacultyId { get; set; }
    public string Username { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public UserRole Role { get; set; }
    public string Position { get; set; } = default!;
    public string Department { get; set; } = default!;
    public DateOnly JoinedDate { get; set; }
    public string FacultyCode { get; set; } = default!;
    public string Qualification { get; set; } = default!;
    public string Specialization { get; set; } = default!;
    public int ExperienceYears { get; set; }
}
