using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Common.Result;
using iNdex.Todo.Contracts.Requests;
using iNdex.Todo.Contracts.Responses;
using iNdex.Todo.Domain.Entities;
using iNdex.Todo.Domain.Errors;

namespace iNdex.Todo.Application.Features.TodoLists.CreateTodoList;

public sealed class CreateTodoListHandler(
    ITodoListRepository repository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : IHandler<CreateTodoListRequest, CreatedResponse>
{
    public async Task<Result<CreatedResponse>> HandleAsync(
        CreateTodoListRequest request,
        CancellationToken cancellationToken = default)
    {
        var ownerExists = await userRepository.ExistsAsync(request.OwnerId, cancellationToken);
        if (!ownerExists)
            return Result.Failure<CreatedResponse>(Error.NotFound(nameof(User), request.OwnerId));

        var list = new TodoList
        {
            Name = request.Name,
            Description = request.Description,
            Color = request.Color,
            Icon = request.Icon,
            OwnerId = request.OwnerId,
            CreatedBy = request.OwnerId.ToString()
        };

        await repository.AddAsync(list, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreatedResponse(list.Id));
    }
}
