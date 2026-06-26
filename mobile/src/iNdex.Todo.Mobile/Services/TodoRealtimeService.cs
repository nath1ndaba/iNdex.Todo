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

    // Team activity events — fires on manager's device in real time
    public event Action<TeamActivityEvent>? OnTeamTicketOpened;
    public event Action<TeamActivityEvent>? OnTeamTicketCompleted;
    public event Action<TeamActivityEvent>? OnTeamTimeLogged;
    public event Action<object>? OnTeamUpdate;

    // Connection lifecycle events
    public event Action? OnConnected;
    public event Action? OnDisconnected;

    public async Task ConnectAsync(Guid userId, string? role = null)
    {
        var baseUrl = config["ApiBaseUrl"] ?? "https://localhost:51447";
        var roleParam = string.IsNullOrEmpty(role) ? "" : $"&role={Uri.EscapeDataString(role)}";

        _hub = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/hubs/todo?userId={userId}{roleParam}")
            .WithAutomaticReconnect()
            .Build();

        _hub.On<TodoTaskResponse>("TaskCreated", t => OnTaskCreated?.Invoke(t));
        _hub.On<TodoTaskResponse>("TaskUpdated", t => OnTaskUpdated?.Invoke(t));
        _hub.On<Guid>("TaskDeleted", id => OnTaskDeleted?.Invoke(id));
        _hub.On<TodoListResponse>("ListUpdated", l => OnListUpdated?.Invoke(l));

        // Team events
        _hub.On<TeamActivityEvent>("TeamTicketOpened", e => OnTeamTicketOpened?.Invoke(e));
        _hub.On<TeamActivityEvent>("TeamTicketCompleted", e => OnTeamTicketCompleted?.Invoke(e));
        _hub.On<TeamActivityEvent>("TeamTimeLogged", e => OnTeamTimeLogged?.Invoke(e));
        _hub.On<object>("TeamUpdate", e => OnTeamUpdate?.Invoke(e));

        _hub.Reconnected += _ => { OnConnected?.Invoke(); return Task.CompletedTask; };
        _hub.Reconnecting += _ => { OnDisconnected?.Invoke(); return Task.CompletedTask; };
        _hub.Closed += _ => { OnDisconnected?.Invoke(); return Task.CompletedTask; };

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
