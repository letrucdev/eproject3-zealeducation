using MediatR;

namespace ZealEducation.Application.Features.CourseEnquiries.Queries.GetEnquiryStatistics;

public record GetEnquiryStatisticsQuery() : IRequest<EnquiryStatisticsDto>;
