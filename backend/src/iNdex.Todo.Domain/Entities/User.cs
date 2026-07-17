using iNdex.Todo.Domain.Common;

namespace iNdex.Todo.Domain.Entities;

public sealed class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // AI workforce fields
    public string? SkillProfile        { get; set; }  // JSON array e.g. ["Android","API","React"]
    public float?  ProductivityScore   { get; set; }  // 0-100
    public float?  PerformanceRating   { get; set; }  // 0-5
    public string? TrainingAreas       { get; set; }  // JSON array
    public string? Role                { get; set; }  // "Intern","Junior","Senior","Manager" etc.
    public string? Department          { get; set; }  // "Engineering","Marketing" etc.

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
