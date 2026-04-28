using MediatR;
using ZealEducation.Application.Common.Helpers;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.Batches.Queries.GetBatchEnrollments;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;
using FacultyEntity = ZealEducation.Domain.Entities.Faculty;

namespace ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultyBatchEnrollments;

public class GetFacultyBatchEnrollmentsQueryHandler(
    ISender sender,
    IRepository<Batch> batchRepository,
    IRepository<FacultyEntity> facultyRepository,
    ICurrentUser currentUser) : IRequestHandler<GetFacultyBatchEnrollmentsQuery, PaginatedList<BatchEnrollmentItemDto>>
{
    public async Task<PaginatedList<BatchEnrollmentItemDto>> Handle(GetFacultyBatchEnrollmentsQuery request, CancellationToken cancellationToken)
    {
        var facultyId = await facultyRepository.ResolveFacultyIdAsync(currentUser, cancellationToken);
        await batchRepository.EnsureBatchOwnedByFacultyAsync(request.BatchId, facultyId, cancellationToken);

        return await sender.Send(
            new GetBatchEnrollmentsQuery(request.BatchId, request.Page, request.PageSize, request.Search, request.SortBy, request.SortDirection),
            cancellationToken);
    }
}
