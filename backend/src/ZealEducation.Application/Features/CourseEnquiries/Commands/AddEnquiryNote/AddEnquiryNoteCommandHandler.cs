using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.CourseEnquiries.Commands.AddEnquiryNote;

public class AddEnquiryNoteCommandHandler(
    IRepository<CourseEnquiry> enquiryRepository,
    IRepository<EnquiryNote> noteRepository,
    IRepository<Staff> staffRepository,
    IRepository<UserAccount> userRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : IRequestHandler<AddEnquiryNoteCommand, AddEnquiryNoteResponse>
{
    public async Task<AddEnquiryNoteResponse> Handle(AddEnquiryNoteCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var enquiry = await enquiryRepository.GetByIdAsync(request.EnquiryId, cancellationToken)
            ?? throw new NotFoundException(nameof(CourseEnquiry), request.EnquiryId);

        var staffs = await staffRepository.FindAsync(s => s.UserAccountId == userId, cancellationToken);
        var staff = staffs.FirstOrDefault()
            ?? throw new UnauthorizedException("Current user is not linked to a staff profile.");

        var user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new UnauthorizedException("Current user account not found.");

        var note = new EnquiryNote
        {
            Id = Guid.NewGuid(),
            EnquiryId = enquiry.Id,
            AuthorStaffId = staff.Id,
            Content = request.Content.Trim()
        };

        await noteRepository.AddAsync(note, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AddEnquiryNoteResponse
        {
            NoteId = note.Id,
            EnquiryId = enquiry.Id,
            AuthorStaffId = staff.Id,
            AuthorFullName = user.FullName,
            Content = note.Content,
            CreatedAt = note.CreatedAt
        };
    }
}
