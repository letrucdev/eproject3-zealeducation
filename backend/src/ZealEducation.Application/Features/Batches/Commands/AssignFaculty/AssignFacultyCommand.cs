using MediatR;

namespace ZealEducation.Application.Features.Batches.Commands.AssignFaculty;

public record AssignFacultyCommand(
    Guid BatchId,
    Guid? FacultyId) : IRequest<Unit>;
