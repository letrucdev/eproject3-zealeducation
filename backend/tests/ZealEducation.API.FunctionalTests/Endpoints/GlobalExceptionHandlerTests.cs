using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ZealEducation.API.FunctionalTests.Fixtures;

namespace ZealEducation.API.FunctionalTests.Endpoints;

[Collection(FunctionalCollection.Name)]
public class GlobalExceptionHandlerTests
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public GlobalExceptionHandlerTests(ApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Validation_failure_returns_400_with_errors_array()
    {
        // Login validator requires both fields — sending empty body triggers the validator
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { Username = "", Password = "" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        body.GetProperty("message").GetString().Should().Be("Validation failed");
        body.GetProperty("data").GetProperty("errors").ValueKind.Should().Be(JsonValueKind.Array);
    }
}
