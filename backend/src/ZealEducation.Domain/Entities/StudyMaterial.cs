using ZealEducation.Domain.Common;

namespace ZealEducation.Domain.Entities;

public class StudyMaterial : BaseAuditableEntity
{
    public Guid CourseId { get; set; }
    public Guid UploadedByStaffId { get; set; }
    public string Title { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public string FilePath { get; set; } = default!;
    public string FileType { get; set; } = default!;
    public decimal FileSizeMb { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime UploadedAt { get; set; }

    public Course Course { get; set; } = default!;
    public Staff UploadedByStaff { get; set; } = default!;
    public ICollection<MaterialDownloadLog> DownloadLogs { get; set; } = [];
}
