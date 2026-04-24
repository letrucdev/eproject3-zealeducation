using MediatR;

namespace ZealEducation.Application.Features.Batches.Commands.CreateBatch;

public record CreateBatchCommand(
    string BatchCode,
    Guid CourseId,
    Guid? FacultyId,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Location,
    int MaxCapacity) : IRequest<CreateBatchResponse>;
