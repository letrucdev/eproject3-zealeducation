using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Feedback.Queries.GetFeedbacks;

public record GetFeedbacksQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    FeedbackType? Type = null,
    Guid? BatchId = null,
    int? Rating = null,
    bool? IsProcessed = null,
    string? SortBy = null,
    string? SortDirection = null) : IRequest<PaginatedList<FeedbackListItemDto>>;
