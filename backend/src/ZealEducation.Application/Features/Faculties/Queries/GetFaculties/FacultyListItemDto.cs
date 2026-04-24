namespace ZealEducation.Application.Features.Faculties.Queries.GetFaculties;

public class FacultyListItemDto
{
    public Guid FacultyId { get; set; }
    public string FacultyCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string Qualification { get; set; } = default!;
    public string Specialization { get; set; } = default!;
    public int ExperienceYears { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
