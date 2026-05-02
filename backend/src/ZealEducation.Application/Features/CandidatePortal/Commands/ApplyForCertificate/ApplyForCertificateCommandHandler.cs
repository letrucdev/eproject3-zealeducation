using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Helpers;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.CandidatePortal.Common;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.CandidatePortal.Commands.ApplyForCertificate;

public class ApplyForCertificateCommandHandler(
    ICurrentUser currentUser,
    IRepository<Candidate> candidateRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<FeeStructure> feeRepository,
    IRepository<AttendanceRecord> attendanceRepository,
    IRepository<ClassSession> sessionRepository,
    IRepository<ExamResult> examRepository,
    IRepository<CertificateApplication> applicationRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ApplyForCertificateCommand, Guid>
{
    public async Task<Guid> Handle(ApplyForCertificateCommand request, CancellationToken cancellationToken)
    {
        var candidate = await CandidateResolver.ResolveAsync(currentUser, candidateRepository, cancellationToken);

        var enrollment = await enrollmentRepository.Query()
            .FirstOrDefaultAsync(e => e.CandidateId == candidate.Id && e.BatchId == request.BatchId, cancellationToken)
            ?? throw new NotFoundException("You are not enrolled in this batch.");

        var existing = await applicationRepository.Query()
            .AnyAsync(a => a.EnrollmentId == enrollment.Id, cancellationToken);
        if (existing)
            throw new ConflictException("A certificate application already exists for this enrollment.");

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
        if (!result.IsEligible)
            throw new ConflictException(result.Reason ?? "Not eligible for a certificate.");

        var application = new CertificateApplication
        {
            Id = Guid.NewGuid(),
            EnrollmentId = enrollment.Id,
            Status = CertificateApplicationStatus.Pending,
        };

        await applicationRepository.AddAsync(application, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return application.Id;
    }
}
