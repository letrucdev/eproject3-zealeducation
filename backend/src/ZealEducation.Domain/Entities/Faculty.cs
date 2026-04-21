using ZealEducation.Domain.Common;

namespace ZealEducation.Domain.Entities;

public class Faculty : BaseAuditableEntity
{
    public Guid StaffId { get; set; }
    public string FacultyCode { get; set; } = default!;
    public string Qualification { get; set; } = default!;
    public string Specialization { get; set; } = default!;
    public int ExperienceYears { get; set; }

    public Staff Staff { get; set; } = default!;
}
