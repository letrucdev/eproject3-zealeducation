using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Common.Helpers;

public static class CurrentFacultyExtensions
{
    public static async Task<Guid> ResolveFacultyIdAsync(
        this IRepository<Faculty> facultyRepository,
        ICurrentUser currentUser,
        CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
            throw new UnauthorizedException("Current user could not be resolved.");

        var facultyId = await facultyRepository.Query()
            .Where(f => f.Staff.UserAccountId == currentUser.UserId.Value)
            .Select(f => (Guid?)f.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return facultyId
            ?? throw new UnauthorizedException("Current user is not registered as faculty.");
    }

    public static async Task EnsureBatchOwnedByFacultyAsync(
        this IRepository<Batch> batchRepository,
        Guid batchId,
        Guid facultyId,
        CancellationToken cancellationToken = default)
    {
        var owned = await batchRepository.Query()
            .AnyAsync(b => b.Id == batchId && b.FacultyId == facultyId, cancellationToken);

        if (!owned)
            throw new NotFoundException(nameof(Batch), batchId);
    }
}
