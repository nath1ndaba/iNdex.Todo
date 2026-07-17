namespace iNdex.Todo.Contracts.Requests;

// Auth
public sealed record RegisterUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? Role       = null,
    string? Department = null);

public sealed record LoginRequest(
    string Email,
    string Password);

public sealed record RefreshTokenRequest(
    string RefreshToken);

public sealed record RevokeTokenRequest(
    string RefreshToken);

// TodoLists
public sealed record CreateTodoListRequest(
    string Name,
    string? Description,
    string? Color,
    string? Icon,
    Guid OwnerId);

public sealed record UpdateTodoListRequest(
    string Name,
    string? Description,
    string? Color,
    string? Icon);

// TodoTasks
public sealed record CreateTodoTaskRequest(
    string Title,
    string? Description,
    DateTime? DueDate,
    int Priority,
    Guid? CategoryId,
    Guid TodoListId);

public sealed record UpdateTodoTaskRequest(
    string Title,
    string? Description,
    DateTime? DueDate,
    int Priority,
    Guid? CategoryId);

public sealed record CompleteTaskRequest(
    bool IsCompleted);

// Tickets
public sealed record CreateTicketRequest(
    string Title,
    string? Description,
    int Priority,
    int Type,
    DateTime? DueDate,
    DateTime? StartDate,
    int EstimatedHours,
    Guid CreatedByUserId,
    Guid? AssignedToUserId);

public sealed record UpdateTicketRequest(
    string Title,
    string? Description,
    int Priority,
    int Type,
    int Status,
    DateTime? DueDate,
    DateTime? StartDate,
    int EstimatedHours,
    Guid? AssignedToUserId);

public sealed record LogTimeRequest(
    Guid TicketId,
    Guid UserId,
    decimal Hours,
    string Description,
    DateTime? LoggedDate);
