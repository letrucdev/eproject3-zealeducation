using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.ClassSessions.Commands.CreateBulkSessions;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.ClassSessions;

public class CreateBulkSessionsCommandHandlerTests
{
    private readonly Mock<IRepository<Batch>> _batchRepo = new();
    private readonly Mock<IRepository<ClassSession>> _sessionRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private CreateBulkSessionsCommandHandler CreateHandler() =>
        new(_batchRepo.Object, _sessionRepo.Object, _uow.Object);

    private static Batch ExistingBatch(
        Guid batchId,
        BatchStatus status = BatchStatus.Active,
        DateOnly? start = null,
        DateOnly? end = null) => new()
    {
        Id = batchId,
        BatchCode = "B-001",
        CourseId = Guid.NewGuid(),
        StartDate = start ?? new DateOnly(2030, 1, 6),
        EndDate = end ?? new DateOnly(2030, 1, 19),
        Status = status
    };

    private void SetupSessionQuery(IEnumerable<ClassSession> data)
    {
        var queryable = new TestAsyncEnumerable<ClassSession>(data);
        _sessionRepo.Setup(r => r.Query()).Returns(queryable);
    }

    private static CreateBulkSessionsCommand Cmd(
        Guid batchId,
        IReadOnlyList<DayOfWeek>? days = null,
        TimeOnly? startTime = null,
        TimeOnly? endTime = null,
        string? topic = "Topic",
        string? location = "Room A") => new(
            batchId,
            days ?? new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday },
            startTime ?? new TimeOnly(9, 0),
            endTime ?? new TimeOnly(11, 0),
            topic,
            location);

    [Fact]
    public async Task Throws_NotFoundException_when_batch_does_not_exist()
    {
        var batchId = Guid.NewGuid();
        _batchRepo.SetupGetById(batchId, null);

        var act = async () => await CreateHandler().Handle(Cmd(batchId), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _sessionRepo.Verify(r => r.AddAsync(It.IsAny<ClassSession>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_batch_is_completed()
    {
        var batchId = Guid.NewGuid();
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId, status: BatchStatus.Completed));

        var act = async () => await CreateHandler().Handle(Cmd(batchId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Cannot add sessions to a completed or cancelled batch.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_batch_is_cancelled()
    {
        var batchId = Guid.NewGuid();
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId, status: BatchStatus.Cancelled));

        var act = async () => await CreateHandler().Handle(Cmd(batchId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Cannot add sessions to a completed or cancelled batch.");
    }

    [Fact]
    public async Task Generates_sessions_only_on_selected_weekdays_within_batch_range()
    {
        var batchId = Guid.NewGuid();
        var start = new DateOnly(2030, 1, 6);  // Sunday
        var end = new DateOnly(2030, 1, 19);   // Sunday — two full weeks
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId, start: start, end: end));
        SetupSessionQuery(Array.Empty<ClassSession>());

        var captured = new List<ClassSession>();
        _sessionRepo.Setup(r => r.AddAsync(It.IsAny<ClassSession>(), It.IsAny<CancellationToken>()))
            .Callback<ClassSession, CancellationToken>((s, _) => captured.Add(s))
            .ReturnsAsync((ClassSession s, CancellationToken _) => s);

        var response = await CreateHandler().Handle(
            Cmd(batchId, days: new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday }),
            default);

        // Mondays: 7, 14. Wednesdays: 8, 15. => 4 sessions
        response.CreatedCount.Should().Be(4);
        response.SkippedDates.Should().BeEmpty();
        captured.Should().HaveCount(4);
        captured.All(s => s.SessionDate >= start && s.SessionDate <= end).Should().BeTrue();
        captured.All(s => s.BatchId == batchId).Should().BeTrue();
        captured.All(s => s.Status == ClassSessionStatus.Scheduled).Should().BeTrue();
        captured.Select(s => s.SessionDate).Should().BeEquivalentTo(new[]
        {
            new DateOnly(2030, 1, 7),
            new DateOnly(2030, 1, 9),
            new DateOnly(2030, 1, 14),
            new DateOnly(2030, 1, 16)
        });
        captured.All(s => s.SessionDate.DayOfWeek == DayOfWeek.Monday
            || s.SessionDate.DayOfWeek == DayOfWeek.Wednesday).Should().BeTrue();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Skips_dates_that_overlap_with_existing_sessions()
    {
        var batchId = Guid.NewGuid();
        var start = new DateOnly(2030, 1, 6);
        var end = new DateOnly(2030, 1, 19);
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId, start: start, end: end));

        SetupSessionQuery(new[]
        {
            new ClassSession
            {
                Id = Guid.NewGuid(),
                BatchId = batchId,
                SessionDate = new DateOnly(2030, 1, 7),
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(12, 0)
            }
        });

        var captured = new List<ClassSession>();
        _sessionRepo.Setup(r => r.AddAsync(It.IsAny<ClassSession>(), It.IsAny<CancellationToken>()))
            .Callback<ClassSession, CancellationToken>((s, _) => captured.Add(s))
            .ReturnsAsync((ClassSession s, CancellationToken _) => s);

        var response = await CreateHandler().Handle(
            Cmd(batchId,
                days: new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday },
                startTime: new TimeOnly(11, 0),
                endTime: new TimeOnly(13, 0)),
            default);

        response.CreatedCount.Should().Be(3);
        response.SkippedDates.Should().ContainSingle()
            .Which.Should().Be(new DateOnly(2030, 1, 7));
        captured.Should().HaveCount(3);
        captured.Select(s => s.SessionDate).Should().NotContain(new DateOnly(2030, 1, 7));
    }

    [Fact]
    public async Task Skips_save_when_no_session_was_created()
    {
        var batchId = Guid.NewGuid();
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId,
            start: new DateOnly(2030, 1, 7),  // Monday
            end: new DateOnly(2030, 1, 7)));
        SetupSessionQuery(Array.Empty<ClassSession>());

        var response = await CreateHandler().Handle(
            Cmd(batchId, days: new List<DayOfWeek> { DayOfWeek.Friday }),
            default);

        response.CreatedCount.Should().Be(0);
        response.SkippedDates.Should().BeEmpty();
        _sessionRepo.Verify(r => r.AddAsync(It.IsAny<ClassSession>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Trims_topic_and_location_and_normalizes_whitespace_to_null()
    {
        var batchId = Guid.NewGuid();
        var start = new DateOnly(2030, 1, 6);
        var end = new DateOnly(2030, 1, 12);
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId, start: start, end: end));
        SetupSessionQuery(Array.Empty<ClassSession>());

        var captured = new List<ClassSession>();
        _sessionRepo.Setup(r => r.AddAsync(It.IsAny<ClassSession>(), It.IsAny<CancellationToken>()))
            .Callback<ClassSession, CancellationToken>((s, _) => captured.Add(s))
            .ReturnsAsync((ClassSession s, CancellationToken _) => s);

        await CreateHandler().Handle(
            Cmd(batchId,
                days: new List<DayOfWeek> { DayOfWeek.Monday },
                topic: "  Topic 1  ",
                location: "   "),
            default);

        captured.Should().NotBeEmpty();
        captured.All(s => s.Topic == "Topic 1").Should().BeTrue();
        captured.All(s => s.Location == null).Should().BeTrue();
    }
}
