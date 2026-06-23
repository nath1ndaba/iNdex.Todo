using iNdex.Todo.API.Extensions;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Features.Users.GetAllUsers;
using iNdex.Todo.Application.Features.Users.GetUserById;
using iNdex.Todo.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;

namespace iNdex.Todo.API.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", GetAllUsers)
            .WithName("GetAllUsers")
            .WithSummary("Get all users (for assignee pickers)")
            .Produces<List<UserResponse>>();

        group.MapGet("/{id:guid}", GetUserById)
            .WithName("GetUserById")
            .WithSummary("Get a user by ID")
            .Produces<UserResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetAllUsers(
        [FromServices] IHandler<GetAllUsersQuery, List<UserResponse>> handler,
        CancellationToken ct)
        => (await handler.HandleAsync(new GetAllUsersQuery(), ct)).ToHttpResult();

    private static async Task<IResult> GetUserById(
        Guid id,
        [FromServices] IHandler<GetUserByIdQuery, UserResponse> handler,
        CancellationToken ct)
        => (await handler.HandleAsync(new GetUserByIdQuery(id), ct)).ToHttpResult();
}
