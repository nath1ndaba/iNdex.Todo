using Microsoft.AspNetCore.SignalR;

namespace iNdex.Todo.Infrastructure.SignalR;

public sealed class TodoHub : Hub
{
    // Groups are per-user: users join their own userId group on connect
    public override async Task OnConnectedAsync()
    {
        var ctx    = Context.GetHttpContext();
        var userId = ctx?.Request.Query["userId"].ToString();
        var role   = ctx?.Request.Query["role"].ToString();

        if (!string.IsNullOrEmpty(userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");

        // Managers join a dedicated group so they receive all team broadcasts
        if (!string.IsNullOrEmpty(role) &&
            (role.Equals("Manager", StringComparison.OrdinalIgnoreCase) ||
             role.Equals("Admin",   StringComparison.OrdinalIgnoreCase)))
            await Groups.AddToGroupAsync(Context.ConnectionId, "managers");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var ctx    = Context.GetHttpContext();
        var userId = ctx?.Request.Query["userId"].ToString();

        if (!string.IsNullOrEmpty(userId))
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user:{userId}");

        // Clean up managers group too
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "managers");

        await base.OnDisconnectedAsync(exception);
    }

    // Allow clients to join a specific list's group for collaborative updates
    public async Task JoinList(string listId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"list:{listId}");

    public async Task LeaveList(string listId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"list:{listId}");
}
