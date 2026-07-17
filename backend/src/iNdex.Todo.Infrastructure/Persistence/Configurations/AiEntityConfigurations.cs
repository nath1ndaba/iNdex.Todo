using iNdex.Todo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iNdex.Todo.Infrastructure.Persistence.Configurations;

public sealed class EmployeePerformanceConfiguration : IEntityTypeConfiguration<EmployeePerformance>
{
    public void Configure(EntityTypeBuilder<EmployeePerformance> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.PeriodLabel).HasMaxLength(20).IsRequired();
        builder.Property(e => e.GrowthTrend).HasMaxLength(20).IsRequired();
        builder.Property(e => e.RecommendedTraining).HasMaxLength(2000);
        builder.HasIndex(e => new { e.UserId, e.PeriodLabel }).IsUnique();

        builder.HasOne(e => e.User)
               .WithMany()
               .HasForeignKey(e => e.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class InternEvaluationConfiguration : IEntityTypeConfiguration<InternEvaluation>
{
    public void Configure(EntityTypeBuilder<InternEvaluation> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.PeriodLabel).HasMaxLength(20).IsRequired();
        builder.Property(e => e.AiRecommendation).HasMaxLength(50).IsRequired();
        builder.Property(e => e.MentorFeedback).HasMaxLength(2000);
        builder.HasIndex(e => new { e.InternId, e.PeriodLabel });

        builder.HasOne(e => e.Intern)
               .WithMany()
               .HasForeignKey(e => e.InternId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Mentor)
               .WithMany()
               .HasForeignKey(e => e.MentorId)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false);
    }
}

public sealed class ExecutiveSummaryConfiguration : IEntityTypeConfiguration<ExecutiveSummary>
{
    public void Configure(EntityTypeBuilder<ExecutiveSummary> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TopRisks).HasMaxLength(4000);
        builder.Property(e => e.RecommendedActions).HasMaxLength(4000);
        builder.Property(e => e.FullSummary).HasMaxLength(5000);
        builder.HasIndex(e => e.Date).IsUnique();
    }
}
