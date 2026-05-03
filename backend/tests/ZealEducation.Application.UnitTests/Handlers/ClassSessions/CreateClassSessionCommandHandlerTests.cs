using System.Collections;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.ClassSessions.Commands.CreateClassSession;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.ClassSessions;

public class CreateClassSessionCommandHandlerTests
{
    private readonly Mock<IRepository<Batch>> _batchRepo = new();
    private readonly Mock<IRepository<ClassSession>> _sessionRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private CreateClassSessionCommandHandler CreateHandler() =>
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
        StartDate = start ?? new DateOnly(2030, 1, 1),
        EndDate = end ?? new DateOnly(2030, 6, 1),
        Status = status
    };

    private static CreateClassSessionCommand Cmd(
        Guid batchId,
        DateOnly? sessionDate = null,
        TimeOnly? startTime = null,
        TimeOnly? endTime = null,
        string? topic = "Intro",
        string? location = "Room A") => new(
            batchId,
            sessionDate ?? new DateOnly(2030, 1, 15),
            startTime ?? new TimeOnly(9, 0),
            endTime ?? new TimeOnly(11, 0),
            topic,
            location);

    private void SetupSessionQuery(IEnumerable<ClassSession> data)
    {
        var queryable = new TestAsyncEnumerable<ClassSession>(data);
        _sessionRepo.Setup(r => r.Query()).Returns(queryable);
    }

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
    public async Task Throws_ConflictException_when_session_date_before_batch_start()
    {
        var batchId = Guid.NewGuid();
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId,
            start: new DateOnly(2030, 2, 1), end: new DateOnly(2030, 6, 1)));

        var act = async () => await CreateHandler().Handle(
            Cmd(batchId, sessionDate: new DateOnly(2030, 1, 15)), default);

        await act.Should().ThrowAsync<ConflictException>()
            .Where(e => e.Message.Contains("must fall within the batch period"));
    }

    [Fact]
    public async Task Throws_ConflictException_when_session_date_after_batch_end()
    {
        var batchId = Guid.NewGuid();
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId,
            start: new DateOnly(2030, 1, 1), end: new DateOnly(2030, 6, 1)));

        var act = async () => await CreateHandler().Handle(
            Cmd(batchId, sessionDate: new DateOnly(2030, 7, 1)), default);

        await act.Should().ThrowAsync<ConflictException>()
            .Where(e => e.Message.Contains("must fall within the batch period"));
    }

    [Fact]
    public async Task Throws_ConflictException_when_session_overlaps_with_existing_session()
    {
        var batchId = Guid.NewGuid();
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId));

        SetupSessionQuery(new[]
        {
            new ClassSession
            {
                Id = Guid.NewGuid(),
                BatchId = batchId,
                SessionDate = new DateOnly(2030, 1, 15),
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(12, 0)
            }
        });

        var act = async () => await CreateHandler().Handle(
            Cmd(batchId,
                sessionDate: new DateOnly(2030, 1, 15),
                startTime: new TimeOnly(11, 0),
                endTime: new TimeOnly(13, 0)),
            default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("This session overlaps with another session on the same date.");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Creates_session_when_command_is_valid()
    {
        var batchId = Guid.NewGuid();
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId));
        SetupSessionQuery(Array.Empty<ClassSession>());

        ClassSession? captured = null;
        _sessionRepo.Setup(r => r.AddAsync(It.IsAny<ClassSession>(), It.IsAny<CancellationToken>()))
            .Callback<ClassSession, CancellationToken>((s, _) => captured = s)
            .ReturnsAsync((ClassSession s, CancellationToken _) => s);

        var sessionId = await CreateHandler().Handle(
            Cmd(batchId,
                sessionDate: new DateOnly(2030, 1, 15),
                startTime: new TimeOnly(9, 0),
                endTime: new TimeOnly(11, 0),
                topic: "Algebra",
                location: "Hall 3"),
            default);

        captured.Should().NotBeNull();
        captured!.Id.Should().Be(sessionId);
        captured.BatchId.Should().Be(batchId);
        captured.SessionDate.Should().Be(new DateOnly(2030, 1, 15));
        captured.StartTime.Should().Be(new TimeOnly(9, 0));
        captured.EndTime.Should().Be(new TimeOnly(11, 0));
        captured.Topic.Should().Be("Algebra");
        captured.Location.Should().Be("Hall 3");
        captured.Status.Should().Be(ClassSessionStatus.Scheduled);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Trims_topic_and_location_and_normalizes_whitespace_to_null()
    {
        var batchId = Guid.NewGuid();
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId));
        SetupSessionQuery(Array.Empty<ClassSession>());

        ClassSession? captured = null;
        _sessionRepo.Setup(r => r.AddAsync(It.IsAny<ClassSession>(), It.IsAny<CancellationToken>()))
            .Callback<ClassSession, CancellationToken>((s, _) => captured = s)
            .ReturnsAsync((ClassSession s, CancellationToken _) => s);

        await CreateHandler().Handle(
            Cmd(batchId, topic: "  Topic 1  ", location: "   "),
            default);

        captured!.Topic.Should().Be("Topic 1");
        captured.Location.Should().BeNull();
    }
}

internal sealed class TestAsyncEnumerable<T> : IOrderedQueryable<T>, IAsyncEnumerable<T>
{
    private readonly IQueryable<T> _inner;

    public TestAsyncEnumerable(IEnumerable<T> source) => _inner = source.AsQueryable();
    public TestAsyncEnumerable(Expression expression) => _inner = new EnumerableQuery<T>(expression);

    public Type ElementType => typeof(T);
    public Expression Expression => _inner.Expression;
    public IQueryProvider Provider => new TestAsyncQueryProvider<T>(_inner.Provider);

    public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new TestAsyncEnumerator<T>(_inner.GetEnumerator());
}

internal sealed class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync() => new(_inner.MoveNext());

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }
}

internal sealed class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

    public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TEntity>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        => new TestAsyncEnumerable<TElement>(expression);

    public object? Execute(Expression expression) => _inner.Execute(expression);

    public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        var expectedResultType = typeof(TResult).GetGenericArguments().FirstOrDefault();
        if (expectedResultType is null)
        {
            return (TResult)(object)Task.FromResult(_inner.Execute(expression));
        }

        var executionResult = _inner.Execute(expression);
        return (TResult)typeof(Task)
            .GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(expectedResultType)
            .Invoke(null, new[] { executionResult })!;
    }
}
