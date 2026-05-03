using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.CourseEnquiries.Commands.CreateEnquiry;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.CourseEnquiries;

public class CreateEnquiryCommandHandlerTests
{
    private readonly Mock<IRepository<CourseEnquiry>> _enquiryRepo = new();
    private readonly Mock<IRepository<Staff>> _staffRepo = new();
    private readonly Mock<IRepository<Course>> _courseRepo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private CreateEnquiryCommandHandler CreateHandler() => new(
        _enquiryRepo.Object,
        _staffRepo.Object,
        _courseRepo.Object,
        _uow.Object,
        _currentUser.Object);

    private static CreateEnquiryCommand Cmd(
        string fullName = "Jane Smith",
        string phone = "0123456789",
        string? email = "jane@example.com",
        Guid? courseId = null,
        EnquirySource source = EnquirySource.Phone,
        EnquiryStatus status = EnquiryStatus.New,
        DateOnly? nextFollowUp = null) => new(
            fullName,
            phone,
            email,
            courseId ?? Guid.NewGuid(),
            source,
            status,
            nextFollowUp);

    [Fact]
    public async Task Throws_unauthorized_when_current_user_is_not_resolved()
    {
        _currentUser.SetupGet(u => u.UserId).Returns((Guid?)null);

        var act = async () => await CreateHandler().Handle(Cmd(), default);

        await act.Should().ThrowAsync<UnauthorizedException>();
        _enquiryRepo.Verify(r => r.AddAsync(It.IsAny<CourseEnquiry>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_NotFoundException_when_course_does_not_exist()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _staffRepo.SetupFind(new[] { new Staff { Id = Guid.NewGuid(), UserAccountId = userId } });
        _courseRepo.SetupGetById(courseId, null);

        var act = async () => await CreateHandler().Handle(Cmd(courseId: courseId), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_course_is_inactive()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _staffRepo.SetupFind(new[] { new Staff { Id = Guid.NewGuid(), UserAccountId = userId } });
        _courseRepo.SetupGetById(courseId, new Course { Id = courseId, CourseName = "X", IsActive = false });

        var act = async () => await CreateHandler().Handle(Cmd(courseId: courseId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("The selected course is inactive and no longer accepting enquiries.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_phone_duplicate_exists()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _staffRepo.SetupFind(new[] { new Staff { Id = Guid.NewGuid(), UserAccountId = userId } });
        _courseRepo.SetupGetById(courseId, new Course { Id = courseId, CourseName = "X", IsActive = true });
        _enquiryRepo.SetupFind(new[] { new CourseEnquiry { Id = Guid.NewGuid(), Phone = "0123456789" } });

        var act = async () => await CreateHandler().Handle(Cmd(courseId: courseId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("An enquiry with this phone number already exists.");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Creates_enquiry_with_trimmed_fields_and_returns_response()
    {
        var userId = Guid.NewGuid();
        var staffId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var course = new Course { Id = courseId, CourseName = "Java Bootcamp", IsActive = true };

        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _staffRepo.SetupFind(new[] { new Staff { Id = staffId, UserAccountId = userId } });
        _courseRepo.SetupGetById(courseId, course);
        _enquiryRepo.SetupFind(Array.Empty<CourseEnquiry>());

        CourseEnquiry? captured = null;
        _enquiryRepo.Setup(r => r.AddAsync(It.IsAny<CourseEnquiry>(), It.IsAny<CancellationToken>()))
            .Callback<CourseEnquiry, CancellationToken>((e, _) => captured = e)
            .ReturnsAsync((CourseEnquiry e, CancellationToken _) => e);

        var followUp = new DateOnly(2030, 6, 1);
        var cmd = new CreateEnquiryCommand(
            "  Jane Smith  ",
            "0123456789",
            "  jane@example.com  ",
            courseId,
            EnquirySource.Website,
            EnquiryStatus.Contacted,
            followUp);

        var response = await CreateHandler().Handle(cmd, default);

        captured.Should().NotBeNull();
        captured!.FullName.Should().Be("Jane Smith");
        captured.Phone.Should().Be("0123456789");
        captured.Email.Should().Be("jane@example.com");
        captured.CourseInterestedId.Should().Be(courseId);
        captured.Source.Should().Be(EnquirySource.Website);
        captured.Status.Should().Be(EnquiryStatus.Contacted);
        captured.NextFollowUpDate.Should().Be(followUp);
        captured.AssignedCounselorId.Should().Be(staffId);

        response.EnquiryId.Should().Be(captured.Id);
        response.FullName.Should().Be("Jane Smith");
        response.Phone.Should().Be("0123456789");
        response.CourseInterestedId.Should().Be(courseId);
        response.CourseInterestedName.Should().Be("Java Bootcamp");

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Sets_email_to_null_when_request_email_is_whitespace()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _staffRepo.SetupFind(new[] { new Staff { Id = Guid.NewGuid(), UserAccountId = userId } });
        _courseRepo.SetupGetById(courseId, new Course { Id = courseId, CourseName = "C", IsActive = true });
        _enquiryRepo.SetupFind(Array.Empty<CourseEnquiry>());

        CourseEnquiry? captured = null;
        _enquiryRepo.Setup(r => r.AddAsync(It.IsAny<CourseEnquiry>(), It.IsAny<CancellationToken>()))
            .Callback<CourseEnquiry, CancellationToken>((e, _) => captured = e)
            .ReturnsAsync((CourseEnquiry e, CancellationToken _) => e);

        await CreateHandler().Handle(Cmd(courseId: courseId, email: "   "), default);

        captured!.Email.Should().BeNull();
    }
}
