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

// Tickets
public record CreateTicketRequest(
    string Title, string? Description, int Priority, int Type,
    DateTime? DueDate, DateTime? StartDate, int EstimatedHours,
    Guid CreatedByUserId, Guid? AssignedToUserId);

public record UpdateTicketRequest(
    string Title, string? Description, int Priority, int Type, int Status,
    DateTime? DueDate, DateTime? StartDate, int EstimatedHours, Guid? AssignedToUserId);

public record LogTimeRequest(
    Guid TicketId, Guid UserId, decimal Hours, string Description, DateTime? LoggedDate);

public record TicketResponse(
    Guid Id, string TicketNumber, string Title, string? Description,
    string Status, string Priority, string Type,
    DateTime? DueDate, DateTime? StartDate,
    int EstimatedHours, decimal TotalLoggedHours,
    Guid CreatedByUserId, string CreatedByName,
    Guid? AssignedToUserId, string? AssignedToName,
    int CommentCount, DateTime CreatedAt);

public record TimeLogResponse(
    Guid Id, Guid TicketId, string TicketNumber,
    Guid UserId, string UserName,
    decimal Hours, string Description, DateTime LoggedDate);

public record TimeLogSummaryResponse(
    Guid TicketId, string TicketNumber, string TicketTitle,
    decimal TotalHours, int EstimatedHours, decimal RemainingHours,
    List<TimeLogResponse> Logs);

public static class TicketExtensions
{
    public static string StatusIcon(this string status) => status switch
    {
        "InProgress" => Icons.Material.Filled.PlayCircle,
        "InReview"   => Icons.Material.Filled.RateReview,
        "Done"       => Icons.Material.Filled.CheckCircle,
        "Cancelled"  => Icons.Material.Filled.Cancel,
        _            => Icons.Material.Filled.RadioButtonUnchecked
    };
    public static MudBlazor.Color StatusColor(this string status) => status switch
    {
        "InProgress" => MudBlazor.Color.Info,
        "InReview"   => MudBlazor.Color.Warning,
        "Done"       => MudBlazor.Color.Success,
        "Cancelled"  => MudBlazor.Color.Error,
        _            => MudBlazor.Color.Default
    };
    public static string TypeIcon(this string type) => type switch
    {
        "Bug"         => Icons.Material.Filled.BugReport,
        "Feature"     => Icons.Material.Filled.NewReleases,
        "Improvement" => Icons.Material.Filled.TrendingUp,
        "Research"    => Icons.Material.Filled.Science,
        _             => Icons.Material.Filled.Task
    };
}

// ── AI Models ─────────────────────────────────────────────────────────────────
public record PredictPriorityRequest(string Title, string? Description, string? Category, string? TicketType);
public record SuggestAssigneeRequest(Guid TicketId, string TicketCategory, string TicketType, string TicketPriority);
public record PredictCompletionRequest(Guid TicketId, Guid? AssigneeId);
public record PredictDeadlineRiskRequest(Guid TicketId);
public record SummarizeTicketRequest(Guid TicketId);
public record NaturalLanguageTicketRequest(string Instruction, Guid CreatedByUserId);
public record SuggestCategoryRequest(string Title, string? Description);

public record PriorityPredictionResponse(string SuggestedPriority, float Confidence, string ConfidencePercent);
public record AssigneeSuggestion(Guid UserId, string Name, float Score, string SkillMatch);
public record AssigneeSuggestionResponse(List<AssigneeSuggestion> Suggestions);
public record CompletionPredictionResponse(float EstimatedDays, string EstimatedLabel);
public record DeadlineRiskResponse(string Risk, float Probability, string Explanation);
public record TicketSummaryResponse(Guid TicketId, string Summary, DateTime GeneratedAt);
public record NlTicketResponse(string Title, string? Description, string? AssigneeName, Guid? AssigneeId, string Priority, string Type, string? DueDate, string? Category);
public record CategorySuggestionResponse(string Category, float Confidence, string ConfidencePercent);
public record ExecutiveSummaryResponse(DateTime Date, int OpenTickets, int CriticalTickets, int CompletedThisWeek, float ReleaseReadiness, List<string> TopRisks, List<string> RecommendedActions, string Narrative);
public record ReleaseReadinessResponse(float Score, string Grade, string Analysis, bool ReadyToRelease);
