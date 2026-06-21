using Microsoft.AspNetCore.SignalR;

namespace iNdex.Todo.Infrastructure.SignalR;

public interface ITodoHubService
{
    Task NotifyTaskCreated(Guid userId, object payload, CancellationToken ct = default);
    Task NotifyTaskUpdated(Guid userId, object payload, CancellationToken ct = default);
    Task NotifyTaskDeleted(Guid userId, Guid taskId, CancellationToken ct = default);
    Task NotifyListUpdated(Guid listId, object payload, CancellationToken ct = default);
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
}
