using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.ClassSessions.Commands.MarkAttendance;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.ClassSessions;

public class MarkAttendanceCommandHandlerTests
{
    private readonly Mock<IRepository<ClassSession>> _sessionRepo = new();
    private readonly Mock<IRepository<Enrollment>> _enrollmentRepo = new();
    private readonly Mock<IRepository<AttendanceRecord>> _attendanceRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private MarkAttendanceCommandHandler CreateHandler() =>
        new(_sessionRepo.Object, _enrollmentRepo.Object, _attendanceRepo.Object, _uow.Object);

    private static ClassSession ExistingSession(Guid sessionId, Guid batchId, TimeOnly? endTime = null) => new()
    {
        Id = sessionId,
        BatchId = batchId,
        SessionDate = new DateOnly(2030, 1, 15),
        StartTime = new TimeOnly(9, 0),
        EndTime = endTime ?? new TimeOnly(13, 0),
        Status = ClassSessionStatus.Scheduled
    };

    private static Enrollment Enrollment(Guid enrollmentId, Guid batchId) => new()
    {
        Id = enrollmentId,
        BatchId = batchId,
        CandidateId = Guid.NewGuid(),
        CourseId = Guid.NewGuid(),
        Status = EnrollmentStatus.Enrolled
    };

    private void SetupEnrollmentQuery(IEnumerable<Enrollment> data)
    {
        var queryable = new TestAsyncEnumerable<Enrollment>(data);
        _enrollmentRepo.Setup(r => r.Query()).Returns(queryable);
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

        var cmd = new MarkAttendanceCommand(sessionId, [
            new AttendanceEntry(Guid.NewGuid(), AttendanceStatus.Present, null, null)
        ]);
        var act = async () => await CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<NotFoundException>();
        _attendanceRepo.Verify(r => r.AddAsync(It.IsAny<AttendanceRecord>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_an_enrollment_does_not_belong_to_batch()
    {
        var sessionId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        _sessionRepo.SetupGetById(sessionId, ExistingSession(sessionId, batchId));

        var enrollmentInBatch = Guid.NewGuid();
        var enrollmentNotInBatch = Guid.NewGuid();
        SetupEnrollmentQuery(new[]
        {
            Enrollment(enrollmentInBatch, batchId)
        });

        var cmd = new MarkAttendanceCommand(sessionId, [
            new AttendanceEntry(enrollmentInBatch, AttendanceStatus.Present, null, null),
            new AttendanceEntry(enrollmentNotInBatch, AttendanceStatus.Absent, null, null)
        ]);
        var act = async () => await CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("One or more enrollments do not belong to this session's batch.");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_practical_hours_exceeds_session_duration()
    {
        var sessionId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        // Session is 9:00 -> 11:00 (2 hours)
        _sessionRepo.SetupGetById(sessionId, ExistingSession(sessionId, batchId, new TimeOnly(11, 0)));

        var enrollmentId = Guid.NewGuid();
        SetupEnrollmentQuery(new[] { Enrollment(enrollmentId, batchId) });

        var cmd = new MarkAttendanceCommand(sessionId, [
            new AttendanceEntry(enrollmentId, AttendanceStatus.Present, 2.25m, null)
        ]);
        var act = async () => await CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Practical hours cannot exceed the session duration (2 hours).");
        _attendanceRepo.Verify(r => r.AddAsync(It.IsAny<AttendanceRecord>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Allows_practical_hours_equal_to_session_duration()
    {
        var sessionId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        // Session is 9:00 -> 11:00 (2 hours)
        _sessionRepo.SetupGetById(sessionId, ExistingSession(sessionId, batchId, new TimeOnly(11, 0)));

        var enrollmentId = Guid.NewGuid();
        SetupEnrollmentQuery(new[] { Enrollment(enrollmentId, batchId) });
        SetupAttendanceQuery(Array.Empty<AttendanceRecord>());

        _attendanceRepo.Setup(r => r.AddAsync(It.IsAny<AttendanceRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AttendanceRecord a, CancellationToken _) => a);

        var cmd = new MarkAttendanceCommand(sessionId, [
            new AttendanceEntry(enrollmentId, AttendanceStatus.Present, 2.0m, null)
        ]);

        await CreateHandler().Handle(cmd, default);

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Inserts_new_attendance_records_when_none_exist()
    {
        var sessionId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        _sessionRepo.SetupGetById(sessionId, ExistingSession(sessionId, batchId));

        var enrollmentId1 = Guid.NewGuid();
        var enrollmentId2 = Guid.NewGuid();
        SetupEnrollmentQuery(new[]
        {
            Enrollment(enrollmentId1, batchId),
            Enrollment(enrollmentId2, batchId)
        });
        SetupAttendanceQuery(Array.Empty<AttendanceRecord>());

        var captured = new List<AttendanceRecord>();
        _attendanceRepo.Setup(r => r.AddAsync(It.IsAny<AttendanceRecord>(), It.IsAny<CancellationToken>()))
            .Callback<AttendanceRecord, CancellationToken>((a, _) => captured.Add(a))
            .ReturnsAsync((AttendanceRecord a, CancellationToken _) => a);

        var cmd = new MarkAttendanceCommand(sessionId, [
            new AttendanceEntry(enrollmentId1, AttendanceStatus.Present, 2.5m, "  Good  "),
            new AttendanceEntry(enrollmentId2, AttendanceStatus.Absent, null, null)
        ]);

        await CreateHandler().Handle(cmd, default);

        captured.Should().HaveCount(2);
        captured.Should().ContainSingle(a =>
            a.EnrollmentId == enrollmentId1
            && a.ClassSessionId == sessionId
            && a.Status == AttendanceStatus.Present
            && a.PracticalHours == 2.5m
            && a.Remarks == "Good");
        captured.Should().ContainSingle(a =>
            a.EnrollmentId == enrollmentId2
            && a.ClassSessionId == sessionId
            && a.Status == AttendanceStatus.Absent
            && a.PracticalHours == null
            && a.Remarks == null);

        _attendanceRepo.Verify(r => r.Update(It.IsAny<AttendanceRecord>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Updates_existing_attendance_records_when_re_marking()
    {
        var sessionId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        _sessionRepo.SetupGetById(sessionId, ExistingSession(sessionId, batchId));

        var enrollmentId = Guid.NewGuid();
        SetupEnrollmentQuery(new[] { Enrollment(enrollmentId, batchId) });

        var existing = new AttendanceRecord
        {
            Id = Guid.NewGuid(),
            ClassSessionId = sessionId,
            EnrollmentId = enrollmentId,
            Status = AttendanceStatus.Absent,
            PracticalHours = null,
            Remarks = "Old"
        };
        SetupAttendanceQuery(new[] { existing });

        var cmd = new MarkAttendanceCommand(sessionId, [
            new AttendanceEntry(enrollmentId, AttendanceStatus.Present, 3.0m, "Updated")
        ]);

        await CreateHandler().Handle(cmd, default);

        existing.Status.Should().Be(AttendanceStatus.Present);
        existing.PracticalHours.Should().Be(3.0m);
        existing.Remarks.Should().Be("Updated");

        _attendanceRepo.Verify(r => r.Update(existing), Times.Once);
        _attendanceRepo.Verify(r => r.AddAsync(It.IsAny<AttendanceRecord>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Deduplicates_entries_keeping_last_for_same_enrollment()
    {
        var sessionId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        _sessionRepo.SetupGetById(sessionId, ExistingSession(sessionId, batchId));

        var enrollmentId = Guid.NewGuid();
        SetupEnrollmentQuery(new[] { Enrollment(enrollmentId, batchId) });
        SetupAttendanceQuery(Array.Empty<AttendanceRecord>());

        var captured = new List<AttendanceRecord>();
        _attendanceRepo.Setup(r => r.AddAsync(It.IsAny<AttendanceRecord>(), It.IsAny<CancellationToken>()))
            .Callback<AttendanceRecord, CancellationToken>((a, _) => captured.Add(a))
            .ReturnsAsync((AttendanceRecord a, CancellationToken _) => a);

        var cmd = new MarkAttendanceCommand(sessionId, [
            new AttendanceEntry(enrollmentId, AttendanceStatus.Absent, null, "first"),
            new AttendanceEntry(enrollmentId, AttendanceStatus.Present, 1m, "last")
        ]);

        await CreateHandler().Handle(cmd, default);

        captured.Should().HaveCount(1);
        captured[0].Status.Should().Be(AttendanceStatus.Present);
        captured[0].Remarks.Should().Be("last");
    }

    [Fact]
    public async Task Trims_remarks_and_normalizes_whitespace_to_null()
    {
        var sessionId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        _sessionRepo.SetupGetById(sessionId, ExistingSession(sessionId, batchId));

        var enrollmentId1 = Guid.NewGuid();
        var enrollmentId2 = Guid.NewGuid();
        SetupEnrollmentQuery(new[]
        {
            Enrollment(enrollmentId1, batchId),
            Enrollment(enrollmentId2, batchId)
        });
        SetupAttendanceQuery(Array.Empty<AttendanceRecord>());

        var captured = new List<AttendanceRecord>();
        _attendanceRepo.Setup(r => r.AddAsync(It.IsAny<AttendanceRecord>(), It.IsAny<CancellationToken>()))
            .Callback<AttendanceRecord, CancellationToken>((a, _) => captured.Add(a))
            .ReturnsAsync((AttendanceRecord a, CancellationToken _) => a);

        var cmd = new MarkAttendanceCommand(sessionId, [
            new AttendanceEntry(enrollmentId1, AttendanceStatus.Present, null, "  trimmed  "),
            new AttendanceEntry(enrollmentId2, AttendanceStatus.Late, null, "   ")
        ]);

        await CreateHandler().Handle(cmd, default);

        captured.Single(a => a.EnrollmentId == enrollmentId1).Remarks.Should().Be("trimmed");
        captured.Single(a => a.EnrollmentId == enrollmentId2).Remarks.Should().BeNull();
    }
}
