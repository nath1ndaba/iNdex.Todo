using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace iNdex.Todo.API.Middleware;

public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problem = new ProblemDetails
        {
            Title = "An unexpected error occurred.",
            Status = StatusCodes.Status500InternalServerError,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
