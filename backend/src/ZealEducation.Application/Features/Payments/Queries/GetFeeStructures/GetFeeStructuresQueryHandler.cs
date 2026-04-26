using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Payments.Queries.GetFeeStructures;

public class GetFeeStructuresQueryHandler(
    IRepository<FeeStructure> feeRepository,
    IRepository<Candidate> candidateRepository,
    IRepository<UserAccount> userRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Course> courseRepository) : IRequestHandler<GetFeeStructuresQuery, PaginatedList<FeeStructureListItemDto>>
{
    public async Task<PaginatedList<FeeStructureListItemDto>> Handle(GetFeeStructuresQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

        var query =
            from fee in feeRepository.Query()
            join candidate in candidateRepository.Query() on fee.CandidateId equals candidate.Id
            join user in userRepository.Query() on candidate.UserAccountId equals user.Id
            join enrollment in enrollmentRepository.Query() on fee.Id equals enrollment.FeeId into enrollmentsByFee
            from enrollment in enrollmentsByFee.DefaultIfEmpty()
            join course in courseRepository.Query() on enrollment.CourseId equals course.Id into courses
            from course in courses.DefaultIfEmpty()
            select new
            {
                fee,
                candidate,
                user,
                course
            };

        if (request.Status.HasValue)
        {
            var status = request.Status.Value;
            query = query.Where(x => x.fee.PaymentStatus == status);
        }

        if (request.Type.HasValue)
        {
            var type = request.Type.Value;
            query = query.Where(x => x.fee.FeeType == type);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.candidate.CandidateCode.ToLower().Contains(search) ||
                x.user.FullName.ToLower().Contains(search) ||
                (x.course != null && x.course.CourseName.ToLower().Contains(search)));
        }

        var projected = query
            .OrderByDescending(x => x.fee.CreatedAt)
            .Select(x => new FeeStructureListItemDto
            {
                FeeId = x.fee.Id,
                CandidateId = x.candidate.Id,
                CandidateCode = x.candidate.CandidateCode,
                CandidateFullName = x.user.FullName,
                CourseTitle = x.course != null ? x.course.CourseName : null,
                FeeType = x.fee.FeeType,
                TotalFee = x.fee.TotalFee,
                AmountPaid = x.fee.AmountPaid,
                OutstandingBalance = x.fee.OutstandingBalance,
                PaymentStatus = x.fee.PaymentStatus,
                PaymentType = x.fee.PaymentType,
                CreatedAt = x.fee.CreatedAt
            });

        return await PaginatedList<FeeStructureListItemDto>.CreateAsync(projected, page, pageSize);
    }
}
