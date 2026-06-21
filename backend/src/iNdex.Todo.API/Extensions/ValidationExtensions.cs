using FluentValidation;
using iNdex.Todo.Application.Common.Result;
using iNdex.Todo.Domain.Errors;

namespace iNdex.Todo.API.Extensions;

public static class ValidationExtensions
{
    public static async Task<Result<T>> ValidateAndHandleAsync<T>(
        this IValidator<T> validator,
        T request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            var errorMessages = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<T>(Error.Validation("Validation.Failed", errorMessages));
        }

        return Result.Success(request);
    }
}
