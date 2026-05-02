using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.StudyMaterials.Commands.UpdateStudyMaterialTitle;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.StudyMaterials;

public class UpdateStudyMaterialTitleCommandHandlerTests
{
    private readonly Mock<IRepository<StudyMaterial>> _materialRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private UpdateStudyMaterialTitleCommandHandler CreateHandler() =>
        new(_materialRepo.Object, _uow.Object);

    [Fact]
    public async Task Throws_NotFoundException_when_material_does_not_exist()
    {
        var materialId = Guid.NewGuid();
        _materialRepo.SetupGetById(materialId, null);

        var act = async () => await CreateHandler().Handle(
            new UpdateStudyMaterialTitleCommand(materialId, "New Title"),
            default);

        await act.Should().ThrowAsync<NotFoundException>();
        _materialRepo.Verify(r => r.Update(It.IsAny<StudyMaterial>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Updates_title_and_rebuilds_filename_and_saves()
    {
        var materialId = Guid.NewGuid();
        var material = new StudyMaterial
        {
            Id = materialId,
            CourseId = Guid.NewGuid(),
            UploadedByStaffId = Guid.NewGuid(),
            Title = "Old Title",
            FileName = "old_title.pdf",
            FilePath = "study-materials/2030/01/abc.pdf",
            FileType = "pdf",
            FileSizeMb = 1.5m,
            UploadedAt = DateTime.UtcNow
        };
        _materialRepo.SetupGetById(materialId, material);

        await CreateHandler().Handle(
            new UpdateStudyMaterialTitleCommand(materialId, "  New Title  "),
            default);

        material.Title.Should().Be("New Title");
        material.FileName.Should().Be("new_title.pdf");
        _materialRepo.Verify(r => r.Update(material), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
