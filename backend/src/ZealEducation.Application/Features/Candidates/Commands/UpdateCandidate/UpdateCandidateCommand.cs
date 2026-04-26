using MediatR;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Candidates.Commands.UpdateCandidate;

public record UpdateCandidateCommand(
    Guid CandidateId,
    string FullName,
    string Email,
    string Phone,
    string? Address,
    string? EmergencyContact,
    string? Notes,
    CandidateStatus Status) : IRequest<Unit>;
