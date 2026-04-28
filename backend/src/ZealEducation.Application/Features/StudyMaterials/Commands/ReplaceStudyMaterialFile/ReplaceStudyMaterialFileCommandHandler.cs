using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.StudyMaterials.Common;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.StudyMaterials.Commands.ReplaceStudyMaterialFile;

public class ReplaceStudyMaterialFileCommandHandler(
    IRepository<StudyMaterial> materialRepository,
    IRepository<Staff> staffRepository,
    ICurrentUser currentUser,
    IFileStorageService fileStorage,
    IUnitOfWork unitOfWork) : IRequestHandler<ReplaceStudyMaterialFileCommand>
{
    public async Task Handle(ReplaceStudyMaterialFileCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
            throw new UnauthorizedException("Current user could not be resolved.");

        var staff = await staffRepository.Query()
            .FirstOrDefaultAsync(s => s.UserAccountId == currentUser.UserId.Value, cancellationToken)
            ?? throw new ConflictException("Current user is not registered as staff.");

        var material = await materialRepository.GetByIdAsync(request.MaterialId, cancellationToken)
            ?? throw new NotFoundException(nameof(StudyMaterial), request.MaterialId);

        var oldFilePath = material.FilePath;

        using var memory = new MemoryStream();
        await request.FileContent.CopyToAsync(memory, cancellationToken);
        var bytes = memory.ToArray();
        var fileSizeMb = Math.Round(bytes.Length / (1024m * 1024m), 2, MidpointRounding.AwayFromZero);

        var extension = Path.GetExtension(request.FileName).ToLowerInvariant();
        var fileType = MaterialFileRules.ToFileType(request.FileName);
        var contentType = string.IsNullOrWhiteSpace(request.ContentType) || request.ContentType == "application/octet-stream"
            ? MaterialFileRules.ResolveContentType(request.FileName)
            : request.ContentType;

        var uploadedAt = DateTime.UtcNow;
        var newObjectKey = $"study-materials/{uploadedAt:yyyy/MM}/{Guid.NewGuid()}{extension}";

        await fileStorage.UploadAsync(bytes, newObjectKey, contentType, cancellationToken);

        material.FileName = MaterialFileRules.BuildFileName(material.Title, request.FileName);
        material.FilePath = newObjectKey;
        material.FileType = fileType;
        material.FileSizeMb = fileSizeMb;
        material.UploadedAt = uploadedAt;
        material.UploadedByStaffId = staff.Id;
        materialRepository.Update(material);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            try { await fileStorage.DeleteAsync(newObjectKey, CancellationToken.None); } catch { /* best-effort */ }
            throw;
        }

        var stillReferenced = await materialRepository.Query()
            .AnyAsync(m => m.FilePath == oldFilePath && m.Id != material.Id, cancellationToken);
        if (!stillReferenced)
        {
            try { await fileStorage.DeleteAsync(oldFilePath, CancellationToken.None); } catch { /* best-effort */ }
        }
    }
}
