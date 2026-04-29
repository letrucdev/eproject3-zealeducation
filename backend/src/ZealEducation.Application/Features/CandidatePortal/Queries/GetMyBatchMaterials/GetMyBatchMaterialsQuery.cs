using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.StudyMaterials.Queries;

namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyBatchMaterials;

public record GetMyBatchMaterialsQuery(
    Guid BatchId,
    int Page = 1,
    int PageSize = 12,
    string? Search = null) : IRequest<PaginatedList<StudyMaterialListItemDto>>;
