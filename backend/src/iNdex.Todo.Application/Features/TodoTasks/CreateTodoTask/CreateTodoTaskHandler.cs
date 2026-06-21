using FluentValidation;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Common.Result;
using iNdex.Todo.Contracts.Requests;
using iNdex.Todo.Contracts.Responses;
using iNdex.Todo.Domain.Entities;
using iNdex.Todo.Domain.Enums;
using iNdex.Todo.Domain.Errors;

namespace iNdex.Todo.Application.Features.TodoTasks.CreateTodoTask;

public sealed class CreateTodoTaskHandler(
    ITodoTaskRepository taskRepository,
    ITodoListRepository listRepository,
    IUnitOfWork unitOfWork)
    : IHandler<CreateTodoTaskRequest, CreatedResponse>
{
    public async Task<Result<CreatedResponse>> HandleAsync(
        CreateTodoTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var listExists = await listRepository.ExistsAsync(request.TodoListId, cancellationToken);
        if (!listExists)
            return Result.Failure<CreatedResponse>(Error.NotFound(nameof(TodoList), request.TodoListId));

        var task = new TodoTask
        {
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            Priority = (TaskPriority)request.Priority,
            CategoryId = request.CategoryId,
            TodoListId = request.TodoListId,
            CreatedBy = "system"
        };

        await taskRepository.AddAsync(task, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreatedResponse(task.Id));
    }
}

public sealed class CreateTodoTaskValidator : AbstractValidator<CreateTodoTaskRequest>
{
    public CreateTodoTaskValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required.")
            .MaximumLength(500).WithMessage("Task title must not exceed 500 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.Priority)
            .InclusiveBetween(0, 4).WithMessage("Priority must be between 0 (None) and 4 (Critical).");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future.")
            .When(x => x.DueDate.HasValue);

        RuleFor(x => x.TodoListId)
            .NotEmpty().WithMessage("TodoListId is required.");
    }
}
