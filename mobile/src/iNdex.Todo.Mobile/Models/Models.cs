using MudBlazor;

namespace iNdex.Todo.Mobile.Models;

// ── Requests ─────────────────────────────────────────────────────────────────

public record RegisterUserRequest(string FirstName, string LastName, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record RefreshTokenRequest(string RefreshToken);
public record RevokeTokenRequest(string RefreshToken);

public record CreateTodoListRequest(string Name, string? Description, string? Color, string? Icon, Guid OwnerId);

public record UpdateTodoListRequest(string Name, string? Description, string? Color, string? Icon);

public record CreateTodoTaskRequest(
    string Title,
    string? Description,
    DateTime? DueDate,
    int Priority,
    Guid? CategoryId,
    Guid TodoListId);

public record UpdateTodoTaskRequest(
    string Title,
    string? Description,
    DateTime? DueDate,
    int Priority,
    Guid? CategoryId);

public record CompleteTaskRequest(bool IsCompleted);

// ── Responses ────────────────────────────────────────────────────────────────

public record CreatedResponse(Guid Id);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserResponse User);

public record UserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? ProfileImageUrl,
    DateTime? LastLoginAt,
    DateTime CreatedAt);

public record TodoListResponse(
    Guid Id,
    string Name,
    string? Description,
    string? Color,
    string? Icon,
    Guid OwnerId,
    int TaskCount,
    DateTime CreatedAt);

public record TodoTaskResponse(
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

// ── UI Helpers ────────────────────────────────────────────────────────────────

public enum TaskPriorityUi
{
    None = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public static class PriorityExtensions
{
    public static string ToIcon(this string priority) => priority switch
    {
        "Critical" => Icons.Material.Filled.PriorityHigh,
        "High"     => Icons.Material.Filled.KeyboardArrowUp,
        "Medium"   => Icons.Material.Filled.Remove,
        "Low"      => Icons.Material.Filled.KeyboardArrowDown,
        _          => Icons.Material.Filled.FiberManualRecord
    };

    public static MudBlazor.Color ToColor(this string priority) => priority switch
    {
        "Critical" => MudBlazor.Color.Error,
        "High"     => MudBlazor.Color.Warning,
        "Medium"   => MudBlazor.Color.Info,
        "Low"      => MudBlazor.Color.Success,
        _          => MudBlazor.Color.Default
    };
}
