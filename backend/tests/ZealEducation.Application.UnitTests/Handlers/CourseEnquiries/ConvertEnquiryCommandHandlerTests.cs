using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.CourseEnquiries.Commands.ConvertEnquiry;
using ZealEducation.Application.Features.CourseEnquiries.Notifications;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.CourseEnquiries;

public class ConvertEnquiryCommandHandlerTests
{
    private readonly Mock<IRepository<CourseEnquiry>> _enquiryRepo = new();
    private readonly Mock<IRepository<UserAccount>> _userRepo = new();
    private readonly Mock<IRepository<Candidate>> _candidateRepo = new();
    private readonly Mock<IRepository<Course>> _courseRepo = new();
    private readonly Mock<IRepository<Enrollment>> _enrollmentRepo = new();
    private readonly Mock<IRepository<FeeStructure>> _feeRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IEnquiryConvertedNotificationService> _notification = new();
    private readonly ILogger<ConvertEnquiryCommandHandler> _logger = NullLogger<ConvertEnquiryCommandHandler>.Instance;

    private ConvertEnquiryCommandHandler CreateHandler() => new(
        _enquiryRepo.Object,
        _userRepo.Object,
        _candidateRepo.Object,
        _courseRepo.Object,
        _enrollmentRepo.Object,
        _feeRepo.Object,
        _uow.Object,
        _hasher.Object,
        _notification.Object,
        _logger);

    private static ConvertEnquiryCommand Cmd(
        Guid? enquiryId = null,
        string email = "convert@example.com",
        DateOnly? dob = null,
        Gender gender = Gender.Female,
        string? address = "Some street 1",
        string? emergency = "0123456789") => new(
            enquiryId ?? Guid.NewGuid(),
            email,
            dob ?? new DateOnly(1995, 5, 5),
            gender,
            address,
            emergency);

    private static CourseEnquiry Existing(Guid enquiryId, Guid courseId, EnquiryStatus status = EnquiryStatus.New) => new()
    {
        Id = enquiryId,
        FullName = "Jane Smith",
        Phone = "0123456789",
        Email = "jane@example.com",
        CourseInterestedId = courseId,
        Status = status,
        AssignedCounselorId = Guid.NewGuid()
    };

    private static Course ActiveCourse(Guid id) => new()
    {
        Id = id,
        CourseName = "Java Bootcamp",
        DurationWeeks = 12,
        BaseFee = 1000m,
        IsActive = true
    };

    private void SetupHappyPath(CourseEnquiry enquiry, Course course)
    {
        _enquiryRepo.SetupGetById(enquiry.Id, enquiry);
        _courseRepo.SetupGetById(course.Id, course);
        _userRepo.SetupSequence(r => r.FindAsync(
                It.IsAny<Expression<Func<UserAccount, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserAccount>())
            .ReturnsAsync(new List<UserAccount>())
            .ReturnsAsync(new List<UserAccount>());
        _candidateRepo.SetupFind(Array.Empty<Candidate>());
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns<string>(p => $"hashed:{p}");
    }

    [Fact]
    public async Task Throws_NotFoundException_when_enquiry_does_not_exist()
    {
        var enquiryId = Guid.NewGuid();
        _enquiryRepo.SetupGetById(enquiryId, null);

        var act = async () => await CreateHandler().Handle(Cmd(enquiryId), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_enquiry_is_already_converted()
    {
        var enquiryId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        _enquiryRepo.SetupGetById(enquiryId, Existing(enquiryId, courseId, EnquiryStatus.Converted));

        var act = async () => await CreateHandler().Handle(Cmd(enquiryId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("This enquiry has already been converted.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_enquiry_is_closed()
    {
        var enquiryId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        _enquiryRepo.SetupGetById(enquiryId, Existing(enquiryId, courseId, EnquiryStatus.Closed));

        var act = async () => await CreateHandler().Handle(Cmd(enquiryId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("This enquiry is closed.");
    }

    [Fact]
    public async Task Throws_NotFoundException_when_course_is_missing()
    {
        var enquiryId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        _enquiryRepo.SetupGetById(enquiryId, Existing(enquiryId, courseId));
        _courseRepo.SetupGetById(courseId, null);

        var act = async () => await CreateHandler().Handle(Cmd(enquiryId), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Throws_ConflictException_when_email_is_already_in_use()
    {
        var enquiryId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        _enquiryRepo.SetupGetById(enquiryId, Existing(enquiryId, courseId));
        _courseRepo.SetupGetById(courseId, ActiveCourse(courseId));
        _userRepo.SetupFind(new[] { new UserAccount { Id = Guid.NewGuid(), Email = "convert@example.com" } });

        var act = async () => await CreateHandler().Handle(Cmd(enquiryId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Email is already associated with another account.");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_phone_is_already_in_use()
    {
        var enquiryId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        _enquiryRepo.SetupGetById(enquiryId, Existing(enquiryId, courseId));
        _courseRepo.SetupGetById(courseId, ActiveCourse(courseId));
        _userRepo.SetupSequence(r => r.FindAsync(
                It.IsAny<Expression<Func<UserAccount, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserAccount>())
            .ReturnsAsync(new List<UserAccount> { new() { Id = Guid.NewGuid(), Phone = "0123456789" } });

        var act = async () => await CreateHandler().Handle(Cmd(enquiryId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Phone number is already associated with another account.");
    }

    [Fact]
    public async Task Converts_enquiry_creates_user_candidate_enrollment_and_fee_on_success()
    {
        var enquiryId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var enquiry = Existing(enquiryId, courseId);
        var course = ActiveCourse(courseId);
        SetupHappyPath(enquiry, course);

        UserAccount? capturedUser = null;
        Candidate? capturedCandidate = null;
        Enrollment? capturedEnrollment = null;
        FeeStructure? capturedFee = null;

        _userRepo.Setup(r => r.AddAsync(It.IsAny<UserAccount>(), It.IsAny<CancellationToken>()))
            .Callback<UserAccount, CancellationToken>((u, _) => capturedUser = u)
            .ReturnsAsync((UserAccount u, CancellationToken _) => u);
        _candidateRepo.Setup(r => r.AddAsync(It.IsAny<Candidate>(), It.IsAny<CancellationToken>()))
            .Callback<Candidate, CancellationToken>((c, _) => capturedCandidate = c)
            .ReturnsAsync((Candidate c, CancellationToken _) => c);
        _enrollmentRepo.Setup(r => r.AddAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .Callback<Enrollment, CancellationToken>((e, _) => capturedEnrollment = e)
            .ReturnsAsync((Enrollment e, CancellationToken _) => e);
        _feeRepo.Setup(r => r.AddAsync(It.IsAny<FeeStructure>(), It.IsAny<CancellationToken>()))
            .Callback<FeeStructure, CancellationToken>((f, _) => capturedFee = f)
            .ReturnsAsync((FeeStructure f, CancellationToken _) => f);

        var response = await CreateHandler().Handle(
            Cmd(enquiryId, email: "  convert@example.com  ", address: "  My Address  ", emergency: "  9876543210  "),
            default);

        capturedUser.Should().NotBeNull();
        capturedUser!.Email.Should().Be("convert@example.com");
        capturedUser.Phone.Should().Be("0123456789");
        capturedUser.FullName.Should().Be("Jane Smith");
        capturedUser.Role.Should().Be(UserRole.Candidate);
        capturedUser.IsActive.Should().BeFalse();
        capturedUser.MustChangePassword.Should().BeTrue();
        capturedUser.PasswordHash.Should().StartWith("hashed:");
        capturedUser.Username.Should().StartWith("cand_");

        capturedCandidate.Should().NotBeNull();
        capturedCandidate!.UserAccountId.Should().Be(capturedUser.Id);
        capturedCandidate.CandidateCode.Should().StartWith("C");
        capturedCandidate.Address.Should().Be("My Address");
        capturedCandidate.EmergencyContact.Should().Be("9876543210");
        capturedCandidate.Status.Should().Be(CandidateStatus.Active);
        capturedCandidate.RegisteredByStaffId.Should().Be(enquiry.AssignedCounselorId);

        capturedFee.Should().NotBeNull();
        capturedFee!.CandidateId.Should().Be(capturedCandidate.Id);
        capturedFee.FeeType.Should().Be(FeeType.Tuition);
        capturedFee.TotalFee.Should().Be(course.BaseFee);
        capturedFee.OutstandingBalance.Should().Be(course.BaseFee);
        capturedFee.AmountPaid.Should().Be(0);
        capturedFee.PaymentStatus.Should().Be(PaymentStatus.Unpaid);
        capturedFee.PaymentType.Should().Be(PaymentType.NotSet);

        capturedEnrollment.Should().NotBeNull();
        capturedEnrollment!.CandidateId.Should().Be(capturedCandidate.Id);
        capturedEnrollment.FeeId.Should().Be(capturedFee.Id);
        capturedEnrollment.CourseId.Should().Be(course.Id);
        capturedEnrollment.Status.Should().Be(EnrollmentStatus.PendingAssignment);

        enquiry.Status.Should().Be(EnquiryStatus.Converted);
        enquiry.ConvertedCandidateId.Should().Be(capturedCandidate.Id);
        enquiry.ConvertedAt.Should().NotBeNull();
        enquiry.Email.Should().Be("convert@example.com");

        response.CandidateId.Should().Be(capturedCandidate.Id);
        response.UserAccountId.Should().Be(capturedUser.Id);
        response.CandidateCode.Should().Be(capturedCandidate.CandidateCode);
        response.Username.Should().Be(capturedUser.Username);
        response.TemporaryPassword.Should().NotBeNullOrEmpty();
        response.Email.Should().Be("convert@example.com");

        _enquiryRepo.Verify(r => r.Update(enquiry), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _notification.Verify(n => n.QueueAsync(It.IsAny<EnquiryConvertedEmailModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Sets_address_and_emergency_to_null_when_blank()
    {
        var enquiryId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var enquiry = Existing(enquiryId, courseId);
        var course = ActiveCourse(courseId);
        SetupHappyPath(enquiry, course);

        Candidate? capturedCandidate = null;
        _candidateRepo.Setup(r => r.AddAsync(It.IsAny<Candidate>(), It.IsAny<CancellationToken>()))
            .Callback<Candidate, CancellationToken>((c, _) => capturedCandidate = c)
            .ReturnsAsync((Candidate c, CancellationToken _) => c);

        await CreateHandler().Handle(Cmd(enquiryId, address: "   ", emergency: null), default);

        capturedCandidate!.Address.Should().BeNull();
        capturedCandidate.EmergencyContact.Should().BeNull();
    }

    [Fact]
    public async Task Does_not_throw_when_notification_queue_fails()
    {
        var enquiryId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var enquiry = Existing(enquiryId, courseId);
        var course = ActiveCourse(courseId);
        SetupHappyPath(enquiry, course);
        _notification.Setup(n => n.QueueAsync(It.IsAny<EnquiryConvertedEmailModel>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("smtp down"));

        var act = async () => await CreateHandler().Handle(Cmd(enquiryId), default);

        await act.Should().NotThrowAsync();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
