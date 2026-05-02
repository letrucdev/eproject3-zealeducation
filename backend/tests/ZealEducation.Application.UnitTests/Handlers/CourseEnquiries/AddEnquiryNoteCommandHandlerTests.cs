using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.CourseEnquiries.Commands.AddEnquiryNote;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.CourseEnquiries;

public class AddEnquiryNoteCommandHandlerTests
{
    private readonly Mock<IRepository<CourseEnquiry>> _enquiryRepo = new();
    private readonly Mock<IRepository<EnquiryNote>> _noteRepo = new();
    private readonly Mock<IRepository<Staff>> _staffRepo = new();
    private readonly Mock<IRepository<UserAccount>> _userRepo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private AddEnquiryNoteCommandHandler CreateHandler() => new(
        _enquiryRepo.Object,
        _noteRepo.Object,
        _staffRepo.Object,
        _userRepo.Object,
        _uow.Object,
        _currentUser.Object);

    private static AddEnquiryNoteCommand Cmd(
        Guid? enquiryId = null,
        string content = "Followed up by phone.") => new(
            enquiryId ?? Guid.NewGuid(),
            content);

    [Fact]
    public async Task Throws_unauthorized_when_current_user_is_not_resolved()
    {
        _currentUser.SetupGet(u => u.UserId).Returns((Guid?)null);

        var act = async () => await CreateHandler().Handle(Cmd(), default);

        await act.Should().ThrowAsync<UnauthorizedException>();
        _noteRepo.Verify(r => r.AddAsync(It.IsAny<EnquiryNote>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_NotFoundException_when_enquiry_does_not_exist()
    {
        var enquiryId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        _enquiryRepo.SetupGetById(enquiryId, null);

        var act = async () => await CreateHandler().Handle(Cmd(enquiryId), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_unauthorized_when_current_user_is_not_staff()
    {
        var enquiryId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _enquiryRepo.SetupGetById(enquiryId, new CourseEnquiry { Id = enquiryId });
        _staffRepo.SetupFind(Array.Empty<Staff>());

        var act = async () => await CreateHandler().Handle(Cmd(enquiryId), default);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Current user is not linked to a staff profile.");
    }

    [Fact]
    public async Task Throws_unauthorized_when_user_account_is_missing()
    {
        var enquiryId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _enquiryRepo.SetupGetById(enquiryId, new CourseEnquiry { Id = enquiryId });
        _staffRepo.SetupFind(new[] { new Staff { Id = Guid.NewGuid(), UserAccountId = userId } });
        _userRepo.SetupGetById(userId, null);

        var act = async () => await CreateHandler().Handle(Cmd(enquiryId), default);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Current user account not found.");
    }

    [Fact]
    public async Task Adds_note_with_trimmed_content_and_returns_response()
    {
        var enquiryId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var staffId = Guid.NewGuid();
        var enquiry = new CourseEnquiry { Id = enquiryId };
        var staff = new Staff { Id = staffId, UserAccountId = userId };
        var user = new UserAccount { Id = userId, FullName = "Counselor John" };

        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _enquiryRepo.SetupGetById(enquiryId, enquiry);
        _staffRepo.SetupFind(new[] { staff });
        _userRepo.SetupGetById(userId, user);

        EnquiryNote? captured = null;
        _noteRepo.Setup(r => r.AddAsync(It.IsAny<EnquiryNote>(), It.IsAny<CancellationToken>()))
            .Callback<EnquiryNote, CancellationToken>((n, _) => captured = n)
            .ReturnsAsync((EnquiryNote n, CancellationToken _) => n);

        var response = await CreateHandler().Handle(
            Cmd(enquiryId, content: "  Spoke with the candidate, will follow up.  "),
            default);

        captured.Should().NotBeNull();
        captured!.EnquiryId.Should().Be(enquiryId);
        captured.AuthorStaffId.Should().Be(staffId);
        captured.Content.Should().Be("Spoke with the candidate, will follow up.");

        response.NoteId.Should().Be(captured.Id);
        response.EnquiryId.Should().Be(enquiryId);
        response.AuthorStaffId.Should().Be(staffId);
        response.AuthorFullName.Should().Be("Counselor John");
        response.Content.Should().Be("Spoke with the candidate, will follow up.");

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
