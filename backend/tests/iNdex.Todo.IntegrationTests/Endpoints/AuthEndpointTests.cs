using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using iNdex.Todo.IntegrationTests.Fixtures;

namespace iNdex.Todo.IntegrationTests.Endpoints;

public sealed class AuthEndpointTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_ValidRequest_Returns201WithTokens()
    {
        var body = new StringContent(
            """{"firstName":"Jane","lastName":"Doe","email":"jane@test.local","password":"Password1"}""",
            Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/register", body);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var json = await response.Content.ReadAsStringAsync();
        var doc  = System.Text.Json.JsonDocument.Parse(json);
        doc.RootElement.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
        doc.RootElement.GetProperty("refreshToken").GetString().Should().NotBeNullOrEmpty();
        doc.RootElement.GetProperty("user").GetProperty("email").GetString().Should().Be("jane@test.local");
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var body = new StringContent(
            """{"firstName":"A","lastName":"B","email":"dup@test.local","password":"Password1"}""",
            Encoding.UTF8, "application/json");

        await _client.PostAsync("/api/auth/register", body);

        var body2    = new StringContent(
            """{"firstName":"A","lastName":"B","email":"dup@test.local","password":"Password1"}""",
            Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/register", body2);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithTokens()
    {
        // Register first
        var reg = new StringContent(
            """{"firstName":"Tom","lastName":"Test","email":"tom@test.local","password":"Password1"}""",
            Encoding.UTF8, "application/json");
        await _client.PostAsync("/api/auth/register", reg);

        // Login
        var login = new StringContent(
            """{"email":"tom@test.local","password":"Password1"}""",
            Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/login", login);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WrongPassword_Returns422()
    {
        var login = new StringContent(
            """{"email":"nobody@test.local","password":"WrongPass1"}""",
            Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/login", login);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Me_WithValidToken_ReturnsUserProfile()
    {
        var (client, _) = await factory.CreateAuthenticatedClientAsync("me@test.local");
        var response    = await client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("email").GetString().Should().Be("me@test.local");
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/lists?ownerId=" + Guid.NewGuid());
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
