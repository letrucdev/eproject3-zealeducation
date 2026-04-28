namespace ZealEducation.Application.Features.StudyMaterials.Queries;

public class StudyMaterialListItemDto
{
    public Guid MaterialId { get; set; }
    public Guid CourseId { get; set; }
    public string Title { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public string FileType { get; set; } = default!;
    public decimal FileSizeMb { get; set; }
    public bool IsActive { get; set; }
    public DateTime UploadedAt { get; set; }
    public string UploadedByName { get; set; } = default!;
}
