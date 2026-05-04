using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.CertificateApplications.Commands.ApproveCertificateApplication;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.CertificateApplications;

public class ApproveCertificateApplicationCommandHandlerTests
{
    private readonly Mock<IRepository<CertificateApplication>> _applicationRepo = new();
    private readonly Mock<IRepository<Enrollment>> _enrollmentRepo = new();
    private readonly Mock<IRepository<FeeStructure>> _feeRepo = new();
    private readonly Mock<IRepository<AttendanceRecord>> _attendanceRepo = new();
    private readonly Mock<IRepository<ClassSession>> _sessionRepo = new();
    private readonly Mock<IRepository<ExamResult>> _examRepo = new();
    private readonly Mock<IRepository<Staff>> _staffRepo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<ICertificateArchiver> _archiver = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private ApproveCertificateApplicationCommandHandler CreateHandler() => new(
        _applicationRepo.Object,
        _enrollmentRepo.Object,
        _feeRepo.Object,
        _attendanceRepo.Object,
        _sessionRepo.Object,
        _examRepo.Object,
        _staffRepo.Object,
        _currentUser.Object,
        _archiver.Object,
        _uow.Object);

    private void SetupAttendanceQuery(IEnumerable<AttendanceRecord> data) =>
        _attendanceRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<AttendanceRecord>(data));

    private void SetupSessionQuery(IEnumerable<ClassSession> data) =>
        _sessionRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<ClassSession>(data));

    private void SetupExamQuery(IEnumerable<ExamResult> data) =>
        _examRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<ExamResult>(data));

    private static Staff NewStaff(Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        UserAccountId = userId,
        Position = "Lecturer",
        Department = "CS",
        JoinedDate = new DateOnly(2020, 1, 1),
        IsActive = true
    };

    private static Enrollment NewEnrollment(Guid? feeId = null, Guid? batchId = null) => new()
    {
        Id = Guid.NewGuid(),
        CandidateId = Guid.NewGuid(),
        CourseId = Guid.NewGuid(),
        FeeId = feeId,
        BatchId = batchId,
        EnrollmentDate = new DateOnly(2030, 1, 1),
        Status = EnrollmentStatus.Enrolled
    };

    private static FeeStructure PaidFee(Guid feeId) => new()
    {
        Id = feeId,
        TotalFee = 1000m,
        AmountPaid = 1000m,
        OutstandingBalance = 0m,
        PaymentStatus = PaymentStatus.Paid,
        PaymentType = PaymentType.FullPayment
    };

    [Fact]
    public async Task Throws_UnauthorizedException_when_current_user_is_null()
    {
        _currentUser.SetupGet(u => u.UserId).Returns((Guid?)null);

        var act = async () => await CreateHandler().Handle(
            new ApproveCertificateApplicationCommand(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<UnauthorizedException>();
        _applicationRepo.Verify(r => r.Update(It.IsAny<CertificateApplication>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_current_user_is_not_staff()
    {
        var userId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _staffRepo.SetupFind(Array.Empty<Staff>());

        var act = async () => await CreateHandler().Handle(
            new ApproveCertificateApplicationCommand(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Current user is not registered as staff.");
    }

    [Fact]
    public async Task Throws_NotFoundException_when_application_does_not_exist()
    {
        var userId = Guid.NewGuid();
        var applicationId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _staffRepo.SetupFind(new[] { NewStaff(userId) });
        _applicationRepo.SetupGetById(applicationId, null);

        var act = async () => await CreateHandler().Handle(
            new ApproveCertificateApplicationCommand(applicationId), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Throws_ConflictException_when_application_already_approved()
    {
        var userId = Guid.NewGuid();
        var applicationId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _staffRepo.SetupFind(new[] { NewStaff(userId) });
        _applicationRepo.SetupGetById(applicationId, new CertificateApplication
        {
            Id = applicationId,
            EnrollmentId = Guid.NewGuid(),
            Status = CertificateApplicationStatus.Approved,
            CertificateNumber = "CERT-EXISTING"
        });

        var act = async () => await CreateHandler().Handle(
            new ApproveCertificateApplicationCommand(applicationId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Certificate application has already been approved.");
    }

    [Fact]
    public async Task Throws_NotFoundException_when_enrollment_does_not_exist()
    {
        var userId = Guid.NewGuid();
        var applicationId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _staffRepo.SetupFind(new[] { NewStaff(userId) });
        _applicationRepo.SetupGetById(applicationId, new CertificateApplication
        {
            Id = applicationId,
            EnrollmentId = enrollmentId,
            Status = CertificateApplicationStatus.Pending
        });
        _enrollmentRepo.SetupGetById(enrollmentId, null);

        var act = async () => await CreateHandler().Handle(
            new ApproveCertificateApplicationCommand(applicationId), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Throws_ConflictException_when_candidate_is_not_eligible()
    {
        var userId = Guid.NewGuid();
        var applicationId = Guid.NewGuid();
        var enrollment = NewEnrollment(feeId: null, batchId: Guid.NewGuid());
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _staffRepo.SetupFind(new[] { NewStaff(userId) });
        _applicationRepo.SetupGetById(applicationId, new CertificateApplication
        {
            Id = applicationId,
            EnrollmentId = enrollment.Id,
            Status = CertificateApplicationStatus.Pending
        });
        _enrollmentRepo.SetupGetById(enrollment.Id, enrollment);
        SetupAttendanceQuery(Array.Empty<AttendanceRecord>());
        SetupSessionQuery(Array.Empty<ClassSession>());
        SetupExamQuery(Array.Empty<ExamResult>());

        var act = async () => await CreateHandler().Handle(
            new ApproveCertificateApplicationCommand(applicationId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .Where(e => e.Message.StartsWith("Not eligible:"));
        _applicationRepo.Verify(r => r.Update(It.IsAny<CertificateApplication>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Approves_application_and_assigns_certificate_number_when_eligible()
    {
        var userId = Guid.NewGuid();
        var staff = NewStaff(userId);
        var applicationId = Guid.NewGuid();
        var feeId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        var enrollment = NewEnrollment(feeId: feeId, batchId: batchId);

        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _staffRepo.SetupFind(new[] { staff });

        var application = new CertificateApplication
        {
            Id = applicationId,
            EnrollmentId = enrollment.Id,
            Status = CertificateApplicationStatus.Pending
        };
        _applicationRepo.SetupGetById(applicationId, application);
        _enrollmentRepo.SetupGetById(enrollment.Id, enrollment);
        _feeRepo.SetupGetById(feeId, PaidFee(feeId));

        var presentRecords = Enumerable.Range(0, 8)
            .Select(_ => new AttendanceRecord
            {
                Id = Guid.NewGuid(),
                ClassSessionId = Guid.NewGuid(),
                EnrollmentId = enrollment.Id,
                Status = AttendanceStatus.Present
            })
            .ToList();
        SetupAttendanceQuery(presentRecords);

        var sessions = Enumerable.Range(0, 10)
            .Select(_ => new ClassSession
            {
                Id = Guid.NewGuid(),
                BatchId = batchId,
                SessionDate = new DateOnly(2030, 2, 1),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(11, 0)
            })
            .ToList();
        SetupSessionQuery(sessions);

        SetupExamQuery(new[]
        {
            new ExamResult
            {
                Id = Guid.NewGuid(),
                EnrollmentId = enrollment.Id,
                ExamId = Guid.NewGuid(),
                IsFinalized = true,
                IsPassed = true,
                Score = 80,
                GradedById = staff.Id
            }
        });

        _applicationRepo.SetupFind(Array.Empty<CertificateApplication>());

        _archiver.Setup(a => a.GenerateAndUploadAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
            .ReturnsAsync(new byte[] { 1, 2, 3 });

        var response = await CreateHandler().Handle(
            new ApproveCertificateApplicationCommand(applicationId), default);

        response.ApplicationId.Should().Be(applicationId);
        response.CertificateNumber.Should().StartWith("CERT-");
        application.Status.Should().Be(CertificateApplicationStatus.Approved);
        application.CertificateNumber.Should().Be(response.CertificateNumber);
        application.ApprovedByStaffId.Should().Be(staff.Id);
        application.ApprovedAt.Should().NotBeNull();

        _applicationRepo.Verify(r => r.Update(application), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _archiver.Verify(a => a.GenerateAndUploadAsync(
            applicationId, It.IsAny<CancellationToken>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task Throws_ConflictException_when_unique_certificate_number_cannot_be_generated()
    {
        var userId = Guid.NewGuid();
        var staff = NewStaff(userId);
        var applicationId = Guid.NewGuid();
        var feeId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        var enrollment = NewEnrollment(feeId: feeId, batchId: batchId);

        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _staffRepo.SetupFind(new[] { staff });

        var application = new CertificateApplication
        {
            Id = applicationId,
            EnrollmentId = enrollment.Id,
            Status = CertificateApplicationStatus.Pending
        };
        _applicationRepo.SetupGetById(applicationId, application);
        _enrollmentRepo.SetupGetById(enrollment.Id, enrollment);
        _feeRepo.SetupGetById(feeId, PaidFee(feeId));

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

        _applicationRepo.SetupFind(new[]
        {
            new CertificateApplication
            {
                Id = Guid.NewGuid(),
                EnrollmentId = Guid.NewGuid(),
                CertificateNumber = "EXISTING"
            }
        });

        var act = async () => await CreateHandler().Handle(
            new ApproveCertificateApplicationCommand(applicationId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Could not generate a unique certificate number; please try again.");
    }
}
