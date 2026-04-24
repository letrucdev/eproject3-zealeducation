using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.CourseEnquiries.Queries.GetEnquiries;

public class GetEnquiriesQueryHandler(
    IRepository<CourseEnquiry> enquiryRepository,
    IRepository<Staff> staffRepository,
    IRepository<UserAccount> userRepository,
    IRepository<Course> courseRepository) : IRequestHandler<GetEnquiriesQuery, PaginatedList<CourseEnquiryListItemDto>>
{
    private static readonly EnquiryStatus[] OpenStatuses =
    [
        EnquiryStatus.New,
        EnquiryStatus.Contacted,
        EnquiryStatus.InFollowUp,
        EnquiryStatus.Interested
    ];

    public async Task<PaginatedList<CourseEnquiryListItemDto>> Handle(GetEnquiriesQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

        var enquiries = enquiryRepository.Query();
        var staffs = staffRepository.Query();
        var users = userRepository.Query();
        var courses = courseRepository.Query();

        var query = from e in enquiries
                    join s in staffs on e.AssignedCounselorId equals s.Id
                    join u in users on s.UserAccountId equals u.Id
                    join c in courses on e.CourseInterestedId equals c.Id
                    select new { Enquiry = e, CounselorName = u.FullName, c.CourseName };

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.Enquiry.FullName.ToLower().Contains(search) ||
                x.Enquiry.Phone.Contains(search) ||
                (x.Enquiry.Email != null && x.Enquiry.Email.ToLower().Contains(search)) ||
                x.CourseName.ToLower().Contains(search));
        }

        if (request.Status.HasValue)
        {
            var status = request.Status.Value;
            query = query.Where(x => x.Enquiry.Status == status);
        }

        if (request.Source.HasValue)
        {
            var source = request.Source.Value;
            query = query.Where(x => x.Enquiry.Source == source);
        }

        if (request.DueFollowUpOnly == true)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            query = query.Where(x =>
                x.Enquiry.NextFollowUpDate != null &&
                x.Enquiry.NextFollowUpDate <= today &&
                OpenStatuses.Contains(x.Enquiry.Status));
        }

        var projected = query
            .OrderBy(x => x.Enquiry.NextFollowUpDate == null ? 1 : 0)
            .ThenBy(x => x.Enquiry.NextFollowUpDate)
            .ThenByDescending(x => x.Enquiry.CreatedAt)
            .Select(x => new CourseEnquiryListItemDto
            {
                EnquiryId = x.Enquiry.Id,
                FullName = x.Enquiry.FullName,
                Phone = x.Enquiry.Phone,
                Email = x.Enquiry.Email,
                CourseInterestedId = x.Enquiry.CourseInterestedId,
                CourseInterestedName = x.CourseName,
                Source = x.Enquiry.Source,
                Status = x.Enquiry.Status,
                NextFollowUpDate = x.Enquiry.NextFollowUpDate,
                AssignedCounselorId = x.Enquiry.AssignedCounselorId,
                AssignedCounselorName = x.CounselorName,
                ConvertedCandidateId = x.Enquiry.ConvertedCandidateId,
                ConvertedAt = x.Enquiry.ConvertedAt,
                CreatedAt = x.Enquiry.CreatedAt
            });

        return await PaginatedList<CourseEnquiryListItemDto>.CreateAsync(projected, page, pageSize);
    }
}
