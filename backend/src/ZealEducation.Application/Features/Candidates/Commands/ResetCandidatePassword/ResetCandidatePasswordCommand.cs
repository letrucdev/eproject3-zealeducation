using MediatR;

namespace ZealEducation.Application.Features.Candidates.Commands.ResetCandidatePassword;

public record ResetCandidatePasswordCommand(Guid CandidateId) : IRequest<ResetCandidatePasswordResponse>;
