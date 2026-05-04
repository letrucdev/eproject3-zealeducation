using MediatR;

namespace ZealEducation.Application.Features.Candidates.Commands.AddEnrollment;

public record AddEnrollmentCommand(Guid CandidateId, Guid CourseId) : IRequest<AddEnrollmentResponse>;
