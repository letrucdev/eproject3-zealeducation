using MediatR;
using ZealEducation.Application.Common.Models;

namespace ZealEducation.Application.Features.Faculties.Queries.GetFaculties;

public record GetFacultiesQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null) : IRequest<PaginatedList<FacultyListItemDto>>;
