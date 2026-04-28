using MediatR;
using ZealEducation.Application.Common.Models;

namespace ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultyExaminations;

public record GetFacultyExaminationsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    Guid? BatchId = null,
    string? SortBy = null,
    string? SortDirection = null) : IRequest<PaginatedList<FacultyExaminationSummaryDto>>;
