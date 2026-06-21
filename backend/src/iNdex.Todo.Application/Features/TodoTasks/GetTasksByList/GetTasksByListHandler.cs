using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Common.Result;
using iNdex.Todo.Contracts.Responses;

namespace iNdex.Todo.Application.Features.TodoTasks.GetTasksByList;

public sealed record GetTasksByListQuery(Guid ListId);

public sealed class GetTasksByListHandler(ITodoTaskRepository repository)
    : IHandler<GetTasksByListQuery, List<TodoTaskResponse>>
{
    public async Task<Result<List<TodoTaskResponse>>> HandleAsync(
        GetTasksByListQuery request,
        CancellationToken cancellationToken = default)
    {
        var tasks = await repository.GetByListIdAsync(request.ListId, cancellationToken);

        var response = tasks.Select(t => new TodoTaskResponse(
            t.Id,
            t.Title,
            t.Description,
            t.DueDate,
            t.Priority.ToString(),
            t.CategoryId,
            t.TodoListId,
            t.IsCompleted,
            t.CompletedAt,
            t.CreatedAt)).ToList();

        return Result.Success(response);
    }
}
