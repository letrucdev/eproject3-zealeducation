using MediatR;
using ZealEducation.Application.Features.Batches.Queries.GetBatchById;

namespace ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultyBatchById;

public record GetFacultyBatchByIdQuery(Guid BatchId) : IRequest<BatchDetailDto>;
