using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.ClassSessions.Commands.DeleteClassSession;

public class DeleteClassSessionCommandHandler(
    IRepository<ClassSession> sessionRepository,
    IRepository<AttendanceRecord> attendanceRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteClassSessionCommand, Unit>
{
    public async Task<Unit> Handle(DeleteClassSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await sessionRepository.GetByIdAsync(request.SessionId, cancellationToken)
            ?? throw new NotFoundException(nameof(ClassSession), request.SessionId);

        var hasAttendance = await attendanceRepository.Query()
            .AnyAsync(a => a.ClassSessionId == session.Id, cancellationToken);

        if (hasAttendance)
            throw new ConflictException("Cannot delete a session that already has attendance records.");

        sessionRepository.Delete(session);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
