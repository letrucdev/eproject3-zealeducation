using MediatR;
using ZealEducation.Application.Common.Models;

namespace ZealEducation.Application.Features.StudyMaterials.Queries.GetMaterialCourses;

public record GetMaterialCoursesQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null) : IRequest<PaginatedList<MaterialCourseListItemDto>>;
