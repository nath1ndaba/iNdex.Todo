using FluentValidation;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Common.Result;
using iNdex.Todo.Contracts.Requests;
using iNdex.Todo.Contracts.Responses;
using iNdex.Todo.Domain.Entities;
using iNdex.Todo.Domain.Enums;
using iNdex.Todo.Domain.Errors;

namespace iNdex.Todo.Application.Features.TodoTasks.UpdateTodoTask;

public sealed record UpdateTodoTaskCommand(Guid Id, UpdateTodoTaskRequest Request);

public sealed class UpdateTodoTaskHandler(
    ITodoTaskRepository repository,
    IUnitOfWork unitOfWork)
    : IHandler<UpdateTodoTaskCommand, TodoTaskResponse>
{
    public async Task<Result<TodoTaskResponse>> HandleAsync(
        UpdateTodoTaskCommand command,
        CancellationToken cancellationToken = default)
    {
        var task = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (task is null)
            return Result.Failure<TodoTaskResponse>(Error.NotFound(nameof(TodoTask), command.Id));

        task.Title = command.Request.Title;
        task.Description = command.Request.Description;
        task.DueDate = command.Request.DueDate;
        task.Priority = (TaskPriority)command.Request.Priority;
        task.CategoryId = command.Request.CategoryId;
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

public sealed class UpdateTodoTaskValidator : AbstractValidator<UpdateTodoTaskRequest>
{
    public UpdateTodoTaskValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required.")
            .MaximumLength(500).WithMessage("Task title must not exceed 500 characters.");

        RuleFor(x => x.Priority)
            .InclusiveBetween(0, 4).WithMessage("Priority must be between 0 and 4.");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future.")
            .When(x => x.DueDate.HasValue);
    }
}
