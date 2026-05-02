using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Payments.Queries.GetFeeStructures;

public record GetFeeStructuresQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    PaymentStatus? Status = null,
    FeeType? Type = null,
    string? SortBy = null,
    string? SortDirection = null) : IRequest<PaginatedList<FeeStructureListItemDto>>;
