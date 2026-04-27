using MediatR;
using ZealEducation.Application.Common.Models;

namespace ZealEducation.Application.Features.Batches.Queries.GetAssignableCandidates;

public record GetAssignableCandidatesQuery(
    Guid BatchId,
    int Page = 1,
    int PageSize = 10,
    string? Search = null) : IRequest<PaginatedList<AssignableCandidateDto>>;
