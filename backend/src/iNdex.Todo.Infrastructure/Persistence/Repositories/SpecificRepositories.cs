using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace iNdex.Todo.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(AppDbContext context)
    : Repository<User>(context), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
}

public sealed class TodoListRepository(AppDbContext context)
    : Repository<TodoList>(context), ITodoListRepository
{
    public override async Task<TodoList?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(l => l.Tasks)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

    public async Task<List<TodoList>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(l => l.Tasks)
            .Where(l => l.OwnerId == ownerId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
}

public sealed class TodoTaskRepository(AppDbContext context)
    : Repository<TodoTask>(context), ITodoTaskRepository
{
    public override async Task<TodoTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(t => t.Tags)
            .Include(t => t.Reminders)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<List<TodoTask>> GetByListIdAsync(Guid listId, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(t => t.Tags)
            .Where(t => t.TodoListId == listId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
}

public sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
