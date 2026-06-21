using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace iNdex.Todo.Infrastructure.Persistence.Repositories;

public class Repository<TEntity>(AppDbContext context) : IRepository<TEntity>
    where TEntity : BaseEntity
{
    protected readonly DbSet<TEntity> DbSet = context.Set<TEntity>();

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public virtual async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => await DbSet.ToListAsync(cancellationToken);

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await DbSet.AddAsync(entity, cancellationToken);

    public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity is not null) DbSet.Remove(entity);
    }

    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(e => e.Id == id, cancellationToken);
}
