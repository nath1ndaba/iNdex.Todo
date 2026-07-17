namespace iNdex.Todo.Contracts.AI;

// ── Requests ──────────────────────────────────────────────────────────────────

public sealed record PredictPriorityRequest(
    string Title,
    string? Description,
    string? Category,
    string? TicketType);

public sealed record SuggestAssigneeRequest(
    Guid TicketId,
    string TicketCategory,
    string TicketType,
    string TicketPriority);

public sealed record PredictCompletionRequest(
    Guid TicketId,
    Guid? AssigneeId);

public sealed record PredictDeadlineRiskRequest(
    Guid TicketId);

public sealed record SummarizeTicketRequest(
    Guid TicketId);

public sealed record NaturalLanguageTicketRequest(
    string Instruction,
    Guid CreatedByUserId);

public sealed record SuggestCategoryRequest(
    string Title,
    string? Description);

public sealed record GenerateExecutiveSummaryRequest(
    DateTime? Date);

public sealed record ReleaseReadinessRequest;

// ── Responses ─────────────────────────────────────────────────────────────────

public sealed record PriorityPredictionResponse(
    string SuggestedPriority,
    float  Confidence,           // 0-1
    string ConfidencePercent);   // "95%"

public sealed record AssigneeSuggestion(
    Guid   UserId,
    string Name,
    float  Score,
    string SkillMatch);

public sealed record AssigneeSuggestionResponse(
    List<AssigneeSuggestion> Suggestions);

public sealed record CompletionPredictionResponse(
    float  EstimatedDays,
    string EstimatedLabel);   // "~3.5 days"

public sealed record DeadlineRiskResponse(
    string Risk,              // "Low","Medium","High"
    float  Probability,
    string Explanation);

public sealed record TicketSummaryResponse(
    Guid   TicketId,
    string Summary,
    DateTime GeneratedAt);

public sealed record NlTicketResponse(
    string  Title,
    string? Description,
    string? AssigneeName,
    Guid?   AssigneeId,
    string  Priority,
    string  Type,
    string? DueDate,
    string? Category);

public sealed record CategorySuggestionResponse(
    string Category,
    float  Confidence,
    string ConfidencePercent);

public sealed record ExecutiveSummaryResponse(
    DateTime Date,
    int      OpenTickets,
    int      CriticalTickets,
    int      CompletedThisWeek,
    float    ReleaseReadiness,
    List<string> TopRisks,
    List<string> RecommendedActions,
    string   Narrative);

public sealed record ReleaseReadinessResponse(
    float  Score,
    string Grade,         // "A","B","C","D","F"
    string Analysis,
    bool   ReadyToRelease);

public sealed record EmployeePerformanceResponse(
    Guid   UserId,
    string Name,
    string Role,
    int    TasksAssigned,
    int    TasksCompleted,
    float  CompletionRate,
    float  AvgCompletionDays,
    float  ProductivityScore,
    float  ConsistencyScore,
    string GrowthTrend,
    List<string> RecommendedTraining);

public sealed record InternEvaluationResponse(
    Guid   InternId,
    string Name,
    string AttendanceRate,
    float  TaskCompletion,
    float  LearningProgress,
    string MentorFeedback,
    string AiRecommendation,
    float  AiConfidence);
