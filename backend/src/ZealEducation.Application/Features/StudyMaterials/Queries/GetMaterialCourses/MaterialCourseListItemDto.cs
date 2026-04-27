namespace ZealEducation.Application.Features.StudyMaterials.Queries.GetMaterialCourses;

public class MaterialCourseListItemDto
{
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = default!;
    public int TotalMaterials { get; set; }
}
