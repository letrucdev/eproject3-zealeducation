using ZealEducation.Domain.Common;

namespace ZealEducation.Domain.Entities;

public class Course : BaseAuditableEntity
{
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public decimal Price { get; set; }
    public bool IsPublished { get; set; }
}
