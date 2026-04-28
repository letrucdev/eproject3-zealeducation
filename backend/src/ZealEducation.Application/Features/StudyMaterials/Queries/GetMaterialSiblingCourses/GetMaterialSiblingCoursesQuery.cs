using MediatR;

namespace ZealEducation.Application.Features.StudyMaterials.Queries.GetMaterialSiblingCourses;

public record GetMaterialSiblingCoursesQuery(Guid MaterialId)
    : IRequest<List<MaterialSiblingCourseDto>>;

public class MaterialSiblingCourseDto
{
    public Guid MaterialId { get; set; }
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = default!;
}
