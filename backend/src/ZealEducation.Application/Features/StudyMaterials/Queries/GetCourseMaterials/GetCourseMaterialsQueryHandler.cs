using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.StudyMaterials.Queries.GetCourseMaterials;

public class GetCourseMaterialsQueryHandler(
    IRepository<StudyMaterial> materialRepository) : IRequestHandler<GetCourseMaterialsQuery, PaginatedList<StudyMaterialListItemDto>>
{
    public async Task<PaginatedList<StudyMaterialListItemDto>> Handle(GetCourseMaterialsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 12 : Math.Min(request.PageSize, 60);
        var search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim().ToLower();

        var query = materialRepository.Query().Where(m => m.CourseId == request.CourseId);

        if (!request.IncludeInactive)
            query = query.Where(m => m.IsActive);

        if (search != null)
            query = query.Where(m => m.Title.ToLower().Contains(search));

        var projected = query
            .OrderByDescending(m => m.UploadedAt)
            .ThenBy(m => m.Title)
            .Select(m => new StudyMaterialListItemDto
            {
                MaterialId = m.Id,
                CourseId = m.CourseId,
                Title = m.Title,
                FileName = m.FileName,
                FileType = m.FileType,
                FileSizeMb = m.FileSizeMb,
                IsActive = m.IsActive,
                UploadedAt = m.UploadedAt,
                UploadedByName = m.UploadedByStaff.UserAccount.FullName
            });

        return await PaginatedList<StudyMaterialListItemDto>.CreateAsync(projected, page, pageSize);
    }
}
