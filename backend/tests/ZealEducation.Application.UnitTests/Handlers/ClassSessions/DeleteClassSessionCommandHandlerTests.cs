using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.ClassSessions.Commands.DeleteClassSession;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.ClassSessions;

public class DeleteClassSessionCommandHandlerTests
{
    private readonly Mock<IRepository<ClassSession>> _sessionRepo = new();
    private readonly Mock<IRepository<AttendanceRecord>> _attendanceRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private DeleteClassSessionCommandHandler CreateHandler() =>
        new(_sessionRepo.Object, _attendanceRepo.Object, _uow.Object);

    private static ClassSession ExistingSession(Guid sessionId) => new()
    {
        Id = sessionId,
        BatchId = Guid.NewGuid(),
        SessionDate = new DateOnly(2030, 1, 15),
        StartTime = new TimeOnly(9, 0),
        EndTime = new TimeOnly(11, 0),
        Status = ClassSessionStatus.Scheduled
    };

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

        var act = async () => await CreateHandler().Handle(new DeleteClassSessionCommand(sessionId), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _sessionRepo.Verify(r => r.Delete(It.IsAny<ClassSession>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_session_has_attendance_records()
    {
        var sessionId = Guid.NewGuid();
        _sessionRepo.SetupGetById(sessionId, ExistingSession(sessionId));
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

        var act = async () => await CreateHandler().Handle(new DeleteClassSessionCommand(sessionId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Cannot delete a session that already has attendance records.");
        _sessionRepo.Verify(r => r.Delete(It.IsAny<ClassSession>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Deletes_session_when_no_attendance_records()
    {
        var sessionId = Guid.NewGuid();
        var session = ExistingSession(sessionId);
        _sessionRepo.SetupGetById(sessionId, session);
        SetupAttendanceQuery(Array.Empty<AttendanceRecord>());

        await CreateHandler().Handle(new DeleteClassSessionCommand(sessionId), default);

        _sessionRepo.Verify(r => r.Delete(session), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
