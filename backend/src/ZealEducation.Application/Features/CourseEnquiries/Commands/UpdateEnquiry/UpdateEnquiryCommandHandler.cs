using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.CourseEnquiries.Commands.UpdateEnquiry;

public class UpdateEnquiryCommandHandler(
    IRepository<CourseEnquiry> enquiryRepository,
    IRepository<Course> courseRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateEnquiryCommand, Unit>
{
    public async Task<Unit> Handle(UpdateEnquiryCommand request, CancellationToken cancellationToken)
    {
        var enquiry = await enquiryRepository.GetByIdAsync(request.EnquiryId, cancellationToken)
            ?? throw new NotFoundException(nameof(CourseEnquiry), request.EnquiryId);

        if (enquiry.Status == EnquiryStatus.Converted)
            throw new ConflictException("A converted enquiry cannot be updated.");

        if (enquiry.Status == EnquiryStatus.Closed) throw new ConflictException("A closed enquiry cannot be updated.");

        if (enquiry.CourseInterestedId != request.CourseInterestedId)
        {
            var course = await courseRepository.GetByIdAsync(request.CourseInterestedId, cancellationToken)
                ?? throw new NotFoundException(nameof(Course), request.CourseInterestedId);

            if (!course.IsActive)
                throw new ConflictException("The selected course is inactive and no longer accepting enquiries.");

            enquiry.CourseInterestedId = course.Id;
        }

        var phone = request.Phone.Trim();
        if (enquiry.Phone != phone)
        {
            var phoneDuplicates = await enquiryRepository.FindAsync(
                e => e.Phone == phone && e.Id != enquiry.Id, cancellationToken);
            if (phoneDuplicates.Count > 0)
                throw new ConflictException("An enquiry with this phone number already exists.");
        }

        var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        if (email is not null && enquiry.Email != email)
        {
            var emailDuplicates = await enquiryRepository.FindAsync(
                e => e.Email == email && e.Id != enquiry.Id, cancellationToken);
            if (emailDuplicates.Count > 0)
                throw new ConflictException("An enquiry with this email already exists.");
        }

        enquiry.FullName = request.FullName.Trim();
        enquiry.Phone = phone;
        enquiry.Email = email;
        enquiry.Source = request.Source;
        enquiry.Status = request.Status;
        enquiry.NextFollowUpDate = request.NextFollowUpDate;

        enquiryRepository.Update(enquiry);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
