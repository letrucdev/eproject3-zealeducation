using FluentValidation;
using FluentValidation.Results;
using MediatR;
using ZealEducation.Application.Common.Behaviors;

namespace ZealEducation.Application.UnitTests.Common.Behaviors;

public class ValidationBehaviorTests
{
    public record SampleRequest(string Name) : IRequest<string>;

    private static RequestHandlerDelegate<string> NextReturning(string value)
        => () => Task.FromResult(value);

    [Fact]
    public async Task Should_pass_through_when_no_validators_registered()
    {
        var behavior = new ValidationBehavior<SampleRequest, string>([]);

        var result = await behavior.Handle(new SampleRequest("ok"), NextReturning("handled"), default);

        result.Should().Be("handled");
    }

    [Fact]
    public async Task Should_pass_through_when_all_validators_succeed()
    {
        var validator = new Mock<IValidator<SampleRequest>>();
        validator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<SampleRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var behavior = new ValidationBehavior<SampleRequest, string>([validator.Object]);

        var result = await behavior.Handle(new SampleRequest("ok"), NextReturning("handled"), default);

        result.Should().Be("handled");
        validator.Verify(v => v.ValidateAsync(It.IsAny<ValidationContext<SampleRequest>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_throw_ValidationException_when_any_validator_fails()
    {
        var validator = new Mock<IValidator<SampleRequest>>();
        validator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<SampleRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Name", "Name is required")]));

        var behavior = new ValidationBehavior<SampleRequest, string>([validator.Object]);

        var act = async () => await behavior.Handle(new SampleRequest(""), NextReturning("handled"), default);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().ContainSingle(e => e.PropertyName == "Name" && e.ErrorMessage == "Name is required");
    }

    [Fact]
    public async Task Should_aggregate_failures_from_multiple_validators()
    {
        var validatorA = new Mock<IValidator<SampleRequest>>();
        validatorA
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<SampleRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Name", "A")]));

        var validatorB = new Mock<IValidator<SampleRequest>>();
        validatorB
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<SampleRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Name", "B")]));

        var behavior = new ValidationBehavior<SampleRequest, string>([validatorA.Object, validatorB.Object]);

        var act = async () => await behavior.Handle(new SampleRequest(""), NextReturning("handled"), default);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task Should_not_invoke_next_when_validation_fails()
    {
        var validator = new Mock<IValidator<SampleRequest>>();
        validator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<SampleRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Name", "fail")]));

        var nextCalled = false;
        var behavior = new ValidationBehavior<SampleRequest, string>([validator.Object]);

        var act = async () => await behavior.Handle(
            new SampleRequest(""),
            () => { nextCalled = true; return Task.FromResult("handled"); },
            default);

        await act.Should().ThrowAsync<ValidationException>();
        nextCalled.Should().BeFalse();
    }
}
