using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Common.Result;
using iNdex.Todo.Contracts.Responses;
using iNdex.Todo.Domain.Entities;
using iNdex.Todo.Domain.Errors;

namespace iNdex.Todo.Application.Features.TodoLists.GetTodoListById;

public sealed record GetTodoListByIdQuery(Guid Id);

public sealed class GetTodoListByIdHandler(ITodoListRepository repository)
    : IHandler<GetTodoListByIdQuery, TodoListResponse>
{
    public async Task<Result<TodoListResponse>> HandleAsync(
        GetTodoListByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        var list = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (list is null)
            return Result.Failure<TodoListResponse>(Error.NotFound(nameof(TodoList), request.Id));

        return Result.Success(new TodoListResponse(
            list.Id,
            list.Name,
            list.Description,
            list.Color,
            list.Icon,
            list.OwnerId,
            list.Tasks.Count,
            list.CreatedAt));
    }
}
