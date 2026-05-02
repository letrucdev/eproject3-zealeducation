using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.ClassSessions.Commands.UpdateClassSession;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.ClassSessions;

public class UpdateClassSessionCommandHandlerTests
{
    private readonly Mock<IRepository<ClassSession>> _sessionRepo = new();
    private readonly Mock<IRepository<Batch>> _batchRepo = new();
    private readonly Mock<IRepository<AttendanceRecord>> _attendanceRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private UpdateClassSessionCommandHandler CreateHandler() =>
        new(_sessionRepo.Object, _batchRepo.Object, _attendanceRepo.Object, _uow.Object);

    private static Batch ExistingBatch(
        Guid batchId,
        DateOnly? start = null,
        DateOnly? end = null) => new()
    {
        Id = batchId,
        BatchCode = "B-001",
        CourseId = Guid.NewGuid(),
        StartDate = start ?? new DateOnly(2030, 1, 1),
        EndDate = end ?? new DateOnly(2030, 6, 1),
        Status = BatchStatus.Active
    };

    private static ClassSession ExistingSession(
        Guid sessionId,
        Guid batchId,
        DateOnly? sessionDate = null,
        TimeOnly? startTime = null,
        TimeOnly? endTime = null) => new()
    {
        Id = sessionId,
        BatchId = batchId,
        SessionDate = sessionDate ?? new DateOnly(2030, 1, 15),
        StartTime = startTime ?? new TimeOnly(9, 0),
        EndTime = endTime ?? new TimeOnly(11, 0),
        Status = ClassSessionStatus.Scheduled
    };

    private static UpdateClassSessionCommand Cmd(
        Guid sessionId,
        DateOnly? sessionDate = null,
        TimeOnly? startTime = null,
        TimeOnly? endTime = null,
        string? topic = "Topic",
        string? location = "Room A",
        ClassSessionStatus status = ClassSessionStatus.Scheduled) => new(
            sessionId,
            sessionDate ?? new DateOnly(2030, 1, 15),
            startTime ?? new TimeOnly(9, 0),
            endTime ?? new TimeOnly(11, 0),
            topic,
            location,
            status);

    private void SetupSessionQuery(IEnumerable<ClassSession> data)
    {
        var queryable = new TestAsyncEnumerable<ClassSession>(data);
        _sessionRepo.Setup(r => r.Query()).Returns(queryable);
    }

    private void SetupAttendanceQuery(IEnumerable<AttendanceRecord> data)
    {
        var queryable = new TestAsyncEnumerable<AttendanceRecord>(data);
        _attendanceRepo.Setup(r => r.Query()).Returns(queryable);
    }

    [Fact]
    public async Task Throws_NotFoundException_when_session_does_not_exist()
    {
        var sessionId = Guid.NewGuid();
        _sessionRepo.SetupGetById(sessionId, null);

        var act = async () => await CreateHandler().Handle(Cmd(sessionId), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _sessionRepo.Verify(r => r.Update(It.IsAny<ClassSession>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_NotFoundException_when_batch_does_not_exist()
    {
        var sessionId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        _sessionRepo.SetupGetById(sessionId, ExistingSession(sessionId, batchId));
        _batchRepo.SetupGetById(batchId, null);

        var act = async () => await CreateHandler().Handle(Cmd(sessionId), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_changing_date_with_existing_attendance()
    {
        var sessionId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        var session = ExistingSession(sessionId, batchId, sessionDate: new DateOnly(2030, 1, 15));
        _sessionRepo.SetupGetById(sessionId, session);
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId));

        SetupAttendanceQuery(new[]
        {
            new AttendanceRecord
            {
                Id = Guid.NewGuid(),
                ClassSessionId = sessionId,
                EnrollmentId = Guid.NewGuid(),
                Status = AttendanceStatus.Present
            }
        });

        var act = async () => await CreateHandler().Handle(
            Cmd(sessionId, sessionDate: new DateOnly(2030, 1, 16)), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Session date cannot be changed because attendance has already been recorded for this session.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_new_date_falls_outside_batch_period()
    {
        var sessionId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        var session = ExistingSession(sessionId, batchId, sessionDate: new DateOnly(2030, 1, 15));
        _sessionRepo.SetupGetById(sessionId, session);
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId,
            start: new DateOnly(2030, 1, 1), end: new DateOnly(2030, 6, 1)));
        SetupAttendanceQuery(Array.Empty<AttendanceRecord>());

        var act = async () => await CreateHandler().Handle(
            Cmd(sessionId, sessionDate: new DateOnly(2030, 7, 1)), default);

        await act.Should().ThrowAsync<ConflictException>()
            .Where(e => e.Message.Contains("must fall within the batch period"));
    }

    [Fact]
    public async Task Throws_ConflictException_when_new_time_overlaps_with_another_session()
    {
        var sessionId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        var session = ExistingSession(sessionId, batchId,
            sessionDate: new DateOnly(2030, 1, 15),
            startTime: new TimeOnly(9, 0),
            endTime: new TimeOnly(11, 0));
        _sessionRepo.SetupGetById(sessionId, session);
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId));
        SetupAttendanceQuery(Array.Empty<AttendanceRecord>());

        var otherSessionId = Guid.NewGuid();
        SetupSessionQuery(new[]
        {
            new ClassSession
            {
                Id = otherSessionId,
                BatchId = batchId,
                SessionDate = new DateOnly(2030, 1, 15),
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(12, 0)
            }
        });

        var act = async () => await CreateHandler().Handle(
            Cmd(sessionId,
                sessionDate: new DateOnly(2030, 1, 15),
                startTime: new TimeOnly(11, 30),
                endTime: new TimeOnly(13, 30)),
            default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("This session overlaps with another session on the same date.");
    }

    [Fact]
    public async Task Updates_session_when_command_is_valid()
    {
        var sessionId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        var session = ExistingSession(sessionId, batchId,
            sessionDate: new DateOnly(2030, 1, 15),
            startTime: new TimeOnly(9, 0),
            endTime: new TimeOnly(11, 0));
        _sessionRepo.SetupGetById(sessionId, session);
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId));
        SetupAttendanceQuery(Array.Empty<AttendanceRecord>());
        SetupSessionQuery(Array.Empty<ClassSession>());

        await CreateHandler().Handle(
            Cmd(sessionId,
                sessionDate: new DateOnly(2030, 1, 20),
                startTime: new TimeOnly(13, 0),
                endTime: new TimeOnly(15, 0),
                topic: "  New Topic  ",
                location: "  Room B  ",
                status: ClassSessionStatus.Completed),
            default);

        session.SessionDate.Should().Be(new DateOnly(2030, 1, 20));
        session.StartTime.Should().Be(new TimeOnly(13, 0));
        session.EndTime.Should().Be(new TimeOnly(15, 0));
        session.Topic.Should().Be("New Topic");
        session.Location.Should().Be("Room B");
        session.Status.Should().Be(ClassSessionStatus.Completed);

        _sessionRepo.Verify(r => r.Update(session), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Skips_attendance_and_date_range_check_when_session_date_unchanged()
    {
        var sessionId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        var session = ExistingSession(sessionId, batchId,
            sessionDate: new DateOnly(2030, 1, 15));
        _sessionRepo.SetupGetById(sessionId, session);
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId,
            start: new DateOnly(2030, 1, 1), end: new DateOnly(2030, 6, 1)));
        SetupSessionQuery(Array.Empty<ClassSession>());

        await CreateHandler().Handle(
            Cmd(sessionId,
                sessionDate: new DateOnly(2030, 1, 15),
                startTime: new TimeOnly(13, 0),
                endTime: new TimeOnly(15, 0),
                status: ClassSessionStatus.Completed),
            default);

        _attendanceRepo.Verify(r => r.Query(), Times.Never);
        _sessionRepo.Verify(r => r.Update(session), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Sets_topic_and_location_to_null_when_whitespace()
    {
        var sessionId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        var session = ExistingSession(sessionId, batchId);
        _sessionRepo.SetupGetById(sessionId, session);
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId));
        SetupSessionQuery(Array.Empty<ClassSession>());

        await CreateHandler().Handle(
            Cmd(sessionId, topic: "   ", location: ""),
            default);

        session.Topic.Should().BeNull();
        session.Location.Should().BeNull();
    }
}

