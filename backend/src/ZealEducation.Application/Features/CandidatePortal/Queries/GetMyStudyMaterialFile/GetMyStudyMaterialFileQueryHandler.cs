using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.CandidatePortal.Common;
using ZealEducation.Application.Features.StudyMaterials.Common;
using ZealEducation.Application.Features.StudyMaterials.Queries.GetStudyMaterialFile;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyStudyMaterialFile;

public class GetMyStudyMaterialFileQueryHandler(
    ICurrentUser currentUser,
    IRepository<Candidate> candidateRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Course> courseRepository,
    IRepository<StudyMaterial> materialRepository,
    IFileStorageService fileStorage) : IRequestHandler<GetMyStudyMaterialFileQuery, StudyMaterialFileResult>
{
    public async Task<StudyMaterialFileResult> Handle(GetMyStudyMaterialFileQuery request, CancellationToken cancellationToken)
    {
        var candidate = await CandidateResolver.ResolveAsync(currentUser, candidateRepository, cancellationToken);

        var material = await materialRepository.GetByIdAsync(request.MaterialId, cancellationToken);

        // Defense against existence-leak: same NotFoundException for every failure mode.
        if (material is null || !material.IsActive)
            throw new NotFoundException(nameof(StudyMaterial), request.MaterialId);

        var courseActive = await courseRepository.Query()
            .AnyAsync(c => c.Id == material.CourseId && c.IsActive, cancellationToken);
        if (!courseActive)
            throw new NotFoundException(nameof(StudyMaterial), request.MaterialId);

        var enrolled = await enrollmentRepository.Query()
            .AnyAsync(e => e.CandidateId == candidate.Id && e.CourseId == material.CourseId, cancellationToken);
        if (!enrolled)
            throw new NotFoundException(nameof(StudyMaterial), request.MaterialId);

        var bytes = await fileStorage.DownloadAsync(material.FilePath, cancellationToken);
        var contentType = MaterialFileRules.ResolveContentType(material.FileName);
        return new StudyMaterialFileResult(bytes, contentType, material.FileName);
    }
}
