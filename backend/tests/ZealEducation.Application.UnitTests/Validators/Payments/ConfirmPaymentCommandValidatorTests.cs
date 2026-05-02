using FluentValidation.TestHelper;
using ZealEducation.Application.Features.Payments.Commands.ConfirmPayment;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.UnitTests.Validators.Payments;

public class ConfirmPaymentCommandValidatorTests
{
    private readonly ConfirmPaymentCommandValidator _validator = new();

    [Fact]
    public void Should_pass_for_cash_payment()
    {
        var cmd = new ConfirmPaymentCommand(Guid.NewGuid(), null, PaymentMethod.Cash);
        _validator.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_pass_for_bank_transfer_with_proof()
    {
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var cmd = new ConfirmPaymentCommand(
            Guid.NewGuid(),
            null,
            PaymentMethod.BankTransfer,
            stream,
            "image/jpeg",
            "proof.jpg",
            stream.Length);
        _validator.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_fail_when_fee_id_is_empty()
    {
        var cmd = new ConfirmPaymentCommand(Guid.Empty, null, PaymentMethod.Cash);
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(c => c.FeeId);
    }

    [Fact]
    public void Should_fail_when_payment_method_is_not_in_enum()
    {
        var cmd = new ConfirmPaymentCommand(Guid.NewGuid(), null, (PaymentMethod)999);
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(c => c.PaymentMethod);
    }

    [Fact]
    public void Should_fail_when_bank_transfer_without_proof()
    {
        var cmd = new ConfirmPaymentCommand(Guid.NewGuid(), null, PaymentMethod.BankTransfer);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(c => c.BankTransferProofContent);
    }

    [Fact]
    public void Should_fail_when_bank_transfer_proof_too_large()
    {
        using var stream = new MemoryStream(new byte[] { 1 });
        var cmd = new ConfirmPaymentCommand(
            Guid.NewGuid(),
            null,
            PaymentMethod.BankTransfer,
            stream,
            "image/jpeg",
            "proof.jpg",
            ConfirmPaymentCommandValidator.MaxProofSizeBytes + 1);
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(c => c.BankTransferProofLength);
    }

    [Fact]
    public void Should_fail_when_bank_transfer_proof_content_type_unsupported()
    {
        using var stream = new MemoryStream(new byte[] { 1 });
        var cmd = new ConfirmPaymentCommand(
            Guid.NewGuid(),
            null,
            PaymentMethod.BankTransfer,
            stream,
            "application/pdf",
            "proof.pdf",
            stream.Length);
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(c => c.BankTransferProofContentType);
    }

    [Fact]
    public void Should_fail_when_non_bank_transfer_supplies_proof()
    {
        using var stream = new MemoryStream(new byte[] { 1 });
        var cmd = new ConfirmPaymentCommand(
            Guid.NewGuid(),
            null,
            PaymentMethod.Cash,
            stream,
            "image/jpeg",
            "proof.jpg",
            stream.Length);
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(c => c.BankTransferProofContent);
    }
}
