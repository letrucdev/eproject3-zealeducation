using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.Batches.Commands.DeleteBatch;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.Batches;

public class DeleteBatchCommandHandlerTests
{
    private readonly Mock<IRepository<Batch>> _batchRepo = new();
    private readonly Mock<IRepository<Enrollment>> _enrollmentRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private DeleteBatchCommandHandler CreateHandler() =>
        new(_batchRepo.Object, _enrollmentRepo.Object, _uow.Object);

    private static Batch ExistingBatch(Guid batchId, Guid? facultyId = null) => new()
    {
        Id = batchId,
        BatchCode = "B-001",
        CourseId = Guid.NewGuid(),
        FacultyId = facultyId,
        StartDate = new DateOnly(2030, 1, 1),
        EndDate = new DateOnly(2030, 6, 1)
    };

    [Fact]
    public async Task Throws_NotFoundException_when_batch_does_not_exist()
    {
        var batchId = Guid.NewGuid();
        _batchRepo.SetupGetById(batchId, null);

        var act = async () => await CreateHandler().Handle(new DeleteBatchCommand(batchId), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _batchRepo.Verify(r => r.Delete(It.IsAny<Batch>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_batch_has_assigned_faculty()
    {
        var batchId = Guid.NewGuid();
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId, facultyId: Guid.NewGuid()));

        var act = async () => await CreateHandler().Handle(new DeleteBatchCommand(batchId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Cannot delete a batch that has an assigned faculty.");
        _batchRepo.Verify(r => r.Delete(It.IsAny<Batch>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_batch_has_enrollments()
    {
        var batchId = Guid.NewGuid();
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId));
        _enrollmentRepo.SetupFind(new[]
        {
            new Enrollment { Id = Guid.NewGuid(), BatchId = batchId, CandidateId = Guid.NewGuid(), CourseId = Guid.NewGuid() }
        });

        var act = async () => await CreateHandler().Handle(new DeleteBatchCommand(batchId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Cannot delete a batch that has enrollments.");
        _batchRepo.Verify(r => r.Delete(It.IsAny<Batch>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Deletes_batch_when_command_is_valid()
    {
        var batchId = Guid.NewGuid();
        var batch = ExistingBatch(batchId);
        _batchRepo.SetupGetById(batchId, batch);
        _enrollmentRepo.SetupFind(Array.Empty<Enrollment>());

        await CreateHandler().Handle(new DeleteBatchCommand(batchId), default);

        _batchRepo.Verify(r => r.Delete(batch), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
