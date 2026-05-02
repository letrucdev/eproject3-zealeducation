using MediatR;

namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyBatchExamResults;

public record GetMyBatchExamResultsQuery(Guid BatchId) : IRequest<MyBatchExamResultsDto>;
