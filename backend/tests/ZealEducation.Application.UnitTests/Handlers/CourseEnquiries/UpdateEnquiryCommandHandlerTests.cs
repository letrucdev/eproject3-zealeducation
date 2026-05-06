using System.Linq.Expressions;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.CourseEnquiries.Commands.UpdateEnquiry;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.CourseEnquiries;

public class UpdateEnquiryCommandHandlerTests
{
    private readonly Mock<IRepository<CourseEnquiry>> _enquiryRepo = new();
    private readonly Mock<IRepository<Course>> _courseRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private UpdateEnquiryCommandHandler CreateHandler() => new(
        _enquiryRepo.Object,
        _courseRepo.Object,
        _uow.Object);

    private static UpdateEnquiryCommand Cmd(
        Guid? enquiryId = null,
        string fullName = "Jane Smith",
        string phone = "0123456789",
        string? email = "jane@example.com",
        Guid? courseId = null,
        EnquirySource source = EnquirySource.Phone,
        EnquiryStatus status = EnquiryStatus.Contacted,
        DateOnly? nextFollowUp = null) => new(
            enquiryId ?? Guid.NewGuid(),
            fullName,
            phone,
            email,
            courseId ?? Guid.NewGuid(),
            source,
            status,
            nextFollowUp);

    private static CourseEnquiry Existing(Guid enquiryId, Guid courseId, string phone = "0123456789", EnquiryStatus status = EnquiryStatus.New) => new()
    {
        Id = enquiryId,
        FullName = "Old Name",
        Phone = phone,
        Email = "old@example.com",
        CourseInterestedId = courseId,
        Source = EnquirySource.WalkIn,
        Status = status,
        AssignedCounselorId = Guid.NewGuid()
    };

    [Fact]
    public async Task Throws_NotFoundException_when_enquiry_does_not_exist()
    {
        var enquiryId = Guid.NewGuid();
        _enquiryRepo.SetupGetById(enquiryId, null);

        var act = async () => await CreateHandler().Handle(Cmd(enquiryId), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _enquiryRepo.Verify(r => r.Update(It.IsAny<CourseEnquiry>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_enquiry_is_converted()
    {
        var enquiryId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        _enquiryRepo.SetupGetById(enquiryId, Existing(enquiryId, courseId, status: EnquiryStatus.Converted));

        var act = async () => await CreateHandler().Handle(Cmd(enquiryId, courseId: courseId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("A converted enquiry cannot be updated.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_enquiry_is_closed()
    {
        var enquiryId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        _enquiryRepo.SetupGetById(enquiryId, Existing(enquiryId, courseId, status: EnquiryStatus.Closed));

        var act = async () => await CreateHandler().Handle(Cmd(enquiryId, courseId: courseId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("A closed enquiry cannot be updated.");
    }

    [Fact]
    public async Task Throws_NotFoundException_when_changing_to_missing_course()
    {
        var enquiryId = Guid.NewGuid();
        var oldCourseId = Guid.NewGuid();
        var newCourseId = Guid.NewGuid();
        _enquiryRepo.SetupGetById(enquiryId, Existing(enquiryId, oldCourseId));
        _courseRepo.SetupGetById(newCourseId, null);

        var act = async () => await CreateHandler().Handle(Cmd(enquiryId, courseId: newCourseId), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Throws_ConflictException_when_changing_to_inactive_course()
    {
        var enquiryId = Guid.NewGuid();
        var oldCourseId = Guid.NewGuid();
        var newCourseId = Guid.NewGuid();
        _enquiryRepo.SetupGetById(enquiryId, Existing(enquiryId, oldCourseId));
        _courseRepo.SetupGetById(newCourseId, new Course { Id = newCourseId, IsActive = false });

        var act = async () => await CreateHandler().Handle(Cmd(enquiryId, courseId: newCourseId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("The selected course is inactive and no longer accepting enquiries.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_phone_changed_and_duplicate_exists()
    {
        var enquiryId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        _enquiryRepo.SetupGetById(enquiryId, Existing(enquiryId, courseId, phone: "0000000000"));
        _enquiryRepo.SetupFind(new[] { new CourseEnquiry { Id = Guid.NewGuid(), Phone = "0123456789" } });

        var act = async () => await CreateHandler().Handle(Cmd(enquiryId, phone: "0123456789", courseId: courseId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("An enquiry with this phone number already exists.");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_email_changed_and_duplicate_exists()
    {
        var enquiryId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var existing = Existing(enquiryId, courseId);
        existing.Email = "old@example.com";
        _enquiryRepo.SetupGetById(enquiryId, existing);
        _enquiryRepo.SetupFind(new[] { new CourseEnquiry { Id = Guid.NewGuid(), Email = "new@example.com" } });

        var act = async () => await CreateHandler().Handle(
            Cmd(enquiryId, courseId: courseId, email: "new@example.com"), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("An enquiry with this email already exists.");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Updates_enquiry_and_trims_fields_when_command_is_valid()
    {
        var enquiryId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var enquiry = Existing(enquiryId, courseId, phone: "0123456789");
        _enquiryRepo.SetupGetById(enquiryId, enquiry);
        _enquiryRepo.SetupFind(Array.Empty<CourseEnquiry>());

        var followUp = new DateOnly(2031, 1, 1);
        var cmd = new UpdateEnquiryCommand(
            enquiryId,
            "  New Name  ",
            "  0123456789  ",
            "  new@example.com  ",
            courseId,
            EnquirySource.SocialMedia,
            EnquiryStatus.Interested,
            followUp);

        var result = await CreateHandler().Handle(cmd, default);

        enquiry.FullName.Should().Be("New Name");
        enquiry.Phone.Should().Be("0123456789");
        enquiry.Email.Should().Be("new@example.com");
        enquiry.CourseInterestedId.Should().Be(courseId);
        enquiry.Source.Should().Be(EnquirySource.SocialMedia);
        enquiry.Status.Should().Be(EnquiryStatus.Interested);
        enquiry.NextFollowUpDate.Should().Be(followUp);

        result.Should().Be(MediatR.Unit.Value);
        _enquiryRepo.Verify(r => r.Update(enquiry), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _courseRepo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Sets_email_to_null_when_request_email_is_whitespace()
    {
        var enquiryId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var enquiry = Existing(enquiryId, courseId);
        _enquiryRepo.SetupGetById(enquiryId, enquiry);

        await CreateHandler().Handle(Cmd(enquiryId, courseId: courseId, email: "   "), default);

        enquiry.Email.Should().BeNull();
    }

    [Fact]
    public async Task Updates_course_when_course_id_changes()
    {
        var enquiryId = Guid.NewGuid();
        var oldCourseId = Guid.NewGuid();
        var newCourseId = Guid.NewGuid();
        var enquiry = Existing(enquiryId, oldCourseId);
        _enquiryRepo.SetupGetById(enquiryId, enquiry);
        _courseRepo.SetupGetById(newCourseId, new Course { Id = newCourseId, IsActive = true });
        _enquiryRepo.Setup(r => r.FindAsync(
            It.IsAny<Expression<Func<CourseEnquiry, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CourseEnquiry>());

        await CreateHandler().Handle(Cmd(enquiryId, courseId: newCourseId), default);

        enquiry.CourseInterestedId.Should().Be(newCourseId);
        _enquiryRepo.Verify(r => r.Update(enquiry), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
