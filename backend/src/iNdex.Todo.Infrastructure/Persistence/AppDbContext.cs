using Humanizer;
using iNdex.Todo.Domain.Entities;
using iNdex.Todo.Infrastructure.Persistence.Configurations;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // TABLES
            entity.SetTableName(
                entity.ClrType.Name.Underscore().Pluralize().ToLower()
            );

            // COLUMNS (THIS FIXES YOUR ERROR)
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(
                    property.Name.Underscore().ToLower()
                );
            }
        }
    }
}
