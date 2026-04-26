using MediatR;

namespace ZealEducation.Application.Features.ClassSessions.Queries.GetBatchSessions;

public record GetBatchSessionsQuery(Guid BatchId) : IRequest<List<ClassSessionDto>>;
