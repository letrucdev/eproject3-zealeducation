using MediatR;
using ZealEducation.Application.Common.Models;

namespace ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultySchedule;

public record GetFacultyScheduleQuery(
    DateOnly From,
    DateOnly To,
    int Page = 1,
    int PageSize = 10,
    Guid? BatchId = null,
    Guid? CourseId = null,
    string? SortBy = null,
    string? SortDirection = null) : IRequest<PaginatedList<FacultyScheduleItemDto>>;
