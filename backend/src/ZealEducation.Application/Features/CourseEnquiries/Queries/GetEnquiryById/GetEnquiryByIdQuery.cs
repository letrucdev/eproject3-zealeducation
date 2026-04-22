using MediatR;

namespace ZealEducation.Application.Features.CourseEnquiries.Queries.GetEnquiryById;

public record GetEnquiryByIdQuery(Guid EnquiryId) : IRequest<CourseEnquiryDetailDto>;
