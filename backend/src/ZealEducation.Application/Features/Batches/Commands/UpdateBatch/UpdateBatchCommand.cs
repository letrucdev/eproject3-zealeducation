using MediatR;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Batches.Commands.UpdateBatch;

public record UpdateBatchCommand(
    Guid BatchId,
    string BatchCode,
    Guid CourseId,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Location,
    int MaxCapacity,
    BatchStatus Status) : IRequest<Unit>;
