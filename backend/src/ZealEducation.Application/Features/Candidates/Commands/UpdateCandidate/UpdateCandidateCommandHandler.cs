using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Candidates.Commands.UpdateCandidate;

public class UpdateCandidateCommandHandler(
    IRepository<Candidate> candidateRepository,
    IRepository<UserAccount> userRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateCandidateCommand, Unit>
{
    public async Task<Unit> Handle(UpdateCandidateCommand request, CancellationToken cancellationToken)
    {
        var candidate = await candidateRepository.GetByIdAsync(request.CandidateId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidate), request.CandidateId);

        var user = await userRepository.GetByIdAsync(candidate.UserAccountId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserAccount), candidate.UserAccountId);

        var email = request.Email.Trim();
        var phone = request.Phone.Trim();
        var fullName = request.FullName.Trim();

        if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            var emailDuplicates = await userRepository.FindAsync(
                u => u.Email == email && u.Id != user.Id,
                cancellationToken);
            if (emailDuplicates.Count > 0)
                throw new ConflictException("Email is already in use.");
        }

        if (!string.Equals(user.Phone, phone, StringComparison.Ordinal))
        {
            var phoneDuplicates = await userRepository.FindAsync(
                u => u.Phone == phone && u.Id != user.Id,
                cancellationToken);
            if (phoneDuplicates.Count > 0)
                throw new ConflictException("Phone is already in use.");
        }

        user.FullName = fullName;
        user.Email = email;
        user.Phone = phone;
        userRepository.Update(user);

        candidate.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();
        candidate.EmergencyContact = string.IsNullOrWhiteSpace(request.EmergencyContact) ? null : request.EmergencyContact.Trim();
        candidate.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        candidate.Status = request.Status;
        candidateRepository.Update(candidate);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
