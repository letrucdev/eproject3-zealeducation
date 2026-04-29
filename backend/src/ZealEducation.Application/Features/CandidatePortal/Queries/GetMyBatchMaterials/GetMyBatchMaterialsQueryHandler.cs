using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.CandidatePortal.Common;
using ZealEducation.Application.Features.StudyMaterials.Queries;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyBatchMaterials;

public class GetMyBatchMaterialsQueryHandler(
    ICurrentUser currentUser,
    IRepository<Candidate> candidateRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Batch> batchRepository,
    IRepository<StudyMaterial> materialRepository) : IRequestHandler<GetMyBatchMaterialsQuery, PaginatedList<StudyMaterialListItemDto>>
{
    public async Task<PaginatedList<StudyMaterialListItemDto>> Handle(GetMyBatchMaterialsQuery request, CancellationToken cancellationToken)
    {
        var candidate = await CandidateResolver.ResolveAsync(currentUser, candidateRepository, cancellationToken);

        // Single check: batch exists, course active, candidate enrolled in this batch.
        var courseId = await batchRepository.Query()
            .Where(b => b.Id == request.BatchId
                && b.Course.IsActive
                && enrollmentRepository.Query().Any(e => e.CandidateId == candidate.Id && e.BatchId == b.Id))
            .Select(b => (Guid?)b.CourseId)
            .FirstOrDefaultAsync(cancellationToken);

        if (courseId is null)
            throw new NotFoundException(nameof(StudyMaterial), request.BatchId);

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 12 : Math.Min(request.PageSize, 60);
        var search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim().ToLower();

        var query = materialRepository.Query()
            .Where(m => m.CourseId == courseId.Value && m.IsActive);

        if (search != null)
            query = query.Where(m => m.Title.ToLower().Contains(search));

        var projected = query
            .OrderByDescending(m => m.UploadedAt)
            .ThenBy(m => m.Title)
            .Select(m => new StudyMaterialListItemDto
            {
                MaterialId = m.Id,
                CourseId = m.CourseId,
                Title = m.Title,
                FileName = m.FileName,
                FileType = m.FileType,
                FileSizeMb = m.FileSizeMb,
                IsActive = m.IsActive,
                UploadedAt = m.UploadedAt,
                UploadedByName = m.UploadedByStaff.UserAccount.FullName
            });

        return await PaginatedList<StudyMaterialListItemDto>.CreateAsync(projected, page, pageSize);
    }
}
