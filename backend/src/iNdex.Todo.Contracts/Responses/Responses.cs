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

public sealed record TicketResponse(
    Guid Id,
    string TicketNumber,
    string Title,
    string? Description,
    string Status,
    string Priority,
    string Type,
    DateTime? DueDate,
    DateTime? StartDate,
    int EstimatedHours,
    decimal TotalLoggedHours,
    Guid CreatedByUserId,
    string CreatedByName,
    Guid? AssignedToUserId,
    string? AssignedToName,
    int CommentCount,
    DateTime CreatedAt);

public sealed record TimeLogResponse(
    Guid Id,
    Guid TicketId,
    string TicketNumber,
    Guid UserId,
    string UserName,
    decimal Hours,
    string Description,
    DateTime LoggedDate);

public sealed record TimeLogSummaryResponse(
    Guid TicketId,
    string TicketNumber,
    string TicketTitle,
    decimal TotalHours,
    int EstimatedHours,
    decimal RemainingHours,
    List<TimeLogResponse> Logs);
