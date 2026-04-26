using MediatR;

namespace ZealEducation.Application.Features.Payments.Queries.GetFeeStructureDetail;

public record GetFeeStructureDetailQuery(Guid FeeId) : IRequest<FeeStructureDetailDto>;
