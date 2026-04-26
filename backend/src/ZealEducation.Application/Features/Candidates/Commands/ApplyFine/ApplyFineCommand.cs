using MediatR;

namespace ZealEducation.Application.Features.Candidates.Commands.ApplyFine;

public record ApplyFineCommand(
    Guid CandidateId,
    string ViolationReason,
    decimal PenaltyAmount) : IRequest<ApplyFineResponse>;
