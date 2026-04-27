using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.StudyMaterials.Queries.GetMaterialCourses;

public class GetMaterialCoursesQueryHandler(
    IRepository<Course> courseRepository,
    IRepository<StudyMaterial> materialRepository) : IRequestHandler<GetMaterialCoursesQuery, PaginatedList<MaterialCourseListItemDto>>
{
    public async Task<PaginatedList<MaterialCourseListItemDto>> Handle(GetMaterialCoursesQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);
        var search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim().ToLower();

        var courses = courseRepository.Query().Where(c => c.IsActive);
        var materials = materialRepository.Query();

        if (search != null)
        {
            courses = courses.Where(c =>
                materials.Any(m => m.CourseId == c.Id && m.Title.ToLower().Contains(search)));
        }

        var projected = courses
            .OrderBy(c => c.CourseName)
            .Select(c => new MaterialCourseListItemDto
            {
                CourseId = c.Id,
                CourseName = c.CourseName,
                TotalMaterials = search == null
                    ? materials.Count(m => m.CourseId == c.Id)
                    : materials.Count(m => m.CourseId == c.Id && m.Title.ToLower().Contains(search))
            });

        return await PaginatedList<MaterialCourseListItemDto>.CreateAsync(projected, page, pageSize);
    }
}
