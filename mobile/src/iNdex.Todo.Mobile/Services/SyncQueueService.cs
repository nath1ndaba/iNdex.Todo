using System.Text.Json;

namespace iNdex.Todo.Mobile.Services;

/// <summary>
/// Queues mutations locally when offline and replays them when connectivity resumes.
/// Uses in-memory queue for now; replace backing store with SQLite for true offline support.
/// </summary>
public class SyncQueueService
{
    private readonly Queue<SyncEntry> _queue = new();
    private bool _isSyncing;

    public void Enqueue(string entityType, Guid entityId, string operation, object payload)
    {
        _queue.Enqueue(new SyncEntry(
            entityType,
            entityId,
            operation,
            JsonSerializer.Serialize(payload),
            DateTime.UtcNow));

        Console.WriteLine($"[SyncQueue] Queued {operation} on {entityType}/{entityId}. Queue depth: {_queue.Count}");
    }

    public async Task FlushAsync(Func<SyncEntry, Task<bool>> processor)
    {
        if (_isSyncing || _queue.Count == 0) return;
        _isSyncing = true;

        try
        {
            while (_queue.TryPeek(out _))
            {
                var entry = _queue.Peek();
                var success = await processor(entry);

                if (success)
                    _queue.Dequeue();
                else
                    break; // stop on first failure; retry next time
            }
        }
        finally
        {
            _isSyncing = false;
        }
    }

    public int PendingCount => _queue.Count;
}

public record SyncEntry(
    string EntityType,
    Guid EntityId,
    string Operation,
    string Payload,
    DateTime QueuedAt);
