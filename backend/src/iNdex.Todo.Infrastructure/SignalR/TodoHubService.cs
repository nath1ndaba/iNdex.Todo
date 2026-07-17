using Microsoft.AspNetCore.SignalR;

namespace iNdex.Todo.Infrastructure.SignalR;

public interface ITodoHubService
{
    Task NotifyTaskCreated(Guid userId, object payload, CancellationToken ct = default);
    Task NotifyTaskUpdated(Guid userId, object payload, CancellationToken ct = default);
    Task NotifyTaskDeleted(Guid userId, Guid taskId, CancellationToken ct = default);
    Task NotifyListUpdated(Guid listId, object payload, CancellationToken ct = default);

    // ── Team activity broadcasts (sent to the "managers" group) ─────────────
    Task BroadcastTicketOpened(Guid byUserId, string userName, string ticketNumber, string title, CancellationToken ct = default);
    Task BroadcastTicketCompleted(Guid byUserId, string userName, string ticketNumber, string title, CancellationToken ct = default);
    Task BroadcastTimeLogged(Guid byUserId, string userName, string ticketNumber, decimal hours, CancellationToken ct = default);
    Task NotifyManagerTeamUpdate(object snapshot, CancellationToken ct = default);
}

public sealed class TodoHubService(IHubContext<TodoHub> hubContext) : ITodoHubService
{
    public Task NotifyTaskCreated(Guid userId, object payload, CancellationToken ct = default)
        => hubContext.Clients.Group($"user:{userId}").SendAsync("TaskCreated", payload, ct);

    public Task NotifyTaskUpdated(Guid userId, object payload, CancellationToken ct = default)
        => hubContext.Clients.Group($"user:{userId}").SendAsync("TaskUpdated", payload, ct);

    public Task NotifyTaskDeleted(Guid userId, Guid taskId, CancellationToken ct = default)
        => hubContext.Clients.Group($"user:{userId}").SendAsync("TaskDeleted", taskId, ct);

    public Task NotifyListUpdated(Guid listId, object payload, CancellationToken ct = default)
        => hubContext.Clients.Group($"list:{listId}").SendAsync("ListUpdated", payload, ct);

    // ── Team activity — broadcast to all managers ────────────────────────────
    public Task BroadcastTicketOpened(Guid byUserId, string userName, string ticketNumber, string title, CancellationToken ct = default)
        => hubContext.Clients.Group("managers").SendAsync("TeamTicketOpened",
            new { byUserId, userName, ticketNumber, title, at = DateTime.UtcNow }, ct);

    public Task BroadcastTicketCompleted(Guid byUserId, string userName, string ticketNumber, string title, CancellationToken ct = default)
        => hubContext.Clients.Group("managers").SendAsync("TeamTicketCompleted",
            new { byUserId, userName, ticketNumber, title, at = DateTime.UtcNow }, ct);

    public Task BroadcastTimeLogged(Guid byUserId, string userName, string ticketNumber, decimal hours, CancellationToken ct = default)
        => hubContext.Clients.Group("managers").SendAsync("TeamTimeLogged",
            new { byUserId, userName, ticketNumber, hours, at = DateTime.UtcNow }, ct);

    public Task NotifyManagerTeamUpdate(object snapshot, CancellationToken ct = default)
        => hubContext.Clients.Group("managers").SendAsync("TeamUpdate", snapshot, ct);
}
