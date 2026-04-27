using ZealEducation.Domain.Common;

namespace ZealEducation.Domain.Entities;

public class MaterialDownloadLog : BaseAuditableEntity
{
    public Guid MaterialId { get; set; }
    public Guid CandidateId { get; set; }
    public DateTime DownloadedAt { get; set; }

    public StudyMaterial Material { get; set; } = default!;
    public Candidate Candidate { get; set; } = default!;
}
