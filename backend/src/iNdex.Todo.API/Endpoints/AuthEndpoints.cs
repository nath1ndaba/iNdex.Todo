using FluentValidation;
using iNdex.Todo.API.Extensions;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Features.Auth.Login;
using iNdex.Todo.Application.Features.Auth.RefreshToken;
using iNdex.Todo.Application.Features.Users.GetUserById;
using iNdex.Todo.Contracts.Requests;
using iNdex.Todo.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;

namespace iNdex.Todo.API.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth")
            .WithOpenApi()
            .AllowAnonymous();

        group.MapPost("/register", Register)
            .WithName("Register")
            .WithSummary("Register a new user and receive JWT tokens")
            .Produces<AuthResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Login with email and password")
            .Produces<AuthResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh", Refresh)
            .WithName("RefreshToken")
            .WithSummary("Exchange a refresh token for a new token pair")
            .Produces<AuthResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/revoke", Revoke)
            .WithName("RevokeToken")
            .WithSummary("Revoke a refresh token (logout)")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/me", Me)
            .WithName("GetCurrentUser")
            .WithSummary("Get the authenticated user's profile")
            .RequireAuthorization()
            .Produces<UserResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }

    private static async Task<IResult> Register(
        [FromBody]     RegisterUserRequest request,
        [FromServices] IValidator<RegisterUserRequest> validator,
        [FromServices] IHandler<RegisterUserRequest, AuthResponse> handler,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAndHandleAsync(request, ct);
        if (validation.IsFailure) return validation.ToHttpResult();

        var result = await handler.HandleAsync(request, ct);
        return result.ToHttpResult(StatusCodes.Status201Created);
    }

    private static async Task<IResult> Login(
        [FromBody]     LoginRequest request,
        [FromServices] IValidator<LoginRequest> validator,
        [FromServices] IHandler<LoginRequest, AuthResponse> handler,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAndHandleAsync(request, ct);
        if (validation.IsFailure) return validation.ToHttpResult();

        var result = await handler.HandleAsync(request, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> Refresh(
        [FromBody]     RefreshTokenRequest request,
        [FromServices] IHandler<RefreshTokenRequest, AuthResponse> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(request, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> Revoke(
        [FromBody]     RevokeTokenRequest request,
        [FromServices] IHandler<RevokeTokenRequest, bool> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(request, ct);
        return result.ToHttpResult(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> Me(
        [FromServices] ICurrentUserService currentUser,
        [FromServices] IHandler<GetUserByIdQuery, UserResponse> handler,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            return Results.Unauthorized();

        var result = await handler.HandleAsync(new GetUserByIdQuery(currentUser.UserId.Value), ct);
        return result.ToHttpResult();
    }
}
