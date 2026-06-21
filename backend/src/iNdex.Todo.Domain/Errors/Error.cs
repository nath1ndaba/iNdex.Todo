namespace iNdex.Todo.Domain.Errors;

public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.");

    public static Error NotFound(string entityName, Guid id) =>
        new($"{entityName}.NotFound", $"{entityName} with id '{id}' was not found.");

    public static Error Validation(string code, string message) =>
        new(code, message);

    public static Error Conflict(string code, string message) =>
        new(code, message);
}
