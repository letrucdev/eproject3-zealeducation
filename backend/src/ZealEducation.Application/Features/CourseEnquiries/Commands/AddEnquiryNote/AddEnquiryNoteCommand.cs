using MediatR;

namespace ZealEducation.Application.Features.CourseEnquiries.Commands.AddEnquiryNote;

public record AddEnquiryNoteCommand(
    Guid EnquiryId,
    string Content) : IRequest<AddEnquiryNoteResponse>;
