using iNdex.Todo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iNdex.Todo.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Token).HasMaxLength(256).IsRequired();
        builder.Property(r => r.ReplacedByToken).HasMaxLength(256);
        builder.Property(r => r.RevokedReason).HasMaxLength(200);
        builder.HasIndex(r => r.Token).IsUnique();
        builder.HasIndex(r => new { r.UserId, r.IsRevoked });

        builder.HasOne(r => r.User)
               .WithMany(u => u.RefreshTokens)
               .HasForeignKey(r => r.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
