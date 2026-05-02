using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Common.Services;

public class PaymentReceiptArchiver(
    IRepository<PaymentTransaction> paymentRepository,
    IRepository<FeeStructure> feeRepository,
    IRepository<Candidate> candidateRepository,
    IRepository<UserAccount> userRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Course> courseRepository,
    IRepository<InstallmentPlan> installmentRepository,
    IRepository<Staff> staffRepository,
    IReceiptPdfGenerator pdfGenerator,
    IFileStorageService fileStorage,
    IUnitOfWork unitOfWork) : IPaymentReceiptArchiver
{
    public async Task<byte[]> GenerateAndUploadAsync(Guid transactionId, CancellationToken cancellationToken)
    {
        var transaction = await paymentRepository.Query()
            .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken)
            ?? throw new NotFoundException(nameof(PaymentTransaction), transactionId);

        if (!string.IsNullOrEmpty(transaction.ReceiptFilePath))
        {
            return await fileStorage.DownloadAsync(transaction.ReceiptFilePath, cancellationToken);
        }

        var fee = await feeRepository.GetByIdAsync(transaction.FeeId, cancellationToken)
            ?? throw new NotFoundException(nameof(FeeStructure), transaction.FeeId);

        var candidate = await candidateRepository.GetByIdAsync(fee.CandidateId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidate), fee.CandidateId);

        var candidateUser = await userRepository.GetByIdAsync(candidate.UserAccountId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserAccount), candidate.UserAccountId);

        var enrollment = await enrollmentRepository.Query()
            .FirstOrDefaultAsync(e => e.FeeId == fee.Id, cancellationToken);

        Course? course = null;
        if (enrollment != null)
            course = await courseRepository.GetByIdAsync(enrollment.CourseId, cancellationToken);

        InstallmentPlan? installment = null;
        if (transaction.InstallmentPlanId.HasValue)
            installment = await installmentRepository.GetByIdAsync(transaction.InstallmentPlanId.Value, cancellationToken);

        var staff = await staffRepository.GetByIdAsync(transaction.ProcessedByStaffId, cancellationToken);
        UserAccount? staffUser = null;
        if (staff != null)
            staffUser = await userRepository.GetByIdAsync(staff.UserAccountId, cancellationToken);

        var penaltyAmount = installment?.PenaltyAmount ?? 0m;
        var baseAmount = transaction.Amount - penaltyAmount;

        var model = new ReceiptPdfModel
        {
            ReceiptNumber = transaction.ReceiptNumber,
            PaymentDate = transaction.PaymentDate,
            CandidateCode = candidate.CandidateCode,
            CandidateFullName = candidateUser.FullName,
            CourseTitle = course?.CourseName,
            FeeType = fee.FeeType,
            PaymentMethod = transaction.PaymentMethod,
            BaseAmount = baseAmount,
            PenaltyAmount = penaltyAmount,
            TotalAmount = transaction.Amount,
            ProcessedByStaffName = staffUser?.FullName ?? "-",
            OutstandingBalanceAfter = transaction.OutstandingBalanceAfter,
            InstallmentNo = installment?.InstallmentNo
        };

        var bytes = pdfGenerator.Generate(model);

        var objectKey = $"receipts/{transaction.PaymentDate:yyyy/MM}/{transaction.ReceiptNumber}.pdf";
        await fileStorage.UploadAsync(bytes, objectKey, "application/pdf", cancellationToken);

        transaction.ReceiptFilePath = objectKey;
        paymentRepository.Update(transaction);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return bytes;
    }
}
