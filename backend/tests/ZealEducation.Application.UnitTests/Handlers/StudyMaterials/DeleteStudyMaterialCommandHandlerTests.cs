using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.StudyMaterials.Commands.DeleteStudyMaterial;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.StudyMaterials;

public class DeleteStudyMaterialCommandHandlerTests
{
    private readonly Mock<IRepository<StudyMaterial>> _materialRepo = new();
    private readonly Mock<IFileStorageService> _fileStorage = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private DeleteStudyMaterialCommandHandler CreateHandler() =>
        new(_materialRepo.Object, _fileStorage.Object, _uow.Object);

    private void SetupMaterialQuery(IEnumerable<StudyMaterial> data)
    {
        var queryable = new TestAsyncEnumerable<StudyMaterial>(data);
        _materialRepo.Setup(r => r.Query()).Returns(queryable);
    }

    private static StudyMaterial NewMaterial(Guid id, string filePath) => new()
    {
        Id = id,
        CourseId = Guid.NewGuid(),
        UploadedByStaffId = Guid.NewGuid(),
        Title = "Lecture",
        FileName = "lecture.pdf",
        FilePath = filePath,
        FileType = "pdf",
        FileSizeMb = 1m,
        UploadedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task Throws_NotFoundException_when_material_does_not_exist()
    {
        var materialId = Guid.NewGuid();
        SetupMaterialQuery(Array.Empty<StudyMaterial>());

        var act = async () => await CreateHandler().Handle(
            new DeleteStudyMaterialCommand(materialId),
            default);

        await act.Should().ThrowAsync<NotFoundException>();
        _materialRepo.Verify(r => r.Delete(It.IsAny<StudyMaterial>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _fileStorage.Verify(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Deletes_material_and_removes_file_when_no_siblings()
    {
        var materialId = Guid.NewGuid();
        var path = "study-materials/2030/01/file.pdf";
        var material = NewMaterial(materialId, path);
        SetupMaterialQuery(new[] { material });

        await CreateHandler().Handle(new DeleteStudyMaterialCommand(materialId), default);

        _materialRepo.Verify(r => r.Delete(material), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _fileStorage.Verify(s => s.DeleteAsync(path, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Deletes_all_siblings_sharing_file_path()
    {
        var materialId = Guid.NewGuid();
        var path = "study-materials/2030/01/shared.pdf";
        var material = NewMaterial(materialId, path);
        var sibling = NewMaterial(Guid.NewGuid(), path);
        SetupMaterialQuery(new[] { material, sibling });

        await CreateHandler().Handle(new DeleteStudyMaterialCommand(materialId), default);

        _materialRepo.Verify(r => r.Delete(material), Times.Once);
        _materialRepo.Verify(r => r.Delete(sibling), Times.Once);
        _fileStorage.Verify(s => s.DeleteAsync(path, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
