using iNdex.Todo.Mobile.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

namespace iNdex.Todo.Mobile.Services;

public class TodoRealtimeService(IConfiguration config) : IAsyncDisposable
{
    private HubConnection? _hub;

    public event Action<TodoTaskResponse>? OnTaskCreated;
    public event Action<TodoTaskResponse>? OnTaskUpdated;
    public event Action<Guid>? OnTaskDeleted;
    public event Action<TodoListResponse>? OnListUpdated;

    // Connection lifecycle events
    public event Action? OnConnected;
    public event Action? OnDisconnected;

    public async Task ConnectAsync(Guid userId)
    {
        var baseUrl = config["ApiBaseUrl"] ?? "https://localhost:51447";

        _hub = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/hubs/todo?userId={userId}")
            .WithAutomaticReconnect()
            .Build();

        _hub.On<TodoTaskResponse>("TaskCreated", t => OnTaskCreated?.Invoke(t));
        _hub.On<TodoTaskResponse>("TaskUpdated", t => OnTaskUpdated?.Invoke(t));
        _hub.On<Guid>("TaskDeleted", id => OnTaskDeleted?.Invoke(id));
        _hub.On<TodoListResponse>("ListUpdated", l => OnListUpdated?.Invoke(l));

        _hub.Reconnected  += _ => { OnConnected?.Invoke();    return Task.CompletedTask; };
        _hub.Reconnecting += _ => { OnDisconnected?.Invoke(); return Task.CompletedTask; };
        _hub.Closed       += _ => { OnDisconnected?.Invoke(); return Task.CompletedTask; };

        await _hub.StartAsync();
        OnConnected?.Invoke();
    }

    public async Task JoinListAsync(Guid listId)
    {
        if (_hub?.State == HubConnectionState.Connected)
            await _hub.InvokeAsync("JoinList", listId.ToString());
    }

    public async Task LeaveListAsync(Guid listId)
    {
        if (_hub?.State == HubConnectionState.Connected)
            await _hub.InvokeAsync("LeaveList", listId.ToString());
    }

    public async ValueTask DisposeAsync()
    {
        if (_hub is not null)
            await _hub.DisposeAsync();
    }
}
