using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Common.Result;
using iNdex.Todo.Contracts.Responses;
using iNdex.Todo.Domain.Entities;
using iNdex.Todo.Domain.Errors;

namespace iNdex.Todo.Application.Features.TodoTasks.GetTodoTaskById;

public sealed record GetTodoTaskByIdQuery(Guid Id);

public sealed class GetTodoTaskByIdHandler(ITodoTaskRepository repository)
    : IHandler<GetTodoTaskByIdQuery, TodoTaskResponse>
{
    public async Task<Result<TodoTaskResponse>> HandleAsync(
        GetTodoTaskByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        var task = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (task is null)
            return Result.Failure<TodoTaskResponse>(Error.NotFound(nameof(TodoTask), request.Id));

        return Result.Success(new TodoTaskResponse(
            task.Id,
            task.Title,
            task.Description,
            task.DueDate,
            task.Priority.ToString(),
            task.CategoryId,
            task.TodoListId,
            task.IsCompleted,
            task.CompletedAt,
            task.CreatedAt));
    }
}
