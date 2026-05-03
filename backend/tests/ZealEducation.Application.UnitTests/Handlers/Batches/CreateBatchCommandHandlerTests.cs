using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.Batches.Commands.CreateBatch;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.Batches;

public class CreateBatchCommandHandlerTests
{
    private readonly Mock<IRepository<Batch>> _batchRepo = new();
    private readonly Mock<IRepository<Course>> _courseRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private CreateBatchCommandHandler CreateHandler() =>
        new(_batchRepo.Object, _courseRepo.Object, _uow.Object);

    private static CreateBatchCommand Cmd(
        Guid courseId,
        string code = "B-001",
        DateOnly? start = null,
        DateOnly? end = null) => new(
            code,
            courseId,
            start ?? new DateOnly(2030, 1, 1),
            end ?? new DateOnly(2030, 6, 1),
            "Hall 1",
            30);

    [Fact]
    public async Task Throws_NotFoundException_when_course_does_not_exist()
    {
        var courseId = Guid.NewGuid();
        _courseRepo.SetupExists(courseId, false);

        var act = async () => await CreateHandler().Handle(Cmd(courseId), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _batchRepo.Verify(r => r.AddAsync(It.IsAny<Batch>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_batch_code_is_already_in_use()
    {
        var courseId = Guid.NewGuid();
        _courseRepo.SetupExists(courseId, true);
        _batchRepo.SetupFind(new[] { new Batch { Id = Guid.NewGuid(), BatchCode = "B-001" } });

        var act = async () => await CreateHandler().Handle(Cmd(courseId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Batch code is already in use.");
    }

    [Fact]
    public async Task Creates_batch_with_NeedsInstructor_status_and_no_faculty()
    {
        var courseId = Guid.NewGuid();
        _courseRepo.SetupExists(courseId, true);
        _batchRepo.SetupFind(Array.Empty<Batch>());
        _batchRepo.SetupAdd();

        Batch? captured = null;
        _batchRepo.Setup(r => r.AddAsync(It.IsAny<Batch>(), It.IsAny<CancellationToken>()))
            .Callback<Batch, CancellationToken>((b, _) => captured = b)
            .ReturnsAsync((Batch b, CancellationToken _) => b);

        var response = await CreateHandler().Handle(Cmd(courseId), default);

        response.Status.Should().Be(BatchStatus.NeedsInstructor);
        captured.Should().NotBeNull();
        captured!.FacultyId.Should().BeNull();
        captured.Status.Should().Be(BatchStatus.NeedsInstructor);
        captured.BatchCode.Should().Be("B-001");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Trims_batch_code_and_location_before_saving()
    {
        var courseId = Guid.NewGuid();
        _courseRepo.SetupExists(courseId, true);
        _batchRepo.SetupFind(Array.Empty<Batch>());

        Batch? captured = null;
        _batchRepo.Setup(r => r.AddAsync(It.IsAny<Batch>(), It.IsAny<CancellationToken>()))
            .Callback<Batch, CancellationToken>((b, _) => captured = b)
            .ReturnsAsync((Batch b, CancellationToken _) => b);

        var cmd = new CreateBatchCommand("  B-002  ", courseId,
            new DateOnly(2030, 1, 1), new DateOnly(2030, 6, 1), "  Room 5  ", 25);

        await CreateHandler().Handle(cmd, default);

        captured!.BatchCode.Should().Be("B-002");
        captured.Location.Should().Be("Room 5");
    }
}
