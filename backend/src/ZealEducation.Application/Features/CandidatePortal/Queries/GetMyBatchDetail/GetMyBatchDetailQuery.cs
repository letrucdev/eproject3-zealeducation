using MediatR;

namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyBatchDetail;

public record GetMyBatchDetailQuery(Guid BatchId) : IRequest<MyBatchDetailDto>;
