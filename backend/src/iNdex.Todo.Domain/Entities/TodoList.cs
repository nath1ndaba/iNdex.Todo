using iNdex.Todo.Domain.Common;

namespace iNdex.Todo.Domain.Entities;

public sealed class TodoList : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public Guid OwnerId { get; set; }

    public User Owner { get; set; } = null!;
    public ICollection<TodoTask> Tasks { get; set; } = [];
}
