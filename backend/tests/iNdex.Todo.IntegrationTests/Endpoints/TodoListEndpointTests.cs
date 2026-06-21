using System.Net;
using System.Text;
using FluentAssertions;
using iNdex.Todo.IntegrationTests.Fixtures;

namespace iNdex.Todo.IntegrationTests.Endpoints;

public sealed class TodoListEndpointTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    [Fact]
    public async Task CreateList_ValidRequest_Returns201()
    {
        var (client, _) = await factory.CreateAuthenticatedClientAsync("listuser@test.local");

        // Get the user's ID via /api/auth/me
        var me    = await client.GetAsync("/api/auth/me");
        var doc   = System.Text.Json.JsonDocument.Parse(await me.Content.ReadAsStringAsync());
        var userId = doc.RootElement.GetProperty("id").GetString();

        var body = new StringContent(
            $$"""{"name":"Shopping","description":null,"color":"#39D353","icon":null,"ownerId":"{{userId}}"}""",
            Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/lists", body);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var respDoc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        respDoc.RootElement.GetProperty("id").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetAllLists_AuthenticatedUser_ReturnsOwnLists()
    {
        var (client, _) = await factory.CreateAuthenticatedClientAsync("getlists@test.local");

        var me     = await client.GetAsync("/api/auth/me");
        var doc    = System.Text.Json.JsonDocument.Parse(await me.Content.ReadAsStringAsync());
        var userId = doc.RootElement.GetProperty("id").GetString();

        var response = await client.GetAsync($"/api/lists?ownerId={userId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteList_ExistingList_Returns204()
    {
        var (client, _) = await factory.CreateAuthenticatedClientAsync("deletelistuser@test.local");

        var me     = await client.GetAsync("/api/auth/me");
        var doc    = System.Text.Json.JsonDocument.Parse(await me.Content.ReadAsStringAsync());
        var userId = doc.RootElement.GetProperty("id").GetString();

        // Create a list
        var createBody = new StringContent(
            $$"""{"name":"ToDelete","description":null,"color":null,"icon":null,"ownerId":"{{userId}}"}""",
            Encoding.UTF8, "application/json");
        var created    = await client.PostAsync("/api/lists", createBody);
        var createdDoc = System.Text.Json.JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var listId     = createdDoc.RootElement.GetProperty("id").GetString();

        // Delete it
        var deleteResp = await client.DeleteAsync($"/api/lists/{listId}");
        deleteResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Confirm it's gone
        var getResp = await client.GetAsync($"/api/lists/{listId}");
        getResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
