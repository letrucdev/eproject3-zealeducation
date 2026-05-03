using System.Collections;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.Batches.Commands.AssignCandidatesToBatch;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.Batches;

public class AssignCandidatesToBatchCommandHandlerTests
{
    private readonly Mock<IRepository<Batch>> _batchRepo = new();
    private readonly Mock<IRepository<Enrollment>> _enrollmentRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private AssignCandidatesToBatchCommandHandler CreateHandler() =>
        new(_batchRepo.Object, _enrollmentRepo.Object, _uow.Object);

    private static Batch ExistingBatch(
        Guid batchId,
        Guid courseId,
        int maxCapacity = 30,
        BatchStatus status = BatchStatus.Active) => new()
    {
        Id = batchId,
        BatchCode = "B-001",
        CourseId = courseId,
        StartDate = new DateOnly(2030, 1, 1),
        EndDate = new DateOnly(2030, 6, 1),
        MaxCapacity = maxCapacity,
        Status = status
    };

    private static Enrollment NewEnrollment(
        Guid id,
        Guid courseId,
        Guid? batchId = null,
        EnrollmentStatus status = EnrollmentStatus.PendingAssignment) => new()
    {
        Id = id,
        CourseId = courseId,
        BatchId = batchId,
        CandidateId = Guid.NewGuid(),
        Status = status
    };

    private void SetupQuery(IEnumerable<Enrollment> data)
    {
        var queryable = new TestAsyncEnumerable<Enrollment>(data);
        _enrollmentRepo.Setup(r => r.Query()).Returns(queryable);
    }

    [Fact]
    public async Task Throws_NotFoundException_when_batch_does_not_exist()
    {
        var batchId = Guid.NewGuid();
        _batchRepo.SetupGetById(batchId, null);

        var cmd = new AssignCandidatesToBatchCommand(batchId, [Guid.NewGuid()]);
        var act = async () => await CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<NotFoundException>();
        _enrollmentRepo.Verify(r => r.Update(It.IsAny<Enrollment>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_batch_is_completed()
    {
        var batchId = Guid.NewGuid();
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId, Guid.NewGuid(), status: BatchStatus.Completed));

        var cmd = new AssignCandidatesToBatchCommand(batchId, [Guid.NewGuid()]);
        var act = async () => await CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Cannot assign candidates to a completed or cancelled batch.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_batch_is_cancelled()
    {
        var batchId = Guid.NewGuid();
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId, Guid.NewGuid(), status: BatchStatus.Cancelled));

        var cmd = new AssignCandidatesToBatchCommand(batchId, [Guid.NewGuid()]);
        var act = async () => await CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Cannot assign candidates to a completed or cancelled batch.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_batch_capacity_would_be_exceeded()
    {
        var batchId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var batch = ExistingBatch(batchId, courseId, maxCapacity: 2);
        _batchRepo.SetupGetById(batchId, batch);

        var existing1 = NewEnrollment(Guid.NewGuid(), courseId, batchId: batchId, status: EnrollmentStatus.Enrolled);
        var existing2 = NewEnrollment(Guid.NewGuid(), courseId, batchId: batchId, status: EnrollmentStatus.Enrolled);
        SetupQuery(new[] { existing1, existing2 });

        var cmd = new AssignCandidatesToBatchCommand(batchId, [Guid.NewGuid()]);
        var act = async () => await CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<ConflictException>()
            .Where(e => e.Message.Contains("would exceed batch capacity"));
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_NotFoundException_when_some_enrollments_are_missing()
    {
        var batchId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId, courseId, maxCapacity: 30));

        var existingEnrollmentId = Guid.NewGuid();
        var missingEnrollmentId = Guid.NewGuid();
        SetupQuery(new[]
        {
            NewEnrollment(existingEnrollmentId, courseId)
        });

        var cmd = new AssignCandidatesToBatchCommand(batchId, [existingEnrollmentId, missingEnrollmentId]);
        var act = async () => await CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("One or more enrollments were not found.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_enrollment_belongs_to_different_course()
    {
        var batchId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var otherCourseId = Guid.NewGuid();
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId, courseId));

        var enrollmentId = Guid.NewGuid();
        SetupQuery(new[]
        {
            NewEnrollment(enrollmentId, otherCourseId)
        });

        var cmd = new AssignCandidatesToBatchCommand(batchId, [enrollmentId]);
        var act = async () => await CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<ConflictException>()
            .Where(e => e.Message.Contains("different course"));
    }

    [Fact]
    public async Task Throws_ConflictException_when_enrollment_already_assigned_to_a_batch()
    {
        var batchId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId, courseId));

        var enrollmentId = Guid.NewGuid();
        SetupQuery(new[]
        {
            NewEnrollment(enrollmentId, courseId, batchId: Guid.NewGuid())
        });

        var cmd = new AssignCandidatesToBatchCommand(batchId, [enrollmentId]);
        var act = async () => await CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<ConflictException>()
            .Where(e => e.Message.Contains("already assigned to a batch"));
    }

    [Fact]
    public async Task Throws_ConflictException_when_enrollment_not_in_pending_assignment_state()
    {
        var batchId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId, courseId));

        var enrollmentId = Guid.NewGuid();
        SetupQuery(new[]
        {
            NewEnrollment(enrollmentId, courseId, status: EnrollmentStatus.Withdrawn)
        });

        var cmd = new AssignCandidatesToBatchCommand(batchId, [enrollmentId]);
        var act = async () => await CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<ConflictException>()
            .Where(e => e.Message.Contains("not in pending assignment state"));
    }

    [Fact]
    public async Task Assigns_enrollments_to_batch_when_command_is_valid()
    {
        var batchId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId, courseId, maxCapacity: 10));

        var e1 = NewEnrollment(Guid.NewGuid(), courseId);
        var e2 = NewEnrollment(Guid.NewGuid(), courseId);
        SetupQuery(new[] { e1, e2 });

        var cmd = new AssignCandidatesToBatchCommand(batchId, [e1.Id, e2.Id]);
        await CreateHandler().Handle(cmd, default);

        e1.BatchId.Should().Be(batchId);
        e1.Status.Should().Be(EnrollmentStatus.Enrolled);
        e2.BatchId.Should().Be(batchId);
        e2.Status.Should().Be(EnrollmentStatus.Enrolled);

        _enrollmentRepo.Verify(r => r.Update(e1), Times.Once);
        _enrollmentRepo.Verify(r => r.Update(e2), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Deduplicates_enrollment_ids_before_capacity_check()
    {
        var batchId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId, courseId, maxCapacity: 1));

        var e1 = NewEnrollment(Guid.NewGuid(), courseId);
        SetupQuery(new[] { e1 });

        var cmd = new AssignCandidatesToBatchCommand(batchId, [e1.Id, e1.Id]);
        await CreateHandler().Handle(cmd, default);

        e1.BatchId.Should().Be(batchId);
        e1.Status.Should().Be(EnrollmentStatus.Enrolled);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
