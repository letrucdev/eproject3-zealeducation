using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.CourseEnquiries.Queries.GetEnquiryById;

public class GetEnquiryByIdQueryHandler(
    IRepository<CourseEnquiry> enquiryRepository,
    IRepository<EnquiryNote> noteRepository,
    IRepository<Staff> staffRepository,
    IRepository<UserAccount> userRepository,
    IRepository<Candidate> candidateRepository,
    IRepository<Course> courseRepository) : IRequestHandler<GetEnquiryByIdQuery, CourseEnquiryDetailDto>
{
    public async Task<CourseEnquiryDetailDto> Handle(GetEnquiryByIdQuery request, CancellationToken cancellationToken)
    {
        var enquiries = enquiryRepository.Query();
        var staffs = staffRepository.Query();
        var users = userRepository.Query();
        var courses = courseRepository.Query();

        var detail = await (from e in enquiries
                            where e.Id == request.EnquiryId
                            join s in staffs on e.AssignedCounselorId equals s.Id
                            join u in users on s.UserAccountId equals u.Id
                            join c in courses on e.CourseInterestedId equals c.Id
                            select new CourseEnquiryDetailDto
                            {
                                EnquiryId = e.Id,
                                FullName = e.FullName,
                                Phone = e.Phone,
                                Email = e.Email,
                                CourseInterestedId = e.CourseInterestedId,
                                CourseInterestedName = c.CourseName,
                                Source = e.Source,
                                Status = e.Status,
                                NextFollowUpDate = e.NextFollowUpDate,
                                AssignedCounselorId = e.AssignedCounselorId,
                                AssignedCounselorName = u.FullName,
                                ConvertedCandidateId = e.ConvertedCandidateId,
                                ConvertedAt = e.ConvertedAt,
                                CreatedAt = e.CreatedAt,
                                UpdatedAt = e.UpdatedAt
                            }).FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(CourseEnquiry), request.EnquiryId);

        if (detail.ConvertedCandidateId is Guid candidateId)
        {
            var candidates = candidateRepository.Query();
            detail.ConvertedCandidateCode = await candidates
                .Where(c => c.Id == candidateId)
                .Select(c => c.CandidateCode)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var notes = noteRepository.Query();
        var notesQuery = from n in notes
                         where n.EnquiryId == request.EnquiryId
                         join s in staffs on n.AuthorStaffId equals s.Id
                         join u in users on s.UserAccountId equals u.Id
                         orderby n.CreatedAt descending
                         select new EnquiryNoteDto
                         {
                             NoteId = n.Id,
                             AuthorStaffId = n.AuthorStaffId,
                             AuthorFullName = u.FullName,
                             Content = n.Content,
                             CreatedAt = n.CreatedAt
                         };

        detail.Notes = await notesQuery.ToListAsync(cancellationToken);
        return detail;
    }
}
