using MediatR;
using ZealEducation.Application.Common.Helpers;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.ClassSessions.Queries.GetBatchSessions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;
using FacultyEntity = ZealEducation.Domain.Entities.Faculty;

namespace ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultyBatchSessions;

public class GetFacultyBatchSessionsQueryHandler(
    ISender sender,
    IRepository<Batch> batchRepository,
    IRepository<FacultyEntity> facultyRepository,
    ICurrentUser currentUser) : IRequestHandler<GetFacultyBatchSessionsQuery, PaginatedList<ClassSessionDto>>
{
    public async Task<PaginatedList<ClassSessionDto>> Handle(GetFacultyBatchSessionsQuery request, CancellationToken cancellationToken)
    {
        var facultyId = await facultyRepository.ResolveFacultyIdAsync(currentUser, cancellationToken);
        await batchRepository.EnsureBatchOwnedByFacultyAsync(request.BatchId, facultyId, cancellationToken);

        return await sender.Send(
            new GetBatchSessionsQuery(request.BatchId, request.Page, request.PageSize, request.SortBy, request.SortDirection),
            cancellationToken);
    }
}
