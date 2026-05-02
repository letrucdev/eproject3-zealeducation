using FluentValidation;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Payments.Commands.SetPaymentType;

public class SetPaymentTypeCommandValidator : AbstractValidator<SetPaymentTypeCommand>
{
    public SetPaymentTypeCommandValidator()
    {
        RuleFor(x => x.FeeId).NotEmpty();

        RuleFor(x => x.PaymentType)
            .Must(t => t == PaymentType.FullPayment || t == PaymentType.Installment)
            .WithMessage("Payment type must be FullPayment or Installment.");

        RuleFor(x => x.Frequency)
            .NotNull()
            .When(x => x.PaymentType == PaymentType.Installment)
            .WithMessage("Frequency is required when payment type is Installment.");
    }
}
