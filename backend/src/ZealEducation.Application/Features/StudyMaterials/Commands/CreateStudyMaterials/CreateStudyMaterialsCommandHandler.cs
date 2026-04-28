using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.StudyMaterials.Common;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.StudyMaterials.Commands.CreateStudyMaterials;

public class CreateStudyMaterialsCommandHandler(
    IRepository<StudyMaterial> materialRepository,
    IRepository<Course> courseRepository,
    IRepository<Staff> staffRepository,
    ICurrentUser currentUser,
    IFileStorageService fileStorage,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateStudyMaterialsCommand, CreateStudyMaterialsResponse>
{
    public async Task<CreateStudyMaterialsResponse> Handle(CreateStudyMaterialsCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
            throw new UnauthorizedException("Current user could not be resolved.");

        var staff = await staffRepository.Query()
            .FirstOrDefaultAsync(s => s.UserAccountId == currentUser.UserId.Value, cancellationToken)
            ?? throw new ConflictException("Current user is not registered as staff.");

        var distinctCourseIds = request.CourseIds.Distinct().ToList();
        var courses = await courseRepository.Query()
            .Where(c => distinctCourseIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        if (courses.Count != distinctCourseIds.Count)
            throw new NotFoundException("One or more selected courses could not be found.");

        var inactive = courses.Where(c => !c.IsActive).Select(c => c.CourseName).ToList();
        if (inactive.Count > 0)
            throw new ConflictException($"The following courses are inactive and cannot receive materials: {string.Join(", ", inactive)}.");

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
        var objectKey = $"study-materials/{uploadedAt:yyyy/MM}/{Guid.NewGuid()}{extension}";
        var title = request.Title.Trim();
        var storedFileName = MaterialFileRules.BuildFileName(title, request.FileName);

        await fileStorage.UploadAsync(bytes, objectKey, contentType, cancellationToken);

        var materials = distinctCourseIds.Select(courseId => new StudyMaterial
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            UploadedByStaffId = staff.Id,
            Title = title,
            FileName = storedFileName,
            FilePath = objectKey,
            FileType = fileType,
            FileSizeMb = fileSizeMb,
            IsActive = true,
            UploadedAt = uploadedAt
        }).ToList();

        foreach (var material in materials)
        {
            await materialRepository.AddAsync(material, cancellationToken);
        }

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            try { await fileStorage.DeleteAsync(objectKey, CancellationToken.None); } catch { /* best-effort cleanup */ }
            throw;
        }

        return new CreateStudyMaterialsResponse
        {
            MaterialIds = materials.Select(m => m.Id).ToList(),
            FilePath = objectKey,
            FileSizeMb = fileSizeMb
        };
    }
}
