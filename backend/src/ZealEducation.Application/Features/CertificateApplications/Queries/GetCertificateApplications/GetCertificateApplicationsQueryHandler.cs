using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Helpers;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.CertificateApplications.Queries.GetCertificateApplications;

public class GetCertificateApplicationsQueryHandler(
    IRepository<CertificateApplication> applicationRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Candidate> candidateRepository,
    IRepository<UserAccount> userRepository,
    IRepository<Batch> batchRepository,
    IRepository<Course> courseRepository,
    IRepository<FeeStructure> feeRepository,
    IRepository<AttendanceRecord> attendanceRepository,
    IRepository<ClassSession> sessionRepository,
    IRepository<ExamResult> examRepository,
    IRepository<Staff> staffRepository) : IRequestHandler<GetCertificateApplicationsQuery, PaginatedList<CertificateApplicationListItemDto>>
{
    private record FeeSnapshot(PaymentStatus Status, decimal Outstanding);

    private record RawRow(
        Guid ApplicationId,
        Guid CandidateId,
        string CandidateCode,
        string CandidateName,
        Guid EnrollmentId,
        Guid? BatchId,
        string? BatchCode,
        Guid CourseId,
        string CourseName,
        CertificateApplicationStatus Status,
        string? CertificateNumber,
        DateTime AppliedAt,
        DateTime? ApprovedAt,
        Guid? ApprovedByStaffId,
        string? ApprovedByName,
        Guid? FeeId,
        int PresentCount,
        int TotalSessions,
        int ExamCount,
        bool ExamsAllPassed);

    public async Task<PaginatedList<CertificateApplicationListItemDto>> Handle(
        GetCertificateApplicationsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

        var query =
            from app in applicationRepository.Query()
            join e in enrollmentRepository.Query() on app.EnrollmentId equals e.Id
            join c in candidateRepository.Query() on e.CandidateId equals c.Id
            join cu in userRepository.Query() on c.UserAccountId equals cu.Id
            join co in courseRepository.Query() on e.CourseId equals co.Id
            select new { app, e, c, cu, co };

        if (request.Status.HasValue)
        {
            var status = request.Status.Value;
            query = query.Where(x => x.app.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.cu.FullName.ToLower().Contains(search) ||
                x.c.CandidateCode.ToLower().Contains(search) ||
                x.co.CourseName.ToLower().Contains(search));
        }

        var sortKey = (request.SortBy ?? string.Empty).Trim().ToLower();
        var direction = (request.SortDirection ?? "desc").Trim().ToLower();

        var ordered = (sortKey, direction) switch
        {
            ("status", "asc") => query.OrderBy(x => x.app.Status),
            ("status", _) => query.OrderByDescending(x => x.app.Status),
            ("candidatename", "asc") => query.OrderBy(x => x.cu.FullName),
            ("candidatename", _) => query.OrderByDescending(x => x.cu.FullName),
            ("coursename", "asc") => query.OrderBy(x => x.co.CourseName),
            ("coursename", _) => query.OrderByDescending(x => x.co.CourseName),
            ("approvedat", "asc") => query.OrderBy(x => x.app.ApprovedAt),
            ("approvedat", _) => query.OrderByDescending(x => x.app.ApprovedAt),
            ("createdat", "asc") => query.OrderBy(x => x.app.CreatedAt),
            _ => query.OrderByDescending(x => x.app.CreatedAt),
        };

        var rawProjection = ordered
            .ThenByDescending(x => x.app.CreatedAt)
            .Select(x => new RawRow(
                x.app.Id,
                x.c.Id,
                x.c.CandidateCode,
                x.cu.FullName,
                x.e.Id,
                x.e.BatchId,
                batchRepository.Query()
                    .Where(b => x.e.BatchId != null && b.Id == x.e.BatchId)
                    .Select(b => b.BatchCode)
                    .FirstOrDefault(),
                x.co.Id,
                x.co.CourseName,
                x.app.Status,
                x.app.CertificateNumber,
                x.app.CreatedAt,
                x.app.ApprovedAt,
                x.app.ApprovedByStaffId,
                x.app.ApprovedByStaffId == null ? null :
                    (from s in staffRepository.Query()
                     join u in userRepository.Query() on s.UserAccountId equals u.Id
                     where s.Id == x.app.ApprovedByStaffId
                     select u.FullName).FirstOrDefault(),
                x.e.FeeId,
                attendanceRepository.Query()
                    .Count(a => a.EnrollmentId == x.e.Id && a.Status == AttendanceStatus.Present),
                x.e.BatchId == null ? 0 :
                    sessionRepository.Query().Count(s => s.BatchId == x.e.BatchId),
                examRepository.Query().Count(r => r.EnrollmentId == x.e.Id),
                examRepository.Query().Count(r => r.EnrollmentId == x.e.Id) > 0
                    && examRepository.Query()
                        .Where(r => r.EnrollmentId == x.e.Id)
                        .All(r => r.IsFinalized && r.IsPassed)
            ));

        var paged = await PaginatedList<RawRow>.CreateAsync(rawProjection, page, pageSize);

        var feeIds = paged.Items
            .Where(r => r.FeeId.HasValue)
            .Select(r => r.FeeId!.Value)
            .Distinct()
            .ToList();
        var feeMap = feeIds.Count == 0
            ? new Dictionary<Guid, FeeSnapshot>()
            : await feeRepository.Query()
                .Where(f => feeIds.Contains(f.Id))
                .ToDictionaryAsync(
                    f => f.Id,
                    f => new FeeSnapshot(f.PaymentStatus, f.OutstandingBalance),
                    cancellationToken);

        var items = paged.Items.Select(r =>
        {
            var feesPaid = r.FeeId.HasValue
                && feeMap.TryGetValue(r.FeeId.Value, out var fs)
                && fs.Status == PaymentStatus.Paid
                && fs.Outstanding == 0m;

            var attendancePercent = r.TotalSessions > 0
                ? Math.Round((decimal)r.PresentCount / r.TotalSessions * 100m, 2)
                : 0m;
            var attendanceOk = attendancePercent >= CertificateEligibilityChecker.MinAttendancePercent;

            var isCurrentlyEligible = feesPaid && attendanceOk && r.ExamsAllPassed;

            return new CertificateApplicationListItemDto
            {
                ApplicationId = r.ApplicationId,
                CandidateId = r.CandidateId,
                CandidateCode = r.CandidateCode,
                CandidateName = r.CandidateName,
                BatchId = r.BatchId,
                BatchCode = r.BatchCode,
                CourseId = r.CourseId,
                CourseName = r.CourseName,
                Status = r.Status,
                CertificateNumber = r.CertificateNumber,
                AppliedAt = r.AppliedAt,
                ApprovedAt = r.ApprovedAt,
                ApprovedByStaffId = r.ApprovedByStaffId,
                ApprovedByName = r.ApprovedByName,
                FeesPaid = feesPaid,
                AttendancePercent = attendancePercent,
                AttendanceOk = attendanceOk,
                ExamsPassed = r.ExamsAllPassed,
                IsCurrentlyEligible = isCurrentlyEligible,
            };
        }).ToList();

        return new PaginatedList<CertificateApplicationListItemDto>(items, paged.TotalCount, paged.PageNumber, pageSize);
    }
}
