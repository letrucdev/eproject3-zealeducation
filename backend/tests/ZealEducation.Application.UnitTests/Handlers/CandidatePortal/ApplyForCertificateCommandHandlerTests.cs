using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.CandidatePortal.Commands.ApplyForCertificate;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.CandidatePortal;

public class ApplyForCertificateCommandHandlerTests
{
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IRepository<Candidate>> _candidateRepo = new();
    private readonly Mock<IRepository<Enrollment>> _enrollmentRepo = new();
    private readonly Mock<IRepository<FeeStructure>> _feeRepo = new();
    private readonly Mock<IRepository<AttendanceRecord>> _attendanceRepo = new();
    private readonly Mock<IRepository<ClassSession>> _sessionRepo = new();
    private readonly Mock<IRepository<ExamResult>> _examRepo = new();
    private readonly Mock<IRepository<CertificateApplication>> _applicationRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private ApplyForCertificateCommandHandler CreateHandler() => new(
        _currentUser.Object,
        _candidateRepo.Object,
        _enrollmentRepo.Object,
        _feeRepo.Object,
        _attendanceRepo.Object,
        _sessionRepo.Object,
        _examRepo.Object,
        _applicationRepo.Object,
        _uow.Object);

    private static Candidate NewCandidate(Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        UserAccountId = userId,
        CandidateCode = "C-001",
        Status = CandidateStatus.Active,
        RegisteredAt = DateTime.UtcNow
    };

    private static Enrollment NewEnrollment(Guid candidateId, Guid batchId, Guid? feeId = null) => new()
    {
        Id = Guid.NewGuid(),
        CandidateId = candidateId,
        BatchId = batchId,
        FeeId = feeId,
        CourseId = Guid.NewGuid(),
        EnrollmentDate = new DateOnly(2030, 1, 1),
        Status = EnrollmentStatus.Enrolled
    };

    private void SetupEnrollmentQuery(IEnumerable<Enrollment> data) =>
        _enrollmentRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<Enrollment>(data));

    private void SetupApplicationQuery(IEnumerable<CertificateApplication> data) =>
        _applicationRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<CertificateApplication>(data));

    private void SetupAttendanceQuery(IEnumerable<AttendanceRecord> data) =>
        _attendanceRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<AttendanceRecord>(data));

    private void SetupSessionQuery(IEnumerable<ClassSession> data) =>
        _sessionRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<ClassSession>(data));

    private void SetupExamQuery(IEnumerable<ExamResult> data) =>
        _examRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<ExamResult>(data));

    [Fact]
    public async Task Throws_UnauthorizedException_when_current_user_is_null()
    {
        _currentUser.SetupGet(u => u.UserId).Returns((Guid?)null);

        var act = async () => await CreateHandler().Handle(
            new ApplyForCertificateCommand(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<UnauthorizedException>();
        _applicationRepo.Verify(r => r.AddAsync(It.IsAny<CertificateApplication>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_NotFoundException_when_candidate_profile_is_missing()
    {
        var userId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _candidateRepo.SetupFind(Array.Empty<Candidate>());

        var act = async () => await CreateHandler().Handle(
            new ApplyForCertificateCommand(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("No candidate profile is associated with this account.");
    }

    [Fact]
    public async Task Throws_NotFoundException_when_candidate_is_not_enrolled_in_the_batch()
    {
        var userId = Guid.NewGuid();
        var candidate = NewCandidate(userId);
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _candidateRepo.SetupFind(new[] { candidate });
        SetupEnrollmentQuery(Array.Empty<Enrollment>());

        var act = async () => await CreateHandler().Handle(
            new ApplyForCertificateCommand(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("You are not enrolled in this batch.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_application_already_exists_for_enrollment()
    {
        var userId = Guid.NewGuid();
        var candidate = NewCandidate(userId);
        var batchId = Guid.NewGuid();
        var enrollment = NewEnrollment(candidate.Id, batchId);
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _candidateRepo.SetupFind(new[] { candidate });
        SetupEnrollmentQuery(new[] { enrollment });
        SetupApplicationQuery(new[]
        {
            new CertificateApplication { Id = Guid.NewGuid(), EnrollmentId = enrollment.Id }
        });

        var act = async () => await CreateHandler().Handle(
            new ApplyForCertificateCommand(batchId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("A certificate application already exists for this enrollment.");
        _applicationRepo.Verify(r => r.AddAsync(It.IsAny<CertificateApplication>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_not_eligible_for_certificate()
    {
        var userId = Guid.NewGuid();
        var candidate = NewCandidate(userId);
        var batchId = Guid.NewGuid();
        var enrollment = NewEnrollment(candidate.Id, batchId);
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _candidateRepo.SetupFind(new[] { candidate });
        SetupEnrollmentQuery(new[] { enrollment });
        SetupApplicationQuery(Array.Empty<CertificateApplication>());
        SetupAttendanceQuery(Array.Empty<AttendanceRecord>());
        SetupSessionQuery(Array.Empty<ClassSession>());
        SetupExamQuery(Array.Empty<ExamResult>());

        var act = async () => await CreateHandler().Handle(
            new ApplyForCertificateCommand(batchId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .Where(e => e.Message.StartsWith("Not eligible:"));
        _applicationRepo.Verify(r => r.AddAsync(It.IsAny<CertificateApplication>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Creates_application_with_pending_status_when_eligible()
    {
        var userId = Guid.NewGuid();
        var candidate = NewCandidate(userId);
        var batchId = Guid.NewGuid();
        var feeId = Guid.NewGuid();
        var enrollment = NewEnrollment(candidate.Id, batchId, feeId);

        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _candidateRepo.SetupFind(new[] { candidate });
        SetupEnrollmentQuery(new[] { enrollment });
        SetupApplicationQuery(Array.Empty<CertificateApplication>());

        _feeRepo.SetupGetById(feeId, new FeeStructure
        {
            Id = feeId,
            TotalFee = 1000m,
            AmountPaid = 1000m,
            OutstandingBalance = 0m,
            PaymentStatus = PaymentStatus.Paid
        });

        var presentRecords = Enumerable.Range(0, 8)
            .Select(_ => new AttendanceRecord
            {
                Id = Guid.NewGuid(),
                EnrollmentId = enrollment.Id,
                Status = AttendanceStatus.Present
            })
            .ToList();
        SetupAttendanceQuery(presentRecords);

        var sessions = Enumerable.Range(0, 10)
            .Select(_ => new ClassSession { Id = Guid.NewGuid(), BatchId = batchId })
            .ToList();
        SetupSessionQuery(sessions);

        SetupExamQuery(new[]
        {
            new ExamResult
            {
                Id = Guid.NewGuid(),
                EnrollmentId = enrollment.Id,
                IsFinalized = true,
                IsPassed = true
            }
        });

        CertificateApplication? captured = null;
        _applicationRepo.Setup(r => r.AddAsync(It.IsAny<CertificateApplication>(), It.IsAny<CancellationToken>()))
            .Callback<CertificateApplication, CancellationToken>((a, _) => captured = a)
            .ReturnsAsync((CertificateApplication a, CancellationToken _) => a);

        var resultId = await CreateHandler().Handle(new ApplyForCertificateCommand(batchId), default);

        captured.Should().NotBeNull();
        captured!.Id.Should().NotBe(Guid.Empty);
        resultId.Should().Be(captured.Id);
        captured.EnrollmentId.Should().Be(enrollment.Id);
        captured.Status.Should().Be(CertificateApplicationStatus.Pending);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
