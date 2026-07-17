using iNdex.Todo.Domain.Common;

namespace iNdex.Todo.Domain.Entities;

/// <summary>
/// Snapshot of an employee's performance metrics for a given period.
/// Computed by the AI layer and stored for trending.
/// </summary>
public sealed class EmployeePerformance : BaseEntity
{
    public Guid   UserId              { get; set; }
    public string PeriodLabel         { get; set; } = string.Empty; // "2025-W23", "2025-06"
    public int    TasksAssigned       { get; set; }
    public int    TasksCompleted      { get; set; }
    public float  CompletionRate      { get; set; }   // 0-1
    public float  AvgCompletionDays   { get; set; }
    public int    TicketsClosed       { get; set; }
    public float  QualityScore        { get; set; }   // 0-100
    public float  ProductivityScore   { get; set; }   // 0-100 (AI computed)
    public float  ConsistencyScore    { get; set; }   // 0-100
    public string GrowthTrend         { get; set; } = string.Empty; // "Improving","Stable","Declining"
    public string RecommendedTraining { get; set; } = string.Empty; // JSON array
    public DateTime ComputedAt        { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}

/// <summary>
/// Periodic intern evaluation record, reviewed by a mentor.
/// </summary>
public sealed class InternEvaluation : BaseEntity
{
    public Guid    InternId         { get; set; }
    public Guid?   MentorId         { get; set; }
    public string  PeriodLabel      { get; set; } = string.Empty;
    public int     AttendanceDays   { get; set; }
    public int     TotalDays        { get; set; }
    public float   TaskCompletion   { get; set; }   // 0-1
    public float   LearningProgress { get; set; }   // 0-100
    public string? MentorFeedback   { get; set; }
    public string  AiRecommendation { get; set; } = string.Empty; // "Promote","Extend","Training"
    public float   AiConfidence     { get; set; }
    public DateTime EvaluatedAt     { get; set; } = DateTime.UtcNow;

    public User  Intern  { get; set; } = null!;
    public User? Mentor  { get; set; }
}

/// <summary>
/// Daily AI-generated executive summary snapshot.
/// </summary>
public sealed class ExecutiveSummary : BaseEntity
{
    public DateTime Date              { get; set; } = DateTime.UtcNow.Date;
    public int      OpenTickets       { get; set; }
    public int      CriticalTickets   { get; set; }
    public int      CompletedThisWeek { get; set; }
    public string   TopRisks          { get; set; } = string.Empty;  // JSON array
    public string   RecommendedActions { get; set; } = string.Empty; // JSON array
    public float    ReleaseReadiness   { get; set; }  // 0-100
    public string   FullSummary        { get; set; } = string.Empty; // Gemini narrative
}
