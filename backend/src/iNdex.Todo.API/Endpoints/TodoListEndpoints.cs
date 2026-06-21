using FluentValidation;
using iNdex.Todo.API.Extensions;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Features.TodoLists.CreateTodoList;
using iNdex.Todo.Application.Features.TodoLists.DeleteTodoList;
using iNdex.Todo.Application.Features.TodoLists.GetAllTodoLists;
using iNdex.Todo.Application.Features.TodoLists.GetTodoListById;
using iNdex.Todo.Application.Features.TodoLists.UpdateTodoList;
using iNdex.Todo.Contracts.Requests;
using iNdex.Todo.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;

namespace iNdex.Todo.API.Endpoints;

public static class TodoListEndpoints
{
    public static IEndpointRouteBuilder MapTodoListEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/lists")
            .WithTags("TodoLists")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapPost("/", CreateTodoList)
            .WithName("CreateTodoList")
            .WithSummary("Create a new todo list")
            .Produces<CreatedResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/", GetAllTodoLists)
            .WithName("GetAllTodoLists")
            .WithSummary("Get all todo lists for an owner")
            .Produces<List<TodoListResponse>>();

        group.MapGet("/{id:guid}", GetTodoListById)
            .WithName("GetTodoListById")
            .WithSummary("Get a todo list by ID")
            .Produces<TodoListResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", UpdateTodoList)
            .WithName("UpdateTodoList")
            .WithSummary("Update a todo list")
            .Produces<TodoListResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteTodoList)
            .WithName("DeleteTodoList")
            .WithSummary("Delete a todo list")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> CreateTodoList(
        [FromBody]     CreateTodoListRequest request,
        [FromServices] IValidator<CreateTodoListRequest> validator,
        [FromServices] IHandler<CreateTodoListRequest, CreatedResponse> handler,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAndHandleAsync(request, ct);
        if (validation.IsFailure) return validation.ToHttpResult();

        var result = await handler.HandleAsync(request, ct);
        return result.ToHttpResult(StatusCodes.Status201Created);
    }

    private static async Task<IResult> GetAllTodoLists(
        Guid ownerId,
        [FromServices] IHandler<GetAllTodoListsQuery, List<TodoListResponse>> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetAllTodoListsQuery(ownerId), ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetTodoListById(
        Guid id,
        [FromServices] IHandler<GetTodoListByIdQuery, TodoListResponse> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetTodoListByIdQuery(id), ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateTodoList(
        Guid id,
        [FromBody]     UpdateTodoListRequest request,
        [FromServices] IValidator<UpdateTodoListRequest> validator,
        [FromServices] IHandler<UpdateTodoListCommand, TodoListResponse> handler,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAndHandleAsync(request, ct);
        if (validation.IsFailure) return validation.ToHttpResult();

        var result = await handler.HandleAsync(new UpdateTodoListCommand(id, request), ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> DeleteTodoList(
        Guid id,
        [FromServices] IHandler<DeleteTodoListCommand, bool> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new DeleteTodoListCommand(id), ct);
        return result.ToHttpResult(StatusCodes.Status204NoContent);
    }
}
