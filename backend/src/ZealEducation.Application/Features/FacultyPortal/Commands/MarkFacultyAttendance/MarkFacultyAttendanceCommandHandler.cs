using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Helpers;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.ClassSessions.Commands.MarkAttendance;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;
using FacultyEntity = ZealEducation.Domain.Entities.Faculty;

namespace ZealEducation.Application.Features.FacultyPortal.Commands.MarkFacultyAttendance;

public class MarkFacultyAttendanceCommandHandler(
    ISender sender,
    IRepository<ClassSession> sessionRepository,
    IRepository<FacultyEntity> facultyRepository,
    ICurrentUser currentUser) : IRequestHandler<MarkFacultyAttendanceCommand, Unit>
{
    public async Task<Unit> Handle(MarkFacultyAttendanceCommand request, CancellationToken cancellationToken)
    {
        var facultyId = await facultyRepository.ResolveFacultyIdAsync(currentUser, cancellationToken);

        var sessionOwned = await sessionRepository.Query()
            .AnyAsync(s => s.Id == request.SessionId && s.Batch.FacultyId == facultyId, cancellationToken);

        if (!sessionOwned)
            throw new NotFoundException(nameof(ClassSession), request.SessionId);

        return await sender.Send(new MarkAttendanceCommand(request.SessionId, request.Entries), cancellationToken);
    }
}
