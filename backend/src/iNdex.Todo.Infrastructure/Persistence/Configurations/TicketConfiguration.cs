using iNdex.Todo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iNdex.Todo.Infrastructure.Persistence.Configurations;

public sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Title).HasMaxLength(500).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(5000);
        builder.Property(t => t.TicketNumber).HasMaxLength(20).IsRequired();
        builder.Property(t => t.Status).HasConversion<string>();
        builder.Property(t => t.Priority).HasConversion<string>();
        builder.Property(t => t.Type).HasConversion<string>();

        builder.HasIndex(t => t.TicketNumber).IsUnique();
        builder.HasIndex(t => t.AssignedToUserId);
        builder.HasIndex(t => t.Status);

        builder.HasQueryFilter(t => !t.IsDeleted);

        builder.HasOne(t => t.CreatedByUser)
               .WithMany(u => u.CreatedTickets)
               .HasForeignKey(t => t.CreatedByUserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.AssignedToUser)
               .WithMany(u => u.AssignedTickets)
               .HasForeignKey(t => t.AssignedToUserId)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false);

        builder.HasMany(t => t.TimeLogs)
               .WithOne(l => l.Ticket)
               .HasForeignKey(l => l.TicketId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Comments)
               .WithOne(c => c.Ticket)
               .HasForeignKey(c => c.TicketId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Attachments)
               .WithOne(a => a.Ticket)
               .HasForeignKey(a => a.TicketId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class TimeLogConfiguration : IEntityTypeConfiguration<TimeLog>
{
    public void Configure(EntityTypeBuilder<TimeLog> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Hours).HasPrecision(5, 2).IsRequired();
        builder.Property(l => l.Description).HasMaxLength(1000).IsRequired();
        builder.HasIndex(l => new { l.TicketId, l.UserId });
        builder.HasIndex(l => l.LoggedDate);

        builder.HasOne(l => l.User)
               .WithMany(u => u.TimeLogs)
               .HasForeignKey(l => l.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class TicketCommentConfiguration : IEntityTypeConfiguration<TicketComment>
{
    public void Configure(EntityTypeBuilder<TicketComment> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Comment).HasMaxLength(2000).IsRequired();
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}

public sealed class TicketAttachmentConfiguration : IEntityTypeConfiguration<TicketAttachment>
{
    public void Configure(EntityTypeBuilder<TicketAttachment> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.FileName).HasMaxLength(255).IsRequired();
        builder.Property(a => a.FileUrl).HasMaxLength(2048).IsRequired();
    }
}
