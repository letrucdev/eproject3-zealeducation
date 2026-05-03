using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.StudyMaterials.Commands.CreateStudyMaterials;
using ZealEducation.Application.UnitTests.Handlers.Batches;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.StudyMaterials;

public class CreateStudyMaterialsCommandHandlerTests
{
    private readonly Mock<IRepository<StudyMaterial>> _materialRepo = new();
    private readonly Mock<IRepository<Course>> _courseRepo = new();
    private readonly Mock<IRepository<Staff>> _staffRepo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IFileStorageService> _fileStorage = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private CreateStudyMaterialsCommandHandler CreateHandler() => new(
        _materialRepo.Object,
        _courseRepo.Object,
        _staffRepo.Object,
        _currentUser.Object,
        _fileStorage.Object,
        _uow.Object);

    private static CreateStudyMaterialsCommand Cmd(
        IReadOnlyList<Guid>? courseIds = null,
        string title = "  Lecture Notes  ",
        string fileName = "notes.pdf",
        string contentType = "application/pdf",
        byte[]? bytes = null)
    {
        bytes ??= new byte[] { 1, 2, 3, 4 };
        return new CreateStudyMaterialsCommand(
            title,
            courseIds ?? new[] { Guid.NewGuid() },
            new MemoryStream(bytes),
            contentType,
            fileName,
            bytes.Length);
    }

    private void SetupStaffQuery(IEnumerable<Staff> data)
    {
        var queryable = new TestAsyncEnumerable<Staff>(data);
        _staffRepo.Setup(r => r.Query()).Returns(queryable);
    }

    private void SetupCourseQuery(IEnumerable<Course> data)
    {
        var queryable = new TestAsyncEnumerable<Course>(data);
        _courseRepo.Setup(r => r.Query()).Returns(queryable);
    }

    private static Staff NewStaff(Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        UserAccountId = userId,
        Position = "Lecturer",
        Department = "CS",
        JoinedDate = new DateOnly(2020, 1, 1),
        IsActive = true
    };

    private static Course NewCourse(Guid id, bool active = true, string name = "C#") => new()
    {
        Id = id,
        CourseName = name,
        DurationWeeks = 8,
        BaseFee = 100m,
        IsActive = active
    };

    [Fact]
    public async Task Throws_UnauthorizedException_when_current_user_is_null()
    {
        _currentUser.SetupGet(u => u.UserId).Returns((Guid?)null);

        var act = async () => await CreateHandler().Handle(Cmd(), default);

        await act.Should().ThrowAsync<UnauthorizedException>();
        _materialRepo.Verify(r => r.AddAsync(It.IsAny<StudyMaterial>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_current_user_is_not_staff()
    {
        _currentUser.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        SetupStaffQuery(Array.Empty<Staff>());

        var act = async () => await CreateHandler().Handle(Cmd(), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Current user is not registered as staff.");
    }

    [Fact]
    public async Task Throws_NotFoundException_when_some_courses_are_missing()
    {
        var userId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        SetupStaffQuery(new[] { NewStaff(userId) });

        var existingCourseId = Guid.NewGuid();
        var missingCourseId = Guid.NewGuid();
        SetupCourseQuery(new[] { NewCourse(existingCourseId) });

        var act = async () => await CreateHandler().Handle(
            Cmd(courseIds: new[] { existingCourseId, missingCourseId }),
            default);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("One or more selected courses could not be found.");
        _fileStorage.Verify(s => s.UploadAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_any_course_is_inactive()
    {
        var userId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        SetupStaffQuery(new[] { NewStaff(userId) });

        var activeId = Guid.NewGuid();
        var inactiveId = Guid.NewGuid();
        SetupCourseQuery(new[]
        {
            NewCourse(activeId, active: true, name: "Active"),
            NewCourse(inactiveId, active: false, name: "Inactive")
        });

        var act = async () => await CreateHandler().Handle(
            Cmd(courseIds: new[] { activeId, inactiveId }),
            default);

        await act.Should().ThrowAsync<ConflictException>()
            .Where(e => e.Message.Contains("Inactive"));
        _fileStorage.Verify(s => s.UploadAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Creates_one_material_per_course_and_uploads_file_once()
    {
        var userId = Guid.NewGuid();
        var staff = NewStaff(userId);
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        SetupStaffQuery(new[] { staff });

        var c1 = Guid.NewGuid();
        var c2 = Guid.NewGuid();
        SetupCourseQuery(new[] { NewCourse(c1), NewCourse(c2) });

        var added = new List<StudyMaterial>();
        _materialRepo.Setup(r => r.AddAsync(It.IsAny<StudyMaterial>(), It.IsAny<CancellationToken>()))
            .Callback<StudyMaterial, CancellationToken>((m, _) => added.Add(m))
            .ReturnsAsync((StudyMaterial m, CancellationToken _) => m);

        _fileStorage.Setup(s => s.UploadAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("fake-url");

        var bytes = new byte[1024];
        var response = await CreateHandler().Handle(
            Cmd(courseIds: new[] { c1, c2 }, bytes: bytes),
            default);

        response.MaterialIds.Should().HaveCount(2);
        response.FilePath.Should().StartWith("study-materials/");
        response.FileSizeMb.Should().BeGreaterThanOrEqualTo(0m);

        added.Should().HaveCount(2);
        added.Select(m => m.CourseId).Should().BeEquivalentTo(new[] { c1, c2 });
        added.Should().OnlyContain(m => m.UploadedByStaffId == staff.Id);
        added.Should().OnlyContain(m => m.IsActive);
        added.Should().OnlyContain(m => m.Title == "Lecture Notes");
        added.Should().OnlyContain(m => m.FilePath == response.FilePath);
        added.Should().OnlyContain(m => m.FileType == "pdf");

        _fileStorage.Verify(s => s.UploadAsync(
            It.IsAny<byte[]>(),
            It.Is<string>(k => k.StartsWith("study-materials/")),
            "application/pdf",
            It.IsAny<CancellationToken>()),
            Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Deduplicates_course_ids_before_creating_materials()
    {
        var userId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        SetupStaffQuery(new[] { NewStaff(userId) });

        var courseId = Guid.NewGuid();
        SetupCourseQuery(new[] { NewCourse(courseId) });

        var added = new List<StudyMaterial>();
        _materialRepo.Setup(r => r.AddAsync(It.IsAny<StudyMaterial>(), It.IsAny<CancellationToken>()))
            .Callback<StudyMaterial, CancellationToken>((m, _) => added.Add(m))
            .ReturnsAsync((StudyMaterial m, CancellationToken _) => m);

        _fileStorage.Setup(s => s.UploadAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("fake-url");

        await CreateHandler().Handle(
            Cmd(courseIds: new[] { courseId, courseId }),
            default);

        added.Should().HaveCount(1);
    }

    [Fact]
    public async Task Cleans_up_uploaded_file_when_save_fails()
    {
        var userId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        SetupStaffQuery(new[] { NewStaff(userId) });

        var courseId = Guid.NewGuid();
        SetupCourseQuery(new[] { NewCourse(courseId) });

        _materialRepo.SetupAdd();
        _fileStorage.Setup(s => s.UploadAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("fake-url");

        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db down"));

        var act = async () => await CreateHandler().Handle(
            Cmd(courseIds: new[] { courseId }),
            default);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _fileStorage.Verify(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Resolves_content_type_from_extension_when_request_provides_octet_stream()
    {
        var userId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        SetupStaffQuery(new[] { NewStaff(userId) });

        var courseId = Guid.NewGuid();
        SetupCourseQuery(new[] { NewCourse(courseId) });

        _materialRepo.SetupAdd();
        _fileStorage.Setup(s => s.UploadAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("fake-url");

        await CreateHandler().Handle(
            Cmd(courseIds: new[] { courseId }, fileName: "image.png", contentType: "application/octet-stream"),
            default);

        _fileStorage.Verify(s => s.UploadAsync(
            It.IsAny<byte[]>(),
            It.IsAny<string>(),
            "image/png",
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
