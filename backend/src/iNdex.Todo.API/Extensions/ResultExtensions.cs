using iNdex.Todo.Application.Common.Result;
using Microsoft.AspNetCore.Mvc;

namespace iNdex.Todo.API.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result, int successStatusCode = StatusCodes.Status200OK)
    {
        if (result.IsFailure)
        {
            var error = result.Error;

            return error.Code.Contains("NotFound")
                ? Results.NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = error.Message,
                    Status = StatusCodes.Status404NotFound
                })
                : error.Code.Contains("Conflict")
                    ? Results.Conflict(new ProblemDetails
                    {
                        Title = "Conflict",
                        Detail = error.Message,
                        Status = StatusCodes.Status409Conflict
                    })
                    : error.Code.Contains("Validation")
                        ? Results.UnprocessableEntity(new ProblemDetails
                        {
                            Title = "Validation Failed",
                            Detail = error.Message,
                            Status = StatusCodes.Status422UnprocessableEntity
                        })
                        : Results.BadRequest(new ProblemDetails
                        {
                            Title = "Bad Request",
                            Detail = error.Message,
                            Status = StatusCodes.Status400BadRequest
                        });
        }

        return successStatusCode switch
        {
            StatusCodes.Status201Created => Results.Created(string.Empty, result.Value),
            StatusCodes.Status204NoContent => Results.NoContent(),
            _ => Results.Ok(result.Value)
        };
    }
}
