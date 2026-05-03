using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.Examinations.Commands.UpdateExamination;
using ZealEducation.Application.UnitTests.Handlers.Batches;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.Examinations;

public class UpdateExaminationCommandHandlerTests
{
    private readonly Mock<IRepository<Examination>> _examRepo = new();
    private readonly Mock<IRepository<ExamResult>> _resultRepo = new();
    private readonly Mock<IRepository<Batch>> _batchRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private UpdateExaminationCommandHandler CreateHandler() =>
        new(_examRepo.Object, _resultRepo.Object, _batchRepo.Object, _uow.Object);

    private static UpdateExaminationCommand Cmd(
        Guid? examId = null,
        string examName = "Updated Exam",
        DateOnly? examDate = null,
        string? location = "Room B",
        int maxScore = 100,
        int passScore = 50) => new(
            examId ?? Guid.NewGuid(),
            examName,
            examDate ?? new DateOnly(2030, 4, 15),
            location,
            maxScore,
            passScore);

    private static Examination ExistingExam(Guid id, Guid batchId) => new()
    {
        Id = id,
        BatchId = batchId,
        ExamName = "Old Name",
        ExamDate = new DateOnly(2030, 2, 1),
        Location = "Old Hall",
        MaxScore = 80,
        PassScore = 40,
        ScheduledById = Guid.NewGuid()
    };

    private static Batch ExistingBatch(
        Guid id,
        DateOnly? start = null,
        DateOnly? end = null,
        BatchStatus status = BatchStatus.Active) => new()
    {
        Id = id,
        BatchCode = "B-100",
        CourseId = Guid.NewGuid(),
        StartDate = start ?? new DateOnly(2030, 1, 1),
        EndDate = end ?? new DateOnly(2030, 6, 1),
        Status = status
    };

    private void SetupResultQuery(IEnumerable<ExamResult> data)
    {
        var queryable = new TestAsyncEnumerable<ExamResult>(data);
        _resultRepo.Setup(r => r.Query()).Returns(queryable);
    }

    private void SetupExamQuery(IEnumerable<Examination> data)
    {
        var queryable = new TestAsyncEnumerable<Examination>(data);
        _examRepo.Setup(r => r.Query()).Returns(queryable);
    }

    [Fact]
    public async Task Throws_NotFoundException_when_examination_does_not_exist()
    {
        var examId = Guid.NewGuid();
        _examRepo.SetupGetById(examId, null);

        var act = async () => await CreateHandler().Handle(Cmd(examId), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _examRepo.Verify(r => r.Update(It.IsAny<Examination>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_examination_has_results()
    {
        var examId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        _examRepo.SetupGetById(examId, ExistingExam(examId, batchId));
        SetupResultQuery(new[] { new ExamResult { Id = Guid.NewGuid(), ExamId = examId } });

        var act = async () => await CreateHandler().Handle(Cmd(examId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Cannot update an examination that already has results recorded.");
        _examRepo.Verify(r => r.Update(It.IsAny<Examination>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_NotFoundException_when_batch_does_not_exist()
    {
        var examId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        _examRepo.SetupGetById(examId, ExistingExam(examId, batchId));
        SetupResultQuery(Array.Empty<ExamResult>());
        _batchRepo.SetupGetById(batchId, null);

        var act = async () => await CreateHandler().Handle(Cmd(examId), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _examRepo.Verify(r => r.Update(It.IsAny<Examination>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_exam_date_is_before_batch_start()
    {
        var examId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        _examRepo.SetupGetById(examId, ExistingExam(examId, batchId));
        SetupResultQuery(Array.Empty<ExamResult>());
        _batchRepo.SetupGetById(batchId,
            ExistingBatch(batchId, start: new DateOnly(2030, 2, 1), end: new DateOnly(2030, 6, 1)));

        var act = async () => await CreateHandler().Handle(
            Cmd(examId, examDate: new DateOnly(2030, 1, 15)),
            default);

        await act.Should().ThrowAsync<ConflictException>()
            .Where(e => e.Message.Contains("Exam date must fall within the batch period"));
    }

    [Fact]
    public async Task Throws_ConflictException_when_exam_date_is_after_batch_end()
    {
        var examId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        _examRepo.SetupGetById(examId, ExistingExam(examId, batchId));
        SetupResultQuery(Array.Empty<ExamResult>());
        _batchRepo.SetupGetById(batchId,
            ExistingBatch(batchId, start: new DateOnly(2030, 1, 1), end: new DateOnly(2030, 6, 1)));

        var act = async () => await CreateHandler().Handle(
            Cmd(examId, examDate: new DateOnly(2030, 7, 1)),
            default);

        await act.Should().ThrowAsync<ConflictException>()
            .Where(e => e.Message.Contains("Exam date must fall within the batch period"));
    }

    [Fact]
    public async Task Throws_ConflictException_when_duplicate_examination_exists_for_another_record()
    {
        var examId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        var otherExamId = Guid.NewGuid();
        var examDate = new DateOnly(2030, 4, 15);
        _examRepo.SetupGetById(examId, ExistingExam(examId, batchId));
        SetupResultQuery(Array.Empty<ExamResult>());
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId));
        SetupExamQuery(new[]
        {
            new Examination
            {
                Id = otherExamId,
                BatchId = batchId,
                ExamName = "Updated Exam",
                ExamDate = examDate
            }
        });

        var act = async () => await CreateHandler().Handle(
            Cmd(examId, examName: "Updated Exam", examDate: examDate),
            default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("An examination with this name already exists on the same date for this batch.");
        _examRepo.Verify(r => r.Update(It.IsAny<Examination>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Updates_examination_when_command_is_valid()
    {
        var examId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        var existing = ExistingExam(examId, batchId);
        _examRepo.SetupGetById(examId, existing);
        SetupResultQuery(Array.Empty<ExamResult>());
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId));
        SetupExamQuery(Array.Empty<Examination>());

        var newDate = new DateOnly(2030, 4, 15);
        await CreateHandler().Handle(
            Cmd(examId, examName: "New Exam", examDate: newDate, location: "Room X", maxScore: 200, passScore: 100),
            default);

        existing.ExamName.Should().Be("New Exam");
        existing.ExamDate.Should().Be(newDate);
        existing.Location.Should().Be("Room X");
        existing.MaxScore.Should().Be(200);
        existing.PassScore.Should().Be(100);

        _examRepo.Verify(r => r.Update(existing), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Trims_exam_name_and_location_before_saving()
    {
        var examId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        var existing = ExistingExam(examId, batchId);
        _examRepo.SetupGetById(examId, existing);
        SetupResultQuery(Array.Empty<ExamResult>());
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId));
        SetupExamQuery(Array.Empty<Examination>());

        await CreateHandler().Handle(
            Cmd(examId, examName: "  Trimmed  ", location: "  Lab 2  "),
            default);

        existing.ExamName.Should().Be("Trimmed");
        existing.Location.Should().Be("Lab 2");
    }

    [Fact]
    public async Task Sets_location_to_null_when_input_is_whitespace()
    {
        var examId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        var existing = ExistingExam(examId, batchId);
        _examRepo.SetupGetById(examId, existing);
        SetupResultQuery(Array.Empty<ExamResult>());
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId));
        SetupExamQuery(Array.Empty<Examination>());

        await CreateHandler().Handle(Cmd(examId, location: "   "), default);

        existing.Location.Should().BeNull();
    }

    [Fact]
    public async Task Sets_location_to_null_when_input_is_null()
    {
        var examId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        var existing = ExistingExam(examId, batchId);
        _examRepo.SetupGetById(examId, existing);
        SetupResultQuery(Array.Empty<ExamResult>());
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId));
        SetupExamQuery(Array.Empty<Examination>());

        await CreateHandler().Handle(Cmd(examId, location: null), default);

        existing.Location.Should().BeNull();
    }
}
