namespace iNdex.Todo.Contracts.Responses;

public sealed record UserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? ProfileImageUrl,
    DateTime? LastLoginAt,
    DateTime CreatedAt);

public sealed record TodoListResponse(
    Guid Id,
    string Name,
    string? Description,
    string? Color,
    string? Icon,
    Guid OwnerId,
    int TaskCount,
    DateTime CreatedAt);

public sealed record TodoTaskResponse(
    Guid Id,
    string Title,
    string? Description,
    DateTime? DueDate,
    string Priority,
    Guid? CategoryId,
    Guid TodoListId,
    bool IsCompleted,
    DateTime? CompletedAt,
    DateTime CreatedAt);

public sealed record CreatedResponse(Guid Id);

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserResponse User);
