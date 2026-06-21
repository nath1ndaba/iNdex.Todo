using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Common.Result;
using iNdex.Todo.Contracts.Responses;

namespace iNdex.Todo.Application.Features.TodoLists.GetAllTodoLists;

public sealed record GetAllTodoListsQuery(Guid OwnerId);

public sealed class GetAllTodoListsHandler(ITodoListRepository repository)
    : IHandler<GetAllTodoListsQuery, List<TodoListResponse>>
{
    public async Task<Result<List<TodoListResponse>>> HandleAsync(
        GetAllTodoListsQuery request,
        CancellationToken cancellationToken = default)
    {
        var lists = await repository.GetByOwnerIdAsync(request.OwnerId, cancellationToken);

        var response = lists.Select(l => new TodoListResponse(
            l.Id,
            l.Name,
            l.Description,
            l.Color,
            l.Icon,
            l.OwnerId,
            l.Tasks.Count,
            l.CreatedAt)).ToList();

        return Result.Success(response);
    }
}
