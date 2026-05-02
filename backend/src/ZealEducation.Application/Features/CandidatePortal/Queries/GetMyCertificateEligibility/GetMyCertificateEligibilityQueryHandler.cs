using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Helpers;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.CandidatePortal.Common;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyCertificateEligibility;

public class GetMyCertificateEligibilityQueryHandler(
    ICurrentUser currentUser,
    IRepository<Candidate> candidateRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<FeeStructure> feeRepository,
    IRepository<AttendanceRecord> attendanceRepository,
    IRepository<ClassSession> sessionRepository,
    IRepository<ExamResult> examRepository,
    IRepository<CertificateApplication> applicationRepository) : IRequestHandler<GetMyCertificateEligibilityQuery, MyCertificateEligibilityDto>
{
    public async Task<MyCertificateEligibilityDto> Handle(GetMyCertificateEligibilityQuery request, CancellationToken cancellationToken)
    {
        var candidate = await CandidateResolver.ResolveAsync(currentUser, candidateRepository, cancellationToken);

        var enrollment = await enrollmentRepository.Query()
            .FirstOrDefaultAsync(e => e.CandidateId == candidate.Id && e.BatchId == request.BatchId, cancellationToken)
            ?? throw new NotFoundException("You are not enrolled in this batch.");

        var fee = enrollment.FeeId.HasValue
            ? await feeRepository.GetByIdAsync(enrollment.FeeId.Value, cancellationToken)
            : null;

        var presentCount = await attendanceRepository.Query()
            .CountAsync(a => a.EnrollmentId == enrollment.Id && a.Status == AttendanceStatus.Present, cancellationToken);

        var totalSessions = enrollment.BatchId.HasValue
            ? await sessionRepository.Query().CountAsync(s => s.BatchId == enrollment.BatchId.Value, cancellationToken)
            : 0;

        var examResults = await examRepository.Query()
            .Where(r => r.EnrollmentId == enrollment.Id)
            .ToListAsync(cancellationToken);

        var result = CertificateEligibilityChecker.Evaluate(fee, presentCount, totalSessions, examResults);

        var existing = await applicationRepository.Query()
            .Where(a => a.EnrollmentId == enrollment.Id)
            .Select(a => (CertificateApplicationStatus?)a.Status)
            .FirstOrDefaultAsync(cancellationToken);

        return new MyCertificateEligibilityDto
        {
            BatchId = request.BatchId,
            IsEligible = result.IsEligible,
            FeesPaid = result.FeesPaid,
            AttendancePercent = result.AttendancePercent,
            AttendanceOk = result.AttendanceOk,
            ExamsPassed = result.ExamsPassed,
            Reason = result.Reason,
            ExistingApplicationStatus = existing,
        };
    }
}
