using iNdex.Todo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace iNdex.Todo.IntegrationTests.Fixtures;

/// <summary>
/// Spins up a real PostgreSQL container via Testcontainers,
/// applies EF Core migrations, and provides an HttpClient for endpoint tests.
/// </summary>
public sealed class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("index_todo_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            // Re-register with the test container connection string
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // Run migrations against the container
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.StopAsync();
    }

    /// <summary>Helper to get an authenticated HttpClient after registering+logging in a test user.</summary>
    public async Task<(HttpClient client, string accessToken)> CreateAuthenticatedClientAsync(
        string email = "test@index.local",
        string password = "TestPass1")
    {
        var client = CreateClient();

        // Register
        var registerBody = new StringContent(
            $$"""{"firstName":"Test","lastName":"User","email":"{{email}}","password":"{{password}}"}""",
            System.Text.Encoding.UTF8, "application/json");

        var regResp = await client.PostAsync("/api/auth/register", registerBody);
        if (!regResp.IsSuccessStatusCode)
        {
            // Already registered — login instead
            var loginBody = new StringContent(
                $$"""{"email":"{{email}}","password":"{{password}}"}""",
                System.Text.Encoding.UTF8, "application/json");
            regResp = await client.PostAsync("/api/auth/login", loginBody);
        }

        var json        = await regResp.Content.ReadAsStringAsync();
        var doc         = System.Text.Json.JsonDocument.Parse(json);
        var accessToken = doc.RootElement.GetProperty("accessToken").GetString()!;

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        return (client, accessToken);
    }
}
