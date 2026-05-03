using System.Security.Cryptography;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Payments.Commands.ConfirmPayment;

public class ConfirmPaymentCommandHandler(
    IRepository<FeeStructure> feeRepository,
    IRepository<InstallmentPlan> installmentRepository,
    IRepository<PaymentTransaction> paymentRepository,
    IRepository<Candidate> candidateRepository,
    IRepository<UserAccount> userRepository,
    IRepository<Staff> staffRepository,
    ICurrentUser currentUser,
    IFileStorageService fileStorage,
    IUnitOfWork unitOfWork) : IRequestHandler<ConfirmPaymentCommand, ConfirmPaymentResponse>
{
    private const decimal PenaltyRate = 0.05m;

    public async Task<ConfirmPaymentResponse> Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
            throw new UnauthorizedException("Current user could not be resolved.");

        var fee = await feeRepository.GetByIdAsync(request.FeeId, cancellationToken)
            ?? throw new NotFoundException(nameof(FeeStructure), request.FeeId);

        if (fee.PaymentType == PaymentType.NotSet)
            throw new ConflictException("Payment type has not been set for this fee.");

        var staff = (await staffRepository.FindAsync(s => s.UserAccountId == currentUser.UserId.Value, cancellationToken))[0]
            ?? throw new ConflictException("Current user is not registered as staff.");

        InstallmentPlan? installment = null;
        decimal baseAmount;
        decimal penaltyAmount = 0m;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (fee.PaymentType == PaymentType.FullPayment)
        {
            if (fee.OutstandingBalance <= 0)
                throw new ConflictException("This fee has already been fully paid.");
            baseAmount = fee.OutstandingBalance;
        }
        else
        {
            if (request.InstallmentPlanId is null)
                throw new ConflictException("InstallmentPlanId is required for installment payment.");

            installment = await installmentRepository.GetByIdAsync(request.InstallmentPlanId.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(InstallmentPlan), request.InstallmentPlanId.Value);

            if (installment.FeeId != fee.Id)
                throw new ConflictException("Installment does not belong to this fee.");

            if (installment.Status == InstallmentStatus.Paid)
                throw new ConflictException("This installment has already been paid.");

            var unpaidPrevious = await installmentRepository.FindAsync(
                i => i.FeeId == fee.Id
                    && i.InstallmentNo < installment.InstallmentNo
                    && i.Status != InstallmentStatus.Paid,
                cancellationToken);
            if (unpaidPrevious.Count > 0)
                throw new ConflictException("Previous installments must be paid before this one.");

            baseAmount = installment.AmountDue;
            if (today > installment.DueDate)
            {
                penaltyAmount = Math.Round(baseAmount * PenaltyRate, 2, MidpointRounding.AwayFromZero);
            }
        }

        var totalAmount = baseAmount + penaltyAmount;
        var receiptNumber = await GenerateUniqueReceiptNumberAsync(cancellationToken);

        var paymentDate = DateTime.UtcNow;
        var transaction = new PaymentTransaction
        {
            Id = Guid.NewGuid(),
            FeeId = fee.Id,
            ProcessedByStaffId = staff.Id,
            InstallmentPlanId = installment?.Id,
            Amount = totalAmount,
            PaymentMethod = request.PaymentMethod,
            ReceiptNumber = receiptNumber,
            PaymentDate = paymentDate
        };

        if (request.PaymentMethod == PaymentMethod.BankTransfer && request.BankTransferProofContent != null)
        {
            transaction.BankTransferProofPath = await UploadBankTransferProofAsync(
                request.BankTransferProofContent,
                request.BankTransferProofContentType!,
                receiptNumber,
                paymentDate,
                cancellationToken);
        }

        await paymentRepository.AddAsync(transaction, cancellationToken);

        fee.AmountPaid += totalAmount;
        fee.PenaltyApplied += penaltyAmount;
        if (installment != null)
        {
            installment.AmountPaid += totalAmount;
            installment.PenaltyAmount = penaltyAmount;
            installment.PaidDate = today;
            installment.Status = InstallmentStatus.Paid;
            installmentRepository.Update(installment);
        }

        var owedToDate = fee.TotalFee + fee.PenaltyApplied;
        transaction.OutstandingBalanceAfter = owedToDate - fee.AmountPaid;

        var newStatus = fee.AmountPaid >= owedToDate
            ? PaymentStatus.Paid
            : (fee.AmountPaid > 0 ? PaymentStatus.Partial : PaymentStatus.Unpaid);
        fee.PaymentStatus = newStatus;
        feeRepository.Update(fee);

        if (fee.FeeType == FeeType.Tuition)
        {
            var candidate = await candidateRepository.GetByIdAsync(fee.CandidateId, cancellationToken);
            if (candidate != null)
            {
                var userAccount = await userRepository.GetByIdAsync(candidate.UserAccountId, cancellationToken);
                if (userAccount != null && !userAccount.IsActive)
                {
                    userAccount.IsActive = true;
                    userRepository.Update(userAccount);
                }
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ConfirmPaymentResponse
        {
            TransactionId = transaction.Id,
            ReceiptNumber = transaction.ReceiptNumber,
            BaseAmount = baseAmount,
            PenaltyAmount = penaltyAmount,
            TotalAmount = totalAmount,
            NewPaymentStatus = newStatus
        };
    }

    private async Task<string> UploadBankTransferProofAsync(
        Stream content,
        string contentType,
        string receiptNumber,
        DateTime paymentDate,
        CancellationToken cancellationToken)
    {
        using var memory = new MemoryStream();
        await content.CopyToAsync(memory, cancellationToken);
        var bytes = memory.ToArray();

        var extension = contentType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => throw new ConflictException($"Unsupported proof image content type '{contentType}'.")
        };

        var objectKey = $"bank-transfer-proofs/{paymentDate:yyyy/MM}/{receiptNumber}{extension}";
        return await fileStorage.UploadAsync(bytes, objectKey, contentType, cancellationToken);
    }

    private async Task<string> GenerateUniqueReceiptNumberAsync(CancellationToken cancellationToken)
    {
        var prefix = $"RCP-{DateTime.UtcNow:yyyyMMdd}-";
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var seq = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var receipt = prefix + seq;
            var existing = await paymentRepository.FindAsync(t => t.ReceiptNumber == receipt, cancellationToken);
            if (existing.Count == 0) return receipt;
        }
        throw new ConflictException("Could not generate a unique receipt number; please try again.");
    }
}
