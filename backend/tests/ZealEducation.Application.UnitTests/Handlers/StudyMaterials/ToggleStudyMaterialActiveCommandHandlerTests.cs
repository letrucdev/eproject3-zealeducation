using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.StudyMaterials.Commands.ToggleStudyMaterialActive;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.StudyMaterials;

public class ToggleStudyMaterialActiveCommandHandlerTests
{
    private readonly Mock<IRepository<StudyMaterial>> _materialRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private ToggleStudyMaterialActiveCommandHandler CreateHandler() =>
        new(_materialRepo.Object, _uow.Object);

    private static StudyMaterial NewMaterial(Guid id, bool active) => new()
    {
        Id = id,
        CourseId = Guid.NewGuid(),
        UploadedByStaffId = Guid.NewGuid(),
        Title = "Lecture",
        FileName = "lecture.pdf",
        FilePath = "study-materials/2030/01/file.pdf",
        FileType = "pdf",
        FileSizeMb = 1m,
        IsActive = active,
        UploadedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task Throws_NotFoundException_when_material_does_not_exist()
    {
        var materialId = Guid.NewGuid();
        _materialRepo.SetupGetById(materialId, null);

        var act = async () => await CreateHandler().Handle(
            new ToggleStudyMaterialActiveCommand(materialId, true),
            default);

        await act.Should().ThrowAsync<NotFoundException>();
        _materialRepo.Verify(r => r.Update(It.IsAny<StudyMaterial>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Deactivates_material_when_set_to_inactive()
    {
        var materialId = Guid.NewGuid();
        var material = NewMaterial(materialId, active: true);
        _materialRepo.SetupGetById(materialId, material);

        await CreateHandler().Handle(
            new ToggleStudyMaterialActiveCommand(materialId, false),
            default);

        material.IsActive.Should().BeFalse();
        _materialRepo.Verify(r => r.Update(material), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Reactivates_material_when_set_to_active()
    {
        var materialId = Guid.NewGuid();
        var material = NewMaterial(materialId, active: false);
        _materialRepo.SetupGetById(materialId, material);

        await CreateHandler().Handle(
            new ToggleStudyMaterialActiveCommand(materialId, true),
            default);

        material.IsActive.Should().BeTrue();
        _materialRepo.Verify(r => r.Update(material), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
