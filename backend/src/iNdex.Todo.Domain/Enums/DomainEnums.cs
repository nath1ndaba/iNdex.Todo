namespace iNdex.Todo.Domain.Enums;

public enum TaskPriority
{
    None = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum RecurrenceFrequency
{
    Daily = 0,
    Weekly = 1,
    Monthly = 2,
    Yearly = 3
}

public enum NotificationType
{
    Email = 0,
    Push = 1,
    InApp = 2
}

public enum TicketStatus
{
    Open = 0,
    InProgress = 1,
    InReview = 2,
    Done = 3,
    Cancelled = 4
}

public enum TicketType
{
    Task = 0,
    Bug = 1,
    Feature = 2,
    Improvement = 3,
    Research = 4
}

public enum SyncOperation
{
    Create = 0,
    Update = 1,
    Delete = 2
}

public enum SyncStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Failed = 3
}

public enum DevicePlatform
{
    Android = 0,
    iOS = 1,
    Web = 2,
    Windows = 3,
    MacOS = 4
}
