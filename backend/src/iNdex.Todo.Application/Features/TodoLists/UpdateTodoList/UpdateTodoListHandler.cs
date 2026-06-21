using FluentValidation;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Common.Result;
using iNdex.Todo.Contracts.Requests;
using iNdex.Todo.Contracts.Responses;
using iNdex.Todo.Domain.Entities;
using iNdex.Todo.Domain.Errors;

namespace iNdex.Todo.Application.Features.TodoLists.UpdateTodoList;

public sealed record UpdateTodoListCommand(Guid Id, UpdateTodoListRequest Request);

public sealed class UpdateTodoListHandler(
    ITodoListRepository repository,
    IUnitOfWork unitOfWork)
    : IHandler<UpdateTodoListCommand, TodoListResponse>
{
    public async Task<Result<TodoListResponse>> HandleAsync(
        UpdateTodoListCommand command,
        CancellationToken cancellationToken = default)
    {
        var list = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (list is null)
            return Result.Failure<TodoListResponse>(Error.NotFound(nameof(TodoList), command.Id));

        list.Name = command.Request.Name;
        list.Description = command.Request.Description;
        list.Color = command.Request.Color;
        list.Icon = command.Request.Icon;
        list.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(list, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

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

public sealed class UpdateTodoListValidator : AbstractValidator<UpdateTodoListRequest>
{
    public UpdateTodoListValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("List name is required.")
            .MaximumLength(200).WithMessage("List name must not exceed 200 characters.");

        RuleFor(x => x.Color)
            .Matches("^#[0-9A-Fa-f]{6}$").WithMessage("Color must be a valid hex color.")
            .When(x => x.Color is not null);
    }
}
