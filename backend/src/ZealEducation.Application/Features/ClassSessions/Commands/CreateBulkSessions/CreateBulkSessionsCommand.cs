using MediatR;

namespace ZealEducation.Application.Features.ClassSessions.Commands.CreateBulkSessions;

public record CreateBulkSessionsCommand(
    Guid BatchId,
    IReadOnlyList<DayOfWeek> DaysOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string? Topic,
    string? Location) : IRequest<CreateBulkSessionsResponse>;
