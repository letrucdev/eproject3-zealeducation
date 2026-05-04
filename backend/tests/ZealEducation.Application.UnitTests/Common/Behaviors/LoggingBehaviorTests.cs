using MediatR;
using Microsoft.Extensions.Logging;
using ZealEducation.Application.Common.Behaviors;

namespace ZealEducation.Application.UnitTests.Common.Behaviors;

public class LoggingBehaviorTests
{
    public record SampleRequest(string Value) : IRequest<string>;

    [Fact]
    public async Task Should_pass_response_through_unchanged()
    {
        var logger = Mock.Of<ILogger<LoggingBehavior<SampleRequest, string>>>();
        var behavior = new LoggingBehavior<SampleRequest, string>(logger);

        var result = await behavior.Handle(
            new SampleRequest("input"),
            () => Task.FromResult("expected-response"),
            default);

        result.Should().Be("expected-response");
    }

    [Fact]
    public async Task Should_log_request_name_before_and_after_handling()
    {
        var logger = new Mock<ILogger<LoggingBehavior<SampleRequest, string>>>();
        var behavior = new LoggingBehavior<SampleRequest, string>(logger.Object);

        await behavior.Handle(new SampleRequest("v"), () => Task.FromResult("ok"), default);

        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, _) => o.ToString()!.Contains("Handling") && o.ToString()!.Contains(nameof(SampleRequest))),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, _) => o.ToString()!.Contains("Handled") && o.ToString()!.Contains(nameof(SampleRequest))),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_propagate_exception_from_next()
    {
        var logger = Mock.Of<ILogger<LoggingBehavior<SampleRequest, string>>>();
        var behavior = new LoggingBehavior<SampleRequest, string>(logger);

        var act = async () => await behavior.Handle(
            new SampleRequest("v"),
            () => Task.FromException<string>(new InvalidOperationException("boom")),
            default);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
    }
}
