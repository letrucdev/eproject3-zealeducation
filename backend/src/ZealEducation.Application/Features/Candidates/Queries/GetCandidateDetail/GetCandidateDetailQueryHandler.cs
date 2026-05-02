using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Candidates.Queries.GetCandidateDetail;

public class GetCandidateDetailQueryHandler(
    IRepository<Candidate> candidateRepository,
    IRepository<UserAccount> userRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Course> courseRepository,
    IRepository<Batch> batchRepository,
    IRepository<FeeStructure> feeRepository) : IRequestHandler<GetCandidateDetailQuery, CandidateDetailDto>
{
    public async Task<CandidateDetailDto> Handle(GetCandidateDetailQuery request, CancellationToken cancellationToken)
    {
        var candidate = await candidateRepository.GetByIdAsync(request.CandidateId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidate), request.CandidateId);

        var user = await userRepository.GetByIdAsync(candidate.UserAccountId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserAccount), candidate.UserAccountId);

        var enrollments = await (
            from e in enrollmentRepository.Query()
            join c in courseRepository.Query() on e.CourseId equals c.Id
            join b in batchRepository.Query() on e.BatchId equals b.Id into batches
            from b in batches.DefaultIfEmpty()
            where e.CandidateId == candidate.Id
            orderby e.EnrollmentDate descending
            select new CandidateEnrollmentDto
            {
                EnrollmentId = e.Id,
                CourseId = c.Id,
                CourseName = c.CourseName,
                DurationWeeks = c.DurationWeeks,
                BatchId = e.BatchId,
                BatchCode = b != null ? b.BatchCode : null,
                BatchStartDate = b != null ? (DateOnly?)b.StartDate : null,
                BatchEndDate = b != null ? (DateOnly?)b.EndDate : null,
                EnrollmentDate = e.EnrollmentDate,
                Status = e.Status,
                FeeId = e.FeeId,
                Notes = e.Notes
            }).ToListAsync(cancellationToken);

        var feeStructures = await (
            from fee in feeRepository.Query()
            join enrollment in enrollmentRepository.Query() on fee.Id equals enrollment.FeeId into enrollmentsByFee
            from enrollment in enrollmentsByFee.DefaultIfEmpty()
            join course in courseRepository.Query() on enrollment.CourseId equals course.Id into courses
            from course in courses.DefaultIfEmpty()
            where fee.CandidateId == candidate.Id
            orderby fee.CreatedAt descending
            select new CandidateFeeStructureSummaryDto
            {
                FeeId = fee.Id,
                FeeType = fee.FeeType,
                TotalFee = fee.TotalFee,
                AmountPaid = fee.AmountPaid,
                OutstandingBalance = fee.OutstandingBalance,
                PaymentStatus = fee.PaymentStatus,
                PaymentType = fee.PaymentType,
                CourseTitle = course != null ? course.CourseName : null,
                Notes = fee.Notes,
                CreatedAt = fee.CreatedAt
            }).ToListAsync(cancellationToken);

        return new CandidateDetailDto
        {
            CandidateId = candidate.Id,
            CandidateCode = candidate.CandidateCode,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Dob = user.Dob,
            Gender = user.Gender,
            IsActive = user.IsActive,
            Address = candidate.Address,
            EmergencyContact = candidate.EmergencyContact,
            Notes = candidate.Notes,
            Status = candidate.Status,
            RegisteredAt = candidate.RegisteredAt,
            Enrollments = enrollments,
            FeeStructures = feeStructures
        };
    }
}
