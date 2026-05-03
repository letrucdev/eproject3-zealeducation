using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.Examinations.Commands.DeleteExamination;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.Examinations;

public class DeleteExaminationCommandHandlerTests
{
    private readonly Mock<IRepository<Examination>> _examRepo = new();
    private readonly Mock<IRepository<ExamResult>> _resultRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private DeleteExaminationCommandHandler CreateHandler() =>
        new(_examRepo.Object, _resultRepo.Object, _uow.Object);

    private static Examination ExistingExam(Guid id) => new()
    {
        Id = id,
        BatchId = Guid.NewGuid(),
        ExamName = "Final",
        ExamDate = new DateOnly(2030, 5, 1),
        MaxScore = 100,
        PassScore = 50,
        ScheduledById = Guid.NewGuid()
    };

    private void SetupResultQuery(IEnumerable<ExamResult> data)
    {
        var queryable = new TestAsyncEnumerable<ExamResult>(data);
        _resultRepo.Setup(r => r.Query()).Returns(queryable);
    }

    [Fact]
    public async Task Throws_NotFoundException_when_examination_does_not_exist()
    {
        var examId = Guid.NewGuid();
        _examRepo.SetupGetById(examId, null);

        var act = async () => await CreateHandler().Handle(new DeleteExaminationCommand(examId), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _examRepo.Verify(r => r.Delete(It.IsAny<Examination>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_examination_has_results()
    {
        var examId = Guid.NewGuid();
        _examRepo.SetupGetById(examId, ExistingExam(examId));
        SetupResultQuery(new[] { new ExamResult { Id = Guid.NewGuid(), ExamId = examId } });

        var act = async () => await CreateHandler().Handle(new DeleteExaminationCommand(examId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Cannot delete an examination that already has results recorded.");
        _examRepo.Verify(r => r.Delete(It.IsAny<Examination>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Deletes_examination_when_no_results_exist()
    {
        var examId = Guid.NewGuid();
        var examination = ExistingExam(examId);
        _examRepo.SetupGetById(examId, examination);
        SetupResultQuery(Array.Empty<ExamResult>());

        await CreateHandler().Handle(new DeleteExaminationCommand(examId), default);

        _examRepo.Verify(r => r.Delete(examination), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
