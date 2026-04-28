using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.StudyMaterials.Queries.GetMaterialSiblingCourses;

public class GetMaterialSiblingCoursesQueryHandler(
    IRepository<StudyMaterial> materialRepository)
    : IRequestHandler<GetMaterialSiblingCoursesQuery, List<MaterialSiblingCourseDto>>
{
    public async Task<List<MaterialSiblingCourseDto>> Handle(
        GetMaterialSiblingCoursesQuery request,
        CancellationToken cancellationToken)
    {
        var filePath = await materialRepository.Query()
            .Where(m => m.Id == request.MaterialId)
            .Select(m => m.FilePath)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(StudyMaterial), request.MaterialId);

        return await materialRepository.Query()
            .Where(m => m.FilePath == filePath)
            .OrderBy(m => m.Course.CourseName)
            .Select(m => new MaterialSiblingCourseDto
            {
                MaterialId = m.Id,
                CourseId = m.CourseId,
                CourseName = m.Course.CourseName
            })
            .ToListAsync(cancellationToken);
    }
}
