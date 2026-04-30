using MediatR;
using ZealEducation.Application.Common.Models;

namespace ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultyExaminationCandidates;

public record GetFacultyExaminationCandidatesQuery(
    Guid ExaminationId,
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? SortBy = null,
    string? SortDirection = null) : IRequest<PaginatedList<FacultyExaminationCandidateDto>>;
