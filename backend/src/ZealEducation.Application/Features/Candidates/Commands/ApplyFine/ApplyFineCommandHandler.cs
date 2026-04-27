using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Candidates.Commands.ApplyFine;

public class ApplyFineCommandHandler(
    IRepository<Candidate> candidateRepository,
    IRepository<Staff> staffRepository,
    IRepository<FeeStructure> feeRepository,
    IRepository<Fine> fineRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<ApplyFineCommand, ApplyFineResponse>
{
    public async Task<ApplyFineResponse> Handle(ApplyFineCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
            throw new UnauthorizedException("Current user could not be resolved.");

        var candidate = await candidateRepository.GetByIdAsync(request.CandidateId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidate), request.CandidateId);

        var staffMatches = await staffRepository.FindAsync(
            s => s.UserAccountId == currentUser.UserId.Value,
            cancellationToken);
        if (staffMatches.Count == 0)
            throw new ConflictException("Current user is not registered as staff.");
        var staff = staffMatches[0];

        var penaltyAmount = Math.Round(request.PenaltyAmount, 2, MidpointRounding.AwayFromZero);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var fee = new FeeStructure
        {
            Id = Guid.NewGuid(),
            CandidateId = candidate.Id,
            TotalFee = penaltyAmount,
            AmountPaid = 0m,
            OutstandingBalance = penaltyAmount,
            FeeType = FeeType.Fine,
            PaymentStatus = PaymentStatus.Unpaid,
            PaymentType = PaymentType.NotSet,
            Notes = request.ViolationReason.Trim()
        };

        var fine = new Fine
        {
            Id = Guid.NewGuid(),
            FeeId = fee.Id,
            CandidateId = candidate.Id,
            IssuedByStaffId = staff.Id,
            ViolationReason = request.ViolationReason.Trim(),
            PenaltyAmount = penaltyAmount,
            IssuedDate = today,
            IsPaid = false
        };

        await feeRepository.AddAsync(fee, cancellationToken);
        await fineRepository.AddAsync(fine, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApplyFineResponse
        {
            FineId = fine.Id,
            FeeId = fee.Id
        };
    }
}
