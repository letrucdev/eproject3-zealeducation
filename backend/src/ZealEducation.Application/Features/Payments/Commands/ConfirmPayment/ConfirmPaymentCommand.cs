using MediatR;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Payments.Commands.ConfirmPayment;

public record ConfirmPaymentCommand(
    Guid FeeId,
    Guid? InstallmentPlanId,
    PaymentMethod PaymentMethod,
    Stream? BankTransferProofContent = null,
    string? BankTransferProofContentType = null,
    string? BankTransferProofFileName = null,
    long BankTransferProofLength = 0) : IRequest<ConfirmPaymentResponse>;

public class ConfirmPaymentResponse
{
    public Guid TransactionId { get; set; }
    public string ReceiptNumber { get; set; } = default!;
    public decimal BaseAmount { get; set; }
    public decimal PenaltyAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentStatus NewPaymentStatus { get; set; }
}
