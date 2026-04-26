using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Payments.Queries.GetFeeStructureDetail;

public class GetFeeStructureDetailQueryHandler(
    IRepository<FeeStructure> feeRepository,
    IRepository<Candidate> candidateRepository,
    IRepository<UserAccount> userRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Course> courseRepository,
    IRepository<InstallmentPlan> installmentRepository,
    IRepository<PaymentTransaction> paymentRepository,
    IRepository<Staff> staffRepository) : IRequestHandler<GetFeeStructureDetailQuery, FeeStructureDetailDto>
{
    public async Task<FeeStructureDetailDto> Handle(GetFeeStructureDetailQuery request, CancellationToken cancellationToken)
    {
        var fee = await feeRepository.Query().FirstOrDefaultAsync(f => f.Id == request.FeeId, cancellationToken)
            ?? throw new NotFoundException(nameof(FeeStructure), request.FeeId);

        var candidate = await candidateRepository.GetByIdAsync(fee.CandidateId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidate), fee.CandidateId);

        var user = await userRepository.GetByIdAsync(candidate.UserAccountId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserAccount), candidate.UserAccountId);

        var enrollment = await enrollmentRepository.Query()
            .FirstOrDefaultAsync(e => e.FeeId == fee.Id, cancellationToken);

        Course? course = null;
        if (enrollment != null)
        {
            course = await courseRepository.GetByIdAsync(enrollment.CourseId, cancellationToken);
        }

        var installments = await installmentRepository.Query()
            .Where(i => i.FeeId == fee.Id)
            .OrderBy(i => i.InstallmentNo)
            .Select(i => new InstallmentPlanDto
            {
                Id = i.Id,
                InstallmentNo = i.InstallmentNo,
                AmountDue = i.AmountDue,
                DueDate = i.DueDate,
                AmountPaid = i.AmountPaid,
                PaidDate = i.PaidDate,
                Status = i.Status,
                PenaltyAmount = i.PenaltyAmount
            })
            .ToListAsync(cancellationToken);

        var transactions = await (
            from t in paymentRepository.Query()
            where t.FeeId == fee.Id
            join staff in staffRepository.Query() on t.ProcessedByStaffId equals staff.Id into staffsByT
            from staff in staffsByT.DefaultIfEmpty()
            join u in userRepository.Query() on staff.UserAccountId equals u.Id into usersByStaff
            from u in usersByStaff.DefaultIfEmpty()
            join inst in installmentRepository.Query() on t.InstallmentPlanId equals inst.Id into installs
            from inst in installs.DefaultIfEmpty()
            orderby t.PaymentDate descending
            select new PaymentTransactionDto
            {
                Id = t.Id,
                InstallmentPlanId = t.InstallmentPlanId,
                InstallmentNo = inst != null ? inst.InstallmentNo : (int?)null,
                ReceiptNumber = t.ReceiptNumber,
                PaymentDate = t.PaymentDate,
                Amount = t.Amount,
                PaymentMethod = t.PaymentMethod,
                ProcessedByStaffName = u != null ? u.FullName : null,
                HasBankTransferProof = t.BankTransferProofPath != null
            }).ToListAsync(cancellationToken);

        return new FeeStructureDetailDto
        {
            FeeId = fee.Id,
            CandidateId = candidate.Id,
            CandidateCode = candidate.CandidateCode,
            CandidateFullName = user.FullName,
            CandidateEmail = user.Email,
            CandidatePhone = user.Phone,
            CandidateIsActive = user.IsActive,
            EnrollmentId = enrollment?.Id,
            EnrollmentDate = enrollment?.EnrollmentDate,
            CourseId = course?.Id,
            CourseName = course?.CourseName,
            DurationWeeks = course?.DurationWeeks,
            FeeType = fee.FeeType,
            TotalFee = fee.TotalFee,
            AmountPaid = fee.AmountPaid,
            OutstandingBalance = fee.OutstandingBalance,
            PaymentStatus = fee.PaymentStatus,
            PaymentType = fee.PaymentType,
            Notes = fee.Notes,
            InstallmentPlans = installments,
            PaymentTransactions = transactions
        };
    }
}
