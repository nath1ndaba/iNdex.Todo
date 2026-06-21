using FluentValidation;
using iNdex.Todo.API.Extensions;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Features.Users.GetUserById;
using iNdex.Todo.Application.Features.Users.RegisterUser;
using iNdex.Todo.Contracts.Requests;
using iNdex.Todo.Contracts.Responses;

namespace iNdex.Todo.API.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapPost("/", RegisterUser)
            .WithName("RegisterUser")
            .WithSummary("Register a new user")
            .Produces<CreatedResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/{id:guid}", GetUserById)
            .WithName("GetUserById")
            .WithSummary("Get a user by ID")
            .Produces<UserResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> RegisterUser(
        RegisterUserRequest request,
        IValidator<RegisterUserRequest> validator,
        IHandler<RegisterUserRequest, CreatedResponse> handler,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAndHandleAsync(request, ct);
        if (validation.IsFailure) return validation.ToHttpResult();

        var result = await handler.HandleAsync(request, ct);
        return result.ToHttpResult(StatusCodes.Status201Created);
    }

    private static async Task<IResult> GetUserById(
        Guid id,
        IHandler<GetUserByIdQuery, UserResponse> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetUserByIdQuery(id), ct);
        return result.ToHttpResult();
    }
}
