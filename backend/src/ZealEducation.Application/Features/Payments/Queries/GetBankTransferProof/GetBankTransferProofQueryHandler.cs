using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Payments.Queries.GetBankTransferProof;

public class GetBankTransferProofQueryHandler(
    IRepository<PaymentTransaction> paymentRepository,
    IFileStorageService fileStorage) : IRequestHandler<GetBankTransferProofQuery, BankTransferProofResult>
{
    public async Task<BankTransferProofResult> Handle(GetBankTransferProofQuery request, CancellationToken cancellationToken)
    {
        var transaction = await paymentRepository.GetByIdAsync(request.TransactionId, cancellationToken)
            ?? throw new NotFoundException(nameof(PaymentTransaction), request.TransactionId);

        if (string.IsNullOrEmpty(transaction.BankTransferProofPath))
            throw new NotFoundException("Bank transfer proof", request.TransactionId);

        var bytes = await fileStorage.DownloadAsync(transaction.BankTransferProofPath, cancellationToken);

        var extension = Path.GetExtension(transaction.BankTransferProofPath);
        var contentType = extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        var fileName = $"{transaction.ReceiptNumber}-proof{extension}";
        return new BankTransferProofResult(bytes, contentType, fileName);
    }
}
