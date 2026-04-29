using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.CandidatePortal.Common;

internal static class CandidateResolver
{
    /// <summary>
    /// Resolve candidate row tu JWT cua user dang dang nhap. Throw UnauthorizedException
    /// neu khong co user trong context, hoac NotFoundException neu user khong gan candidate nao.
    /// </summary>
    public static async Task<Candidate> ResolveAsync(
        ICurrentUser currentUser,
        IRepository<Candidate> candidateRepository,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedException("Authentication required.");

        var candidates = await candidateRepository.FindAsync(c => c.UserAccountId == userId, cancellationToken);
        if (candidates.Count == 0)
            throw new NotFoundException("No candidate profile is associated with this account.");

        return candidates[0];
    }
}
