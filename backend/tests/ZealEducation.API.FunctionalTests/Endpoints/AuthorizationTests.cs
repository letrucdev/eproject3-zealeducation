using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ZealEducation.API.FunctionalTests.Fixtures;
using ZealEducation.API.FunctionalTests.Helpers;
using ZealEducation.Domain.Enums;

namespace ZealEducation.API.FunctionalTests.Endpoints;

/// Verifies the JWT bearer + role-based authorization layer.
/// These tests guard the security boundary at controller level — they should
/// catch missing [Authorize] attributes, role typos, and middleware-ordering
/// regressions that unit/integration tests cannot.
[Collection(FunctionalCollection.Name)]
public class AuthorizationTests : IAsyncLifetime
{
    private readonly ApplicationFactory _factory;
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public AuthorizationTests(ApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync() => await _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<string> LoginAndGetTokenAsync(string username, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = username,
            Password = password
        });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        return body.GetProperty("data").GetProperty("token").GetString()!;
    }

    private HttpRequestMessage WithAuth(HttpMethod method, string url, string token)
    {
        var req = new HttpRequestMessage(method, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return req;
    }

    [Fact]
    public async Task Anonymous_request_to_protected_endpoint_returns_401()
    {
        var response = await _client.GetAsync("/api/courses?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Request_with_invalid_bearer_token_returns_401()
    {
        var req = WithAuth(HttpMethod.Get, "/api/courses?page=1&pageSize=10", "not-a-valid-token");

        var response = await _client.SendAsync(req);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Authenticated_user_with_disallowed_role_returns_403()
    {
        // CourseController GET requires Incharge or Counselor; SystemAdmin is not in the list.
        await using (var ctx = _factory.CreateDbContext())
        {
            await TestDataSeeder.SeedUserAsync(ctx, "sysadmin", "Pass123!", UserRole.SystemAdmin);
        }
        var token = await LoginAndGetTokenAsync("sysadmin", "Pass123!");

        var req = WithAuth(HttpMethod.Get, "/api/courses?page=1&pageSize=10", token);
        var response = await _client.SendAsync(req);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Authenticated_user_with_allowed_role_returns_200()
    {
        await using (var ctx = _factory.CreateDbContext())
        {
            await TestDataSeeder.SeedUserAsync(ctx, "counselor1", "Pass123!", UserRole.Counselor);
        }
        var token = await LoginAndGetTokenAsync("counselor1", "Pass123!");

        var req = WithAuth(HttpMethod.Get, "/api/courses?page=1&pageSize=10", token);
        var response = await _client.SendAsync(req);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Counselor_cannot_perform_write_action_requiring_Incharge()
    {
        // Course write (POST /api/courses) requires Incharge role only.
        await using (var ctx = _factory.CreateDbContext())
        {
            await TestDataSeeder.SeedUserAsync(ctx, "counselor2", "Pass123!", UserRole.Counselor);
        }
        var token = await LoginAndGetTokenAsync("counselor2", "Pass123!");

        var req = WithAuth(HttpMethod.Post, "/api/courses", token);
        req.Content = JsonContent.Create(new
        {
            CourseName = "New Course",
            DurationWeeks = 8,
            BaseFee = 1000m
        });
        var response = await _client.SendAsync(req);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Inactive_user_cannot_login_so_cannot_obtain_token()
    {
        await using (var ctx = _factory.CreateDbContext())
        {
            await TestDataSeeder.SeedUserAsync(ctx, "deactivated", "Pass123!", UserRole.Counselor, isActive: false);
        }

        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = "deactivated",
            Password = "Pass123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
