using System.Security.Cryptography;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Helpers;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.CertificateApplications.Commands.ApproveCertificateApplication;

public class ApproveCertificateApplicationCommandHandler(
    IRepository<CertificateApplication> applicationRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<FeeStructure> feeRepository,
    IRepository<AttendanceRecord> attendanceRepository,
    IRepository<ClassSession> sessionRepository,
    IRepository<ExamResult> examRepository,
    IRepository<Staff> staffRepository,
    ICurrentUser currentUser,
    ICertificateArchiver archiver,
    IUnitOfWork unitOfWork) : IRequestHandler<ApproveCertificateApplicationCommand, ApproveCertificateApplicationResponse>
{
    public async Task<ApproveCertificateApplicationResponse> Handle(
        ApproveCertificateApplicationCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedException("Current user could not be resolved.");

        var staff = (await staffRepository.FindAsync(s => s.UserAccountId == userId, cancellationToken)).FirstOrDefault()
            ?? throw new ConflictException("Current user is not registered as staff.");

        var application = await applicationRepository.GetByIdAsync(request.ApplicationId, cancellationToken)
            ?? throw new NotFoundException(nameof(CertificateApplication), request.ApplicationId);

        if (application.Status == CertificateApplicationStatus.Approved)
            throw new ConflictException("Certificate application has already been approved.");

        var enrollment = await enrollmentRepository.GetByIdAsync(application.EnrollmentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Enrollment), application.EnrollmentId);

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

        var eligibility = CertificateEligibilityChecker.Evaluate(fee, presentCount, totalSessions, examResults);
        if (!eligibility.IsEligible)
            throw new ConflictException(eligibility.Reason ?? "Candidate is not currently eligible.");

        var certificateNumber = await GenerateUniqueCertificateNumberAsync(cancellationToken);
        var approvedAt = DateTime.UtcNow;

        application.Status = CertificateApplicationStatus.Approved;
        application.CertificateNumber = certificateNumber;
        application.ApprovedByStaffId = staff.Id;
        application.ApprovedAt = approvedAt;

        applicationRepository.Update(application);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate PDF and persist file path. Archiver also calls SaveChangesAsync.
        await archiver.GenerateAndUploadAsync(application.Id, cancellationToken);

        return new ApproveCertificateApplicationResponse(application.Id, certificateNumber);
    }

    private async Task<string> GenerateUniqueCertificateNumberAsync(CancellationToken cancellationToken)
    {
        var prefix = $"CERT-{DateTime.UtcNow:yyyyMMdd}-";
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var seq = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var candidate = prefix + seq;
            var existing = await applicationRepository.FindAsync(a => a.CertificateNumber == candidate, cancellationToken);
            if (existing.Count == 0) return candidate;
        }
        throw new ConflictException("Could not generate a unique certificate number; please try again.");
    }
}
