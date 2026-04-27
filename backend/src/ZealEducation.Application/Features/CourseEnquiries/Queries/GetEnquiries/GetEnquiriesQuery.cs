using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.CourseEnquiries.Queries.GetEnquiries;

public record GetEnquiriesQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    EnquiryStatus? Status = null,
    EnquirySource? Source = null,
    bool? DueFollowUpOnly = null,
    string? SortBy = null,
    string? SortDirection = null) : IRequest<PaginatedList<CourseEnquiryListItemDto>>;
