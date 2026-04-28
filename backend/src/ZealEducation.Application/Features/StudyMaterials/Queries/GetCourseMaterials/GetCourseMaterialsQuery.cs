using MediatR;
using ZealEducation.Application.Common.Models;

namespace ZealEducation.Application.Features.StudyMaterials.Queries.GetCourseMaterials;

public record GetCourseMaterialsQuery(
    Guid CourseId,
    int Page = 1,
    int PageSize = 12,
    string? Search = null,
    bool IncludeInactive = true) : IRequest<PaginatedList<StudyMaterialListItemDto>>;
