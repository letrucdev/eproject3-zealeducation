using FluentValidation;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Payments.Commands.ConfirmPayment;

public class ConfirmPaymentCommandValidator : AbstractValidator<ConfirmPaymentCommand>
{
    public const long MaxProofSizeBytes = 5 * 1024 * 1024;

    public static readonly string[] AllowedProofContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    public ConfirmPaymentCommandValidator()
    {
        RuleFor(x => x.FeeId).NotEmpty();
        RuleFor(x => x.PaymentMethod).IsInEnum();

        When(x => x.PaymentMethod == PaymentMethod.BankTransfer, () =>
        {
            RuleFor(x => x.BankTransferProofContent)
                .NotNull()
                .WithMessage("Bank transfer proof image is required for bank transfer payments.");

            RuleFor(x => x.BankTransferProofLength)
                .GreaterThan(0)
                .LessThanOrEqualTo(MaxProofSizeBytes)
                .WithMessage($"Proof image must be between 1 byte and {MaxProofSizeBytes / (1024 * 1024)} MB.");

            RuleFor(x => x.BankTransferProofContentType)
                .NotEmpty()
                .Must(ct => ct != null && AllowedProofContentTypes.Contains(ct))
                .WithMessage("Proof image must be JPG, PNG, or WebP.");
        });

        When(x => x.PaymentMethod != PaymentMethod.BankTransfer, () =>
        {
            RuleFor(x => x.BankTransferProofContent)
                .Null()
                .WithMessage("Proof image must not be supplied for non bank transfer payments.");
        });
    }
}
