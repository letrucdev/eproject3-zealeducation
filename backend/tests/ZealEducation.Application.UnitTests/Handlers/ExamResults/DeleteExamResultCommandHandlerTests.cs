using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.ExamResults.Commands.DeleteExamResult;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.ExamResults;

public class DeleteExamResultCommandHandlerTests
{
    private readonly Mock<IRepository<ExamResult>> _examResultRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private DeleteExamResultCommandHandler CreateHandler() =>
        new(_examResultRepo.Object, _uow.Object);

    [Fact]
    public async Task Throws_NotFoundException_when_result_does_not_exist()
    {
        var resultId = Guid.NewGuid();
        _examResultRepo.SetupGetById(resultId, null);

        var act = async () => await CreateHandler().Handle(new DeleteExamResultCommand(resultId), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _examResultRepo.Verify(r => r.Delete(It.IsAny<ExamResult>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Deletes_result_and_saves_changes_when_result_exists()
    {
        var resultId = Guid.NewGuid();
        var existing = new ExamResult
        {
            Id = resultId,
            ExamId = Guid.NewGuid(),
            EnrollmentId = Guid.NewGuid(),
            GradedById = Guid.NewGuid(),
            Score = 70m,
            Grade = "C",
            IsPassed = true,
            GradedAt = DateTime.UtcNow
        };
        _examResultRepo.SetupGetById(resultId, existing);

        await CreateHandler().Handle(new DeleteExamResultCommand(resultId), default);

        _examResultRepo.Verify(r => r.Delete(existing), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
