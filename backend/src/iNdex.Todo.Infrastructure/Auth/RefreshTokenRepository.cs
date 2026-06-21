using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Domain.Entities;
using iNdex.Todo.Infrastructure.Persistence;
using iNdex.Todo.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace iNdex.Todo.Infrastructure.Auth;

public sealed class RefreshTokenRepository(AppDbContext context)
    : Repository<RefreshToken>(context), IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(r => r.Token == token, ct);

    public async Task<List<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await DbSet
            .Where(r => r.UserId == userId && !r.IsRevoked && r.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(ct);
}
