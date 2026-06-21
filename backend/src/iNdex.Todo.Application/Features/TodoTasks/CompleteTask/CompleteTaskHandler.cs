using FluentValidation;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Common.Result;
using iNdex.Todo.Contracts.Requests;
using iNdex.Todo.Contracts.Responses;
using iNdex.Todo.Domain.Entities;
using iNdex.Todo.Domain.Errors;

namespace iNdex.Todo.Application.Features.TodoTasks.CompleteTask;

public sealed record CompleteTaskCommand(Guid Id, CompleteTaskRequest Request);

public sealed class CompleteTaskHandler(
    ITodoTaskRepository repository,
    IUnitOfWork unitOfWork)
    : IHandler<CompleteTaskCommand, TodoTaskResponse>
{
    public async Task<Result<TodoTaskResponse>> HandleAsync(
        CompleteTaskCommand command,
        CancellationToken cancellationToken = default)
    {
        var task = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (task is null)
            return Result.Failure<TodoTaskResponse>(Error.NotFound(nameof(TodoTask), command.Id));

        task.IsCompleted = command.Request.IsCompleted;
        task.CompletedAt = command.Request.IsCompleted ? DateTime.UtcNow : null;
        task.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(task, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

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

public sealed class CompleteTaskValidator : AbstractValidator<CompleteTaskRequest>
{
    public CompleteTaskValidator()
    {
        RuleFor(x => x.IsCompleted)
            .NotNull().WithMessage("IsCompleted field is required.");
    }
}
