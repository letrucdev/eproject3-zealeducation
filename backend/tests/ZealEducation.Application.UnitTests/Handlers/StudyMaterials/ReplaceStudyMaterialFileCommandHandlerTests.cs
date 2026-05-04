using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.StudyMaterials.Commands.ReplaceStudyMaterialFile;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.StudyMaterials;

public class ReplaceStudyMaterialFileCommandHandlerTests
{
    private readonly Mock<IRepository<StudyMaterial>> _materialRepo = new();
    private readonly Mock<IRepository<Staff>> _staffRepo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IFileStorageService> _fileStorage = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private ReplaceStudyMaterialFileCommandHandler CreateHandler() => new(
        _materialRepo.Object,
        _staffRepo.Object,
        _currentUser.Object,
        _fileStorage.Object,
        _uow.Object);

    private static ReplaceStudyMaterialFileCommand Cmd(
        Guid materialId,
        string fileName = "replacement.pdf",
        string contentType = "application/pdf",
        byte[]? bytes = null)
    {
        bytes ??= new byte[] { 9, 9, 9 };
        return new ReplaceStudyMaterialFileCommand(
            materialId,
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

    private void SetupMaterialQuery(IEnumerable<StudyMaterial> data)
    {
        var queryable = new TestAsyncEnumerable<StudyMaterial>(data);
        _materialRepo.Setup(r => r.Query()).Returns(queryable);
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

    private static StudyMaterial ExistingMaterial(Guid id, string filePath = "study-materials/2030/01/old.pdf") => new()
    {
        Id = id,
        CourseId = Guid.NewGuid(),
        UploadedByStaffId = Guid.NewGuid(),
        Title = "Lecture",
        FileName = "lecture.pdf",
        FilePath = filePath,
        FileType = "pdf",
        FileSizeMb = 1m,
        UploadedAt = DateTime.UtcNow.AddDays(-10)
    };

    [Fact]
    public async Task Throws_UnauthorizedException_when_current_user_is_null()
    {
        _currentUser.SetupGet(u => u.UserId).Returns((Guid?)null);

        var act = async () => await CreateHandler().Handle(Cmd(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<UnauthorizedException>();
        _materialRepo.Verify(r => r.Update(It.IsAny<StudyMaterial>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_current_user_is_not_staff()
    {
        _currentUser.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        SetupStaffQuery(Array.Empty<Staff>());

        var act = async () => await CreateHandler().Handle(Cmd(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Current user is not registered as staff.");
    }

    [Fact]
    public async Task Throws_NotFoundException_when_material_does_not_exist()
    {
        var userId = Guid.NewGuid();
        var materialId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        SetupStaffQuery(new[] { NewStaff(userId) });
        _materialRepo.SetupGetById(materialId, null);

        var act = async () => await CreateHandler().Handle(Cmd(materialId), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _fileStorage.Verify(s => s.UploadAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Replaces_file_uploads_new_and_deletes_old_when_no_other_references()
    {
        var userId = Guid.NewGuid();
        var staff = NewStaff(userId);
        var materialId = Guid.NewGuid();
        var oldPath = "study-materials/2030/01/old.pdf";
        var material = ExistingMaterial(materialId, oldPath);

        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        SetupStaffQuery(new[] { staff });
        _materialRepo.SetupGetById(materialId, material);
        SetupMaterialQuery(new[] { material });

        _fileStorage.Setup(s => s.UploadAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("fake-url");

        await CreateHandler().Handle(
            Cmd(materialId, fileName: "new.png", contentType: "image/png"),
            default);

        material.FilePath.Should().NotBe(oldPath);
        material.FilePath.Should().StartWith("study-materials/");
        material.FileType.Should().Be("png");
        material.UploadedByStaffId.Should().Be(staff.Id);

        _fileStorage.Verify(s => s.UploadAsync(
            It.IsAny<byte[]>(),
            It.Is<string>(k => k == material.FilePath),
            "image/png",
            It.IsAny<CancellationToken>()),
            Times.Once);
        _fileStorage.Verify(s => s.DeleteAsync(oldPath, It.IsAny<CancellationToken>()), Times.Once);
        _materialRepo.Verify(r => r.Update(material), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Does_not_delete_old_file_when_other_materials_still_reference_it()
    {
        var userId = Guid.NewGuid();
        var staff = NewStaff(userId);
        var materialId = Guid.NewGuid();
        var oldPath = "study-materials/2030/01/shared.pdf";
        var material = ExistingMaterial(materialId, oldPath);
        var sibling = ExistingMaterial(Guid.NewGuid(), oldPath);

        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        SetupStaffQuery(new[] { staff });
        _materialRepo.SetupGetById(materialId, material);
        SetupMaterialQuery(new[] { material, sibling });

        _fileStorage.Setup(s => s.UploadAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("fake-url");

        await CreateHandler().Handle(Cmd(materialId), default);

        _fileStorage.Verify(s => s.DeleteAsync(oldPath, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Cleans_up_uploaded_file_when_save_fails()
    {
        var userId = Guid.NewGuid();
        var staff = NewStaff(userId);
        var materialId = Guid.NewGuid();
        var material = ExistingMaterial(materialId);

        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        SetupStaffQuery(new[] { staff });
        _materialRepo.SetupGetById(materialId, material);

        _fileStorage.Setup(s => s.UploadAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("fake-url");

        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db down"));

        var act = async () => await CreateHandler().Handle(Cmd(materialId), default);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _fileStorage.Verify(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}
