using MediatR;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.CourseEnquiries.Commands.ConvertEnquiry;

public record ConvertEnquiryCommand(
    Guid EnquiryId,
    string Email,
    DateOnly Dob,
    Gender Gender,
    string? Address,
    string? EmergencyContact) : IRequest<ConvertEnquiryResponse>;
