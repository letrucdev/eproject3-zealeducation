using MediatR;

namespace ZealEducation.Application.Features.Payments.Queries.GetPaymentReceipt;

public record GetPaymentReceiptQuery(Guid TransactionId) : IRequest<ReceiptPdfResult>;

public record ReceiptPdfResult(byte[] Content, string FileName);
