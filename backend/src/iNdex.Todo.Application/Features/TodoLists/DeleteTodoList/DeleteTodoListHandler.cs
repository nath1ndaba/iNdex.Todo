using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Common.Result;
using iNdex.Todo.Domain.Entities;
using iNdex.Todo.Domain.Errors;

namespace iNdex.Todo.Application.Features.TodoLists.DeleteTodoList;

public sealed record DeleteTodoListCommand(Guid Id);

public sealed class DeleteTodoListHandler(
    ITodoListRepository repository,
    IUnitOfWork unitOfWork)
    : IHandler<DeleteTodoListCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(
        DeleteTodoListCommand command,
        CancellationToken cancellationToken = default)
    {
        var exists = await repository.ExistsAsync(command.Id, cancellationToken);

        if (!exists)
            return Result.Failure<bool>(Error.NotFound(nameof(TodoList), command.Id));

        await repository.DeleteAsync(command.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
