using iNdex.Todo.Domain.Common;
using iNdex.Todo.Domain.Enums;

namespace iNdex.Todo.Domain.Entities;

/// <summary>
/// A Ticket is a project/team work item that can be assigned to a user,
/// tracked with time logs, and is separate from personal TodoTasks.
/// </summary>
public sealed class Ticket : BaseEntity
{
    public string Title           { get; set; } = string.Empty;
    public string? Description    { get; set; }
    public string TicketNumber    { get; set; } = string.Empty; // e.g. TKT-001
    public TicketStatus Status    { get; set; } = TicketStatus.Open;
    public TaskPriority Priority  { get; set; } = TaskPriority.Medium;
    public TicketType Type        { get; set; } = TicketType.Task;
    public DateTime? DueDate      { get; set; }
    public DateTime? StartDate    { get; set; }
    public int EstimatedHours     { get; set; }

    // Ownership
    public Guid CreatedByUserId   { get; set; }
    public Guid? AssignedToUserId { get; set; }

    // AI-assisted fields (populated by the AI layer, never by user directly)
    public string? AiSummary             { get; set; }
    public string? AiSuggestedPriority   { get; set; }  // "Low","Medium","High","Critical"
    public float?  AiPriorityConfidence  { get; set; }  // 0-1
    public Guid?   AiSuggestedAssigneeId { get; set; }
    public float?  AiAssigneeConfidence  { get; set; }
    public float?  AiEstimatedDays       { get; set; }
    public string? AiDeadlineRisk        { get; set; }  // "Low","Medium","High"
    public string? AiCategory            { get; set; }
    public float?  AiCategoryConfidence  { get; set; }
    public DateTime? AiSummaryGeneratedAt { get; set; }

    // Navigation
    public User CreatedByUser      { get; set; } = null!;
    public User? AssignedToUser    { get; set; }
    public ICollection<TimeLog> TimeLogs { get; set; } = [];
    public ICollection<TicketComment> Comments { get; set; } = [];
    public ICollection<TicketAttachment> Attachments { get; set; } = [];
}

/// <summary>
/// A time log entry: one user logs N hours against a ticket with a description.
/// </summary>
public sealed class TimeLog : BaseEntity
{
    public Guid     TicketId    { get; set; }
    public Guid     UserId      { get; set; }
    public decimal  Hours       { get; set; }       // e.g. 3.5
    public string   Description { get; set; } = string.Empty;  // "Frontend work — auth page"
    public DateTime LoggedDate  { get; set; } = DateTime.UtcNow;

    public Ticket Ticket { get; set; } = null!;
    public User   User   { get; set; } = null!;
}

public sealed class TicketComment : BaseEntity
{
    public Guid   TicketId { get; set; }
    public Guid   UserId   { get; set; }
    public string Comment  { get; set; } = string.Empty;

    public Ticket Ticket { get; set; } = null!;
    public User   User   { get; set; } = null!;
}

public sealed class TicketAttachment : BaseEntity
{
    public Guid   TicketId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl  { get; set; } = string.Empty;
    public long   Size     { get; set; }

    public Ticket Ticket { get; set; } = null!;
}
