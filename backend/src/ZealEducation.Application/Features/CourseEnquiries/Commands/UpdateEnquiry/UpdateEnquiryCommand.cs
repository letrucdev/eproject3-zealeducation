using MediatR;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.CourseEnquiries.Commands.UpdateEnquiry;

public record UpdateEnquiryCommand(
    Guid EnquiryId,
    string FullName,
    string Phone,
    string? Email,
    string CourseInterested,
    EnquirySource Source,
    EnquiryStatus Status,
    DateOnly? NextFollowUpDate) : IRequest<Unit>;
