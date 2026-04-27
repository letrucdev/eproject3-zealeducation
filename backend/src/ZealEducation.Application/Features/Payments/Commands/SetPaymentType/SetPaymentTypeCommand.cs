using MediatR;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Payments.Commands.SetPaymentType;

public record SetPaymentTypeCommand(
    Guid FeeId,
    PaymentType PaymentType,
    InstallmentFrequency? Frequency) : IRequest<SetPaymentTypeResponse>;

public class SetPaymentTypeResponse
{
    public PaymentType PaymentType { get; set; }
    public InstallmentFrequency? Frequency { get; set; }
    public List<SetPaymentTypeInstallmentDto> Installments { get; set; } = [];
}

public class SetPaymentTypeInstallmentDto
{
    public Guid Id { get; set; }
    public int InstallmentNo { get; set; }
    public decimal AmountDue { get; set; }
    public DateOnly DueDate { get; set; }
}
