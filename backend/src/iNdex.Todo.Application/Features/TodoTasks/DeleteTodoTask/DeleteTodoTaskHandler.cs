using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Common.Result;
using iNdex.Todo.Domain.Entities;
using iNdex.Todo.Domain.Errors;

namespace iNdex.Todo.Application.Features.TodoTasks.DeleteTodoTask;

public sealed record DeleteTodoTaskCommand(Guid Id);

public sealed class DeleteTodoTaskHandler(
    ITodoTaskRepository repository,
    IUnitOfWork unitOfWork)
    : IHandler<DeleteTodoTaskCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(
        DeleteTodoTaskCommand command,
        CancellationToken cancellationToken = default)
    {
        var exists = await repository.ExistsAsync(command.Id, cancellationToken);

        if (!exists)
            return Result.Failure<bool>(Error.NotFound(nameof(TodoTask), command.Id));

        await repository.DeleteAsync(command.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
