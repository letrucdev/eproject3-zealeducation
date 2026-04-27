namespace ZealEducation.Application.Features.ClassSessions.Commands.CreateBulkSessions;

public record CreateBulkSessionsResponse(
    int CreatedCount,
    IReadOnlyList<DateOnly> SkippedDates);
