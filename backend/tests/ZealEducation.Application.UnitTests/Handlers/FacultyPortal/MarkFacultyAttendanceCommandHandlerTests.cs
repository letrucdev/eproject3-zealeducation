using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.ClassSessions.Commands.MarkAttendance;
using ZealEducation.Application.Features.FacultyPortal.Commands.MarkFacultyAttendance;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.FacultyPortal;

public class MarkFacultyAttendanceCommandHandlerTests
{
    private readonly Mock<ISender> _sender = new();
    private readonly Mock<IRepository<ClassSession>> _sessionRepo = new();
    private readonly Mock<IRepository<Faculty>> _facultyRepo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();

    private MarkFacultyAttendanceCommandHandler CreateHandler() =>
        new(_sender.Object, _sessionRepo.Object, _facultyRepo.Object, _currentUser.Object);

    private static Faculty NewFaculty(Guid userId, Guid? facultyId = null) => new()
    {
        Id = facultyId ?? Guid.NewGuid(),
        StaffId = Guid.NewGuid(),
        FacultyCode = "F-001",
        Qualification = "MSc",
        Specialization = "CS",
        ExperienceYears = 5,
        Staff = new Staff
        {
            Id = Guid.NewGuid(),
            UserAccountId = userId,
            Position = "Lecturer",
            Department = "CS",
            JoinedDate = new DateOnly(2020, 1, 1),
            IsActive = true
        }
    };

    private static ClassSession NewSession(Guid sessionId, Guid? facultyId = null) => new()
    {
        Id = sessionId,
        BatchId = Guid.NewGuid(),
        SessionDate = new DateOnly(2030, 2, 1),
        StartTime = new TimeOnly(9, 0),
        EndTime = new TimeOnly(11, 0),
        Batch = new Batch
        {
            Id = Guid.NewGuid(),
            BatchCode = "B-001",
            CourseId = Guid.NewGuid(),
            FacultyId = facultyId,
            StartDate = new DateOnly(2030, 1, 1),
            EndDate = new DateOnly(2030, 6, 1)
        }
    };

    private void SetupFacultyQuery(IEnumerable<Faculty> data) =>
        _facultyRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<Faculty>(data));

    private void SetupSessionQuery(IEnumerable<ClassSession> data) =>
        _sessionRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<ClassSession>(data));

    private static List<AttendanceEntry> SampleEntries() => new()
    {
        new AttendanceEntry(Guid.NewGuid(), AttendanceStatus.Present, null, null)
    };

    [Fact]
    public async Task Throws_UnauthorizedException_when_current_user_is_null()
    {
        _currentUser.SetupGet(u => u.UserId).Returns((Guid?)null);

        var act = async () => await CreateHandler().Handle(
            new MarkFacultyAttendanceCommand(Guid.NewGuid(), SampleEntries()), default);

        await act.Should().ThrowAsync<UnauthorizedException>();
        _sender.Verify(s => s.Send(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_UnauthorizedException_when_current_user_is_not_faculty()
    {
        var userId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        SetupFacultyQuery(Array.Empty<Faculty>());

        var act = async () => await CreateHandler().Handle(
            new MarkFacultyAttendanceCommand(Guid.NewGuid(), SampleEntries()), default);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Current user is not registered as faculty.");
    }

    [Fact]
    public async Task Throws_NotFoundException_when_session_is_not_owned_by_faculty()
    {
        var userId = Guid.NewGuid();
        var faculty = NewFaculty(userId);
        var sessionId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        SetupFacultyQuery(new[] { faculty });
        SetupSessionQuery(new[] { NewSession(sessionId, facultyId: Guid.NewGuid()) });

        var act = async () => await CreateHandler().Handle(
            new MarkFacultyAttendanceCommand(sessionId, SampleEntries()), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _sender.Verify(s => s.Send(It.IsAny<MarkAttendanceCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_NotFoundException_when_session_does_not_exist()
    {
        var userId = Guid.NewGuid();
        var faculty = NewFaculty(userId);
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        SetupFacultyQuery(new[] { faculty });
        SetupSessionQuery(Array.Empty<ClassSession>());

        var act = async () => await CreateHandler().Handle(
            new MarkFacultyAttendanceCommand(Guid.NewGuid(), SampleEntries()), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Forwards_to_MarkAttendanceCommand_when_session_is_owned_by_faculty()
    {
        var userId = Guid.NewGuid();
        var faculty = NewFaculty(userId);
        var sessionId = Guid.NewGuid();
        var entries = SampleEntries();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        SetupFacultyQuery(new[] { faculty });
        SetupSessionQuery(new[] { NewSession(sessionId, facultyId: faculty.Id) });

        MarkAttendanceCommand? captured = null;
        _sender.Setup(s => s.Send(It.IsAny<MarkAttendanceCommand>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((cmd, _) => captured = (MarkAttendanceCommand)cmd)
            .ReturnsAsync(Unit.Value);

        var result = await CreateHandler().Handle(
            new MarkFacultyAttendanceCommand(sessionId, entries), default);

        result.Should().Be(Unit.Value);
        captured.Should().NotBeNull();
        captured!.SessionId.Should().Be(sessionId);
        captured.Entries.Should().BeSameAs(entries);
        _sender.Verify(s => s.Send(It.IsAny<MarkAttendanceCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
