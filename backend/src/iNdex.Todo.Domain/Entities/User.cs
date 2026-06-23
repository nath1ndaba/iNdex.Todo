using iNdex.Todo.Domain.Common;

namespace iNdex.Todo.Domain.Entities;

public sealed class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Auth
    public string PasswordHash { get; set; } = string.Empty;

    public ICollection<TodoList> TodoLists { get; set; } = [];
    public ICollection<UserSettings> Settings { get; set; } = [];
    public ICollection<Device> Devices { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<Ticket> CreatedTickets  { get; set; } = [];
    public ICollection<Ticket> AssignedTickets { get; set; } = [];
    public ICollection<TimeLog> TimeLogs       { get; set; } = [];
}
