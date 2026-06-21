using iNdex.Todo.Domain.Common;
using iNdex.Todo.Domain.Enums;

namespace iNdex.Todo.Domain.Entities;

public sealed class TodoTask : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.None;
    public Guid? CategoryId { get; set; }
    public Guid TodoListId { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }

    public TodoList TodoList { get; set; } = null!;
    public Category? Category { get; set; }
    public ICollection<TaskComment> Comments { get; set; } = [];
    public ICollection<TaskAttachment> Attachments { get; set; } = [];
    public ICollection<TaskTag> Tags { get; set; } = [];
    public ICollection<TaskReminder> Reminders { get; set; } = [];
    public ICollection<TaskRecurrence> Recurrences { get; set; } = [];
    public ICollection<TaskActivity> Activities { get; set; } = [];
}
