using MediatR;
using ZealEducation.Application.Common.Models;

namespace ZealEducation.Application.Features.ExamResults.Queries.GetExaminationResults;

public record GetExaminationResultsQuery(
    Guid ExaminationId,
    int Page = 1,
    int PageSize = 10,
    string? SortBy = null,
    string? SortDirection = null) : IRequest<PaginatedList<ExamResultDto>>;
