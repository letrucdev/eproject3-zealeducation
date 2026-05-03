using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ZealEducation.API.FunctionalTests.Fixtures;
using ZealEducation.API.FunctionalTests.Helpers;
using ZealEducation.Domain.Enums;

namespace ZealEducation.API.FunctionalTests.Endpoints;

[Collection(FunctionalCollection.Name)]
public class AuthEndpointsTests : IAsyncLifetime
{
    private readonly ApplicationFactory _factory;
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public AuthEndpointsTests(ApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync() => await _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Login_returns_200_and_token_for_valid_credentials()
    {
        await using (var ctx = _factory.CreateDbContext())
        {
            await TestDataSeeder.SeedUserAsync(ctx, "alice", "Password123!", UserRole.SystemAdmin);
        }

        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = "alice",
            Password = "Password123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        body.GetProperty("message").GetString().Should().Be("Login successful");
        body.GetProperty("data").GetProperty("token").GetString().Should().NotBeNullOrEmpty();
        body.GetProperty("data").GetProperty("user").GetProperty("username").GetString().Should().Be("alice");
    }

    [Fact]
    public async Task Login_returns_401_for_wrong_password()
    {
        await using (var ctx = _factory.CreateDbContext())
        {
            await TestDataSeeder.SeedUserAsync(ctx, "bob", "Password123!");
        }

        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = "bob",
            Password = "WrongPwd"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        body.GetProperty("message").GetString().Should().Contain("Invalid username or password");
    }

    [Fact]
    public async Task Login_returns_400_when_username_missing()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = "",
            Password = "x"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        body.GetProperty("message").GetString().Should().Be("Validation failed");
        var errors = body.GetProperty("data").GetProperty("errors");
        errors.GetArrayLength().Should().BeGreaterThan(0);
    }
}
