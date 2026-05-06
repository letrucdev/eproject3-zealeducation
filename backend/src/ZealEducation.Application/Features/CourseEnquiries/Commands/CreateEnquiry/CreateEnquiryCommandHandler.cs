using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.CourseEnquiries.Commands.CreateEnquiry;

public class CreateEnquiryCommandHandler(
    IRepository<CourseEnquiry> enquiryRepository,
    IRepository<Staff> staffRepository,
    IRepository<Course> courseRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : IRequestHandler<CreateEnquiryCommand, CreateEnquiryResponse>
{
    public async Task<CreateEnquiryResponse> Handle(CreateEnquiryCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var staffs = await staffRepository.FindAsync(s => s.UserAccountId == userId, cancellationToken);
        var staff = staffs.FirstOrDefault()
            ?? throw new UnauthorizedException("Current user is not linked to a staff profile.");

        var course = await courseRepository.GetByIdAsync(request.CourseInterestedId, cancellationToken)
            ?? throw new NotFoundException(nameof(Course), request.CourseInterestedId);

        if (!course.IsActive)
            throw new ConflictException("The selected course is inactive and no longer accepting enquiries.");

        var phone = request.Phone.Trim();
        var phoneDuplicates = await enquiryRepository.FindAsync(e => e.Phone == phone, cancellationToken);
        if (phoneDuplicates.Count > 0)
            throw new ConflictException("An enquiry with this phone number already exists.");

        var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        if (email is not null)
        {
            var emailDuplicates = await enquiryRepository.FindAsync(e => e.Email == email, cancellationToken);
            if (emailDuplicates.Count > 0)
                throw new ConflictException("An enquiry with this email already exists.");
        }

        var enquiry = new CourseEnquiry
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Phone = phone,
            Email = email,
            CourseInterestedId = course.Id,
            Source = request.Source,
            Status = request.Status,
            NextFollowUpDate = request.NextFollowUpDate,
            AssignedCounselorId = staff.Id
        };

        await enquiryRepository.AddAsync(enquiry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateEnquiryResponse
        {
            EnquiryId = enquiry.Id,
            FullName = enquiry.FullName,
            Phone = enquiry.Phone,
            CourseInterestedId = course.Id,
            CourseInterestedName = course.CourseName
        };
    }
}
