using Microsoft.AspNetCore.SignalR;

namespace iNdex.Todo.Infrastructure.SignalR;

public sealed class TodoHub : Hub
{
    // Groups are per-user: users join their own userId group on connect
    public override async Task OnConnectedAsync()
    {
        var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();
        if (!string.IsNullOrEmpty(userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();
        if (!string.IsNullOrEmpty(userId))
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user:{userId}");

        await base.OnDisconnectedAsync(exception);
    }

    // Allow clients to join a specific list's group for collaborative updates
    public async Task JoinList(string listId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"list:{listId}");

    public async Task LeaveList(string listId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"list:{listId}");
}
