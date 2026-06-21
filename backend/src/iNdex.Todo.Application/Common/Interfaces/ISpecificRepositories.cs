using iNdex.Todo.Domain.Entities;

namespace iNdex.Todo.Application.Common.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}

public interface ITodoListRepository : IRepository<TodoList>
{
    Task<List<TodoList>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
}

public interface ITodoTaskRepository : IRepository<TodoTask>
{
    Task<List<TodoTask>> GetByListIdAsync(Guid listId, CancellationToken cancellationToken = default);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
