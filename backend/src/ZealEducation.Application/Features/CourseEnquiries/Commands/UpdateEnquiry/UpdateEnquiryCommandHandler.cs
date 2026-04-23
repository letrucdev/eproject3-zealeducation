using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.CourseEnquiries.Commands.UpdateEnquiry;

public class UpdateEnquiryCommandHandler(
    IRepository<CourseEnquiry> enquiryRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateEnquiryCommand, Unit>
{
    public async Task<Unit> Handle(UpdateEnquiryCommand request, CancellationToken cancellationToken)
    {
        var enquiry = await enquiryRepository.GetByIdAsync(request.EnquiryId, cancellationToken)
            ?? throw new NotFoundException(nameof(CourseEnquiry), request.EnquiryId);

        if (enquiry.Status == EnquiryStatus.Converted)
            throw new ConflictException("A converted enquiry cannot be updated.");

        if (enquiry.Status == EnquiryStatus.Closed) throw new ConflictException("A closed enquiry cannot be updated.");

        enquiry.FullName = request.FullName.Trim();
        enquiry.Phone = request.Phone.Trim();
        enquiry.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        enquiry.CourseInterested = request.CourseInterested.Trim();
        enquiry.Source = request.Source;
        enquiry.Status = request.Status;
        enquiry.NextFollowUpDate = request.NextFollowUpDate;

        enquiryRepository.Update(enquiry);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
