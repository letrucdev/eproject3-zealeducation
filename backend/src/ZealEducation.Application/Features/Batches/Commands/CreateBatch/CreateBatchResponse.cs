using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Batches.Commands.CreateBatch;

public class CreateBatchResponse
{
    public Guid BatchId { get; init; }
    public string BatchCode { get; init; } = default!;
    public BatchStatus Status { get; init; }
}
