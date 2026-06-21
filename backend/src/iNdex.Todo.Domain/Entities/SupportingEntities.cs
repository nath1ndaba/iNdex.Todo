using iNdex.Todo.Domain.Common;
using iNdex.Todo.Domain.Enums;

namespace iNdex.Todo.Domain.Entities;

public sealed class TaskComment : BaseEntity
{
    public Guid TaskId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public Guid UserId { get; set; }

    public TodoTask Task { get; set; } = null!;
    public User User { get; set; } = null!;
}

public sealed class TaskAttachment : BaseEntity
{
    public Guid TaskId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long Size { get; set; }

    public TodoTask Task { get; set; } = null!;
}

public sealed class TaskTag : BaseEntity
{
    public Guid TaskId { get; set; }
    public string Name { get; set; } = string.Empty;

    public TodoTask Task { get; set; } = null!;
}

public sealed class TaskReminder : BaseEntity
{
    public Guid TaskId { get; set; }
    public DateTime ReminderAt { get; set; }
    public NotificationType NotificationType { get; set; }

    public TodoTask Task { get; set; } = null!;
}

public sealed class TaskRecurrence : BaseEntity
{
    public Guid TaskId { get; set; }
    public RecurrenceFrequency Frequency { get; set; }
    public int Interval { get; set; } = 1;

    public TodoTask Task { get; set; } = null!;
}

public sealed class TaskActivity : BaseEntity
{
    public Guid TaskId { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    public TodoTask Task { get; set; } = null!;
}

public sealed class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public Guid OwnerId { get; set; }

    public User Owner { get; set; } = null!;
    public ICollection<TodoTask> Tasks { get; set; } = [];
}

public sealed class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public DateTime? ReadAt { get; set; }

    public User User { get; set; } = null!;
}

public sealed class SyncQueue : BaseEntity
{
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public SyncOperation Operation { get; set; }
    public string Payload { get; set; } = string.Empty;
    public SyncStatus Status { get; set; } = SyncStatus.Pending;
}

public sealed class Device : BaseEntity
{
    public Guid UserId { get; set; }
    public DevicePlatform Platform { get; set; }
    public string PushToken { get; set; } = string.Empty;

    public User User { get; set; } = null!;
}

public sealed class UserSettings : BaseEntity
{
    public Guid UserId { get; set; }
    public string Theme { get; set; } = "system";
    public string Language { get; set; } = "en";
    public bool NotificationEnabled { get; set; } = true;

    public User User { get; set; } = null!;
}

public sealed class AuditLog : BaseEntity
{
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
}
