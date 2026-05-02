using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Payments.Queries.GetPaymentReceipt;

public class GetPaymentReceiptQueryHandler(
    IRepository<PaymentTransaction> paymentRepository,
    IPaymentReceiptArchiver receiptArchiver) : IRequestHandler<GetPaymentReceiptQuery, ReceiptPdfResult>
{
    public async Task<ReceiptPdfResult> Handle(GetPaymentReceiptQuery request, CancellationToken cancellationToken)
    {
        var transaction = await paymentRepository.Query()
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken)
            ?? throw new NotFoundException(nameof(PaymentTransaction), request.TransactionId);

        var bytes = await receiptArchiver.GenerateAndUploadAsync(transaction.Id, cancellationToken);
        return new ReceiptPdfResult(bytes, $"{transaction.ReceiptNumber}.pdf");
    }
}
