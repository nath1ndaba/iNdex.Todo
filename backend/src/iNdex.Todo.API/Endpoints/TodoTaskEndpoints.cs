using FluentValidation;
using iNdex.Todo.API.Extensions;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Features.TodoTasks.CompleteTask;
using iNdex.Todo.Application.Features.TodoTasks.CreateTodoTask;
using iNdex.Todo.Application.Features.TodoTasks.DeleteTodoTask;
using iNdex.Todo.Application.Features.TodoTasks.GetTasksByList;
using iNdex.Todo.Application.Features.TodoTasks.GetTodoTaskById;
using iNdex.Todo.Application.Features.TodoTasks.UpdateTodoTask;
using iNdex.Todo.Contracts.Requests;
using iNdex.Todo.Contracts.Responses;

namespace iNdex.Todo.API.Endpoints;

public static class TodoTaskEndpoints
{
    public static IEndpointRouteBuilder MapTodoTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks")
            .WithTags("TodoTasks")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapPost("/", CreateTodoTask)
            .WithName("CreateTodoTask")
            .WithSummary("Create a new task")
            .Produces<CreatedResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/by-list/{listId:guid}", GetTasksByList)
            .WithName("GetTasksByList")
            .WithSummary("Get all tasks for a todo list")
            .Produces<List<TodoTaskResponse>>();

        group.MapGet("/{id:guid}", GetTodoTaskById)
            .WithName("GetTodoTaskById")
            .WithSummary("Get a task by ID")
            .Produces<TodoTaskResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", UpdateTodoTask)
            .WithName("UpdateTodoTask")
            .WithSummary("Update a task")
            .Produces<TodoTaskResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:guid}/complete", CompleteTask)
            .WithName("CompleteTask")
            .WithSummary("Mark a task as complete or incomplete")
            .Produces<TodoTaskResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteTodoTask)
            .WithName("DeleteTodoTask")
            .WithSummary("Delete a task")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> CreateTodoTask(
        CreateTodoTaskRequest request,
        IValidator<CreateTodoTaskRequest> validator,
        IHandler<CreateTodoTaskRequest, CreatedResponse> handler,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAndHandleAsync(request, ct);
        if (validation.IsFailure) return validation.ToHttpResult();

        var result = await handler.HandleAsync(request, ct);
        return result.ToHttpResult(StatusCodes.Status201Created);
    }

    private static async Task<IResult> GetTasksByList(
        Guid listId,
        IHandler<GetTasksByListQuery, List<TodoTaskResponse>> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetTasksByListQuery(listId), ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetTodoTaskById(
        Guid id,
        IHandler<GetTodoTaskByIdQuery, TodoTaskResponse> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetTodoTaskByIdQuery(id), ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateTodoTask(
        Guid id,
        UpdateTodoTaskRequest request,
        IValidator<UpdateTodoTaskRequest> validator,
        IHandler<UpdateTodoTaskCommand, TodoTaskResponse> handler,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAndHandleAsync(request, ct);
        if (validation.IsFailure) return validation.ToHttpResult();

        var result = await handler.HandleAsync(new UpdateTodoTaskCommand(id, request), ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CompleteTask(
        Guid id,
        CompleteTaskRequest request,
        IValidator<CompleteTaskRequest> validator,
        IHandler<CompleteTaskCommand, TodoTaskResponse> handler,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAndHandleAsync(request, ct);
        if (validation.IsFailure) return validation.ToHttpResult();

        var result = await handler.HandleAsync(new CompleteTaskCommand(id, request), ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> DeleteTodoTask(
        Guid id,
        IHandler<DeleteTodoTaskCommand, bool> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new DeleteTodoTaskCommand(id), ct);
        return result.ToHttpResult(StatusCodes.Status204NoContent);
    }
}
