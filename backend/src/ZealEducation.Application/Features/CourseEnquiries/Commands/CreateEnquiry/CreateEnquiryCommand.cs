using MediatR;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.CourseEnquiries.Commands.CreateEnquiry;

public record CreateEnquiryCommand(
    string FullName,
    string Phone,
    string? Email,
    string CourseInterested,
    EnquirySource Source,
    EnquiryStatus Status,
    DateOnly? NextFollowUpDate) : IRequest<CreateEnquiryResponse>;
