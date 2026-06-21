using iNdex.Todo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iNdex.Todo.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(320).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.ProfileImageUrl).HasMaxLength(2048);

        builder.HasQueryFilter(u => !u.IsDeleted);

        builder.Property(u => u.PasswordHash).HasMaxLength(256).IsRequired();

        builder.HasMany(u => u.TodoLists).WithOne(l => l.Owner).HasForeignKey(l => l.OwnerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(u => u.Devices).WithOne(d => d.User).HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(u => u.Notifications).WithOne(n => n.User).HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(u => u.RefreshTokens).WithOne(r => r.User).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class TodoListConfiguration : IEntityTypeConfiguration<TodoList>
{
    public void Configure(EntityTypeBuilder<TodoList> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Name).HasMaxLength(200).IsRequired();
        builder.Property(l => l.Description).HasMaxLength(1000);
        builder.Property(l => l.Color).HasMaxLength(7);
        builder.Property(l => l.Icon).HasMaxLength(100);

        builder.HasQueryFilter(l => !l.IsDeleted);

        builder.HasMany(l => l.Tasks).WithOne(t => t.TodoList).HasForeignKey(t => t.TodoListId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class TodoTaskConfiguration : IEntityTypeConfiguration<TodoTask>
{
    public void Configure(EntityTypeBuilder<TodoTask> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Title).HasMaxLength(500).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(2000);
        builder.Property(t => t.Priority).HasConversion<string>();

        builder.HasQueryFilter(t => !t.IsDeleted);

        builder.HasMany(t => t.Comments).WithOne(c => c.Task).HasForeignKey(c => c.TaskId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(t => t.Attachments).WithOne(a => a.Task).HasForeignKey(a => a.TaskId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(t => t.Tags).WithOne(tag => tag.Task).HasForeignKey(tag => tag.TaskId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(t => t.Reminders).WithOne(r => r.Task).HasForeignKey(r => r.TaskId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(t => t.Recurrences).WithOne(r => r.Task).HasForeignKey(r => r.TaskId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(t => t.Activities).WithOne(a => a.Task).HasForeignKey(a => a.TaskId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class TaskCommentConfiguration : IEntityTypeConfiguration<TaskComment>
{
    public void Configure(EntityTypeBuilder<TaskComment> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Comment).HasMaxLength(2000).IsRequired();
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}

public sealed class TaskAttachmentConfiguration : IEntityTypeConfiguration<TaskAttachment>
{
    public void Configure(EntityTypeBuilder<TaskAttachment> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.FileName).HasMaxLength(255).IsRequired();
        builder.Property(a => a.FileUrl).HasMaxLength(2048).IsRequired();
        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}

public sealed class TaskTagConfiguration : IEntityTypeConfiguration<TaskTag>
{
    public void Configure(EntityTypeBuilder<TaskTag> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).HasMaxLength(100).IsRequired();
        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Color).HasMaxLength(7);
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Title).HasMaxLength(200).IsRequired();
        builder.Property(n => n.Body).HasMaxLength(1000).IsRequired();
        builder.HasQueryFilter(n => !n.IsDeleted);
    }
}

public sealed class SyncQueueConfiguration : IEntityTypeConfiguration<SyncQueue>
{
    public void Configure(EntityTypeBuilder<SyncQueue> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Operation).HasConversion<string>();
        builder.Property(s => s.Status).HasConversion<string>();
        builder.HasIndex(s => s.Status);
    }
}

public sealed class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.PushToken).HasMaxLength(512).IsRequired();
        builder.Property(d => d.Platform).HasConversion<string>();
        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}

public sealed class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Theme).HasMaxLength(50).IsRequired();
        builder.Property(s => s.Language).HasMaxLength(10).IsRequired();
        // Match the User global query filter to avoid EF warning on required principal
        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}

public sealed class TaskReminderConfiguration : IEntityTypeConfiguration<TaskReminder>
{
    public void Configure(EntityTypeBuilder<TaskReminder> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.NotificationType).HasConversion<string>();
        // Match TodoTask's global query filter
        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}

public sealed class TaskRecurrenceConfiguration : IEntityTypeConfiguration<TaskRecurrence>
{
    public void Configure(EntityTypeBuilder<TaskRecurrence> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Frequency).HasConversion<string>();
        // Match TodoTask's global query filter
        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}

public sealed class TaskActivityConfiguration : IEntityTypeConfiguration<TaskActivity>
{
    public void Configure(EntityTypeBuilder<TaskActivity> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.ActivityType).HasMaxLength(100).IsRequired();
        // Match TodoTask's global query filter
        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
//{
//    public void Configure(EntityTypeBuilder<AuditLog> builder)
//    {
//        builder.HasKey(a => a.Id);
//        builder.Property(a => a.EntityName).HasMaxLength(100).IsRequired();
//        builder.Property(a => a.Action).HasMaxLength(50).IsRequired();
//        builder.Property(a => a.PerformedBy).HasMaxLength(256).IsRequired();
//        builder.HasIndex(a => new { a.EntityName, a.EntityId });
//    }
//}
