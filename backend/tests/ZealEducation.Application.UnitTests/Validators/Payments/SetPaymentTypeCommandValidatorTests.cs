using FluentValidation.TestHelper;
using ZealEducation.Application.Features.Payments.Commands.SetPaymentType;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.UnitTests.Validators.Payments;

public class SetPaymentTypeCommandValidatorTests
{
    private readonly SetPaymentTypeCommandValidator _validator = new();

    [Fact]
    public void Should_pass_for_full_payment()
    {
        var cmd = new SetPaymentTypeCommand(Guid.NewGuid(), PaymentType.FullPayment, null);
        _validator.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_pass_for_installment_with_frequency()
    {
        var cmd = new SetPaymentTypeCommand(Guid.NewGuid(), PaymentType.Installment, InstallmentFrequency.Monthly);
        _validator.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_fail_when_fee_id_is_empty()
    {
        var cmd = new SetPaymentTypeCommand(Guid.Empty, PaymentType.FullPayment, null);
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(c => c.FeeId);
    }

    [Fact]
    public void Should_fail_when_payment_type_is_NotSet()
    {
        var cmd = new SetPaymentTypeCommand(Guid.NewGuid(), PaymentType.NotSet, null);
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(c => c.PaymentType);
    }

    [Fact]
    public void Should_fail_when_installment_without_frequency()
    {
        var cmd = new SetPaymentTypeCommand(Guid.NewGuid(), PaymentType.Installment, null);
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(c => c.Frequency);
    }
}
