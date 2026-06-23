using iNdex.Todo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace iNdex.Todo.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<TodoList> TodoLists => Set<TodoList>();
    public DbSet<TodoTask> TodoTasks => Set<TodoTask>();
    public DbSet<TaskComment> TaskComments => Set<TaskComment>();
    public DbSet<TaskAttachment> TaskAttachments => Set<TaskAttachment>();
    public DbSet<TaskTag> TaskTags => Set<TaskTag>();
    public DbSet<TaskReminder> TaskReminders => Set<TaskReminder>();
    public DbSet<TaskRecurrence> TaskRecurrences => Set<TaskRecurrence>();
    public DbSet<TaskActivity> TaskActivities => Set<TaskActivity>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<SyncQueue> SyncQueues => Set<SyncQueue>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TimeLog> TimeLogs => Set<TimeLog>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<TicketAttachment> TicketAttachments => Set<TicketAttachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
