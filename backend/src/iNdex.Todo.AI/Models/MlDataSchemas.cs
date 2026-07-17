using Microsoft.ML.Data;

namespace iNdex.Todo.AI.Models;

// ── Feature 1: Priority Prediction ───────────────────────────────────────────

public sealed class PriorityTrainingData
{
    [LoadColumn(0)] public string Title       { get; set; } = string.Empty;
    [LoadColumn(1)] public string Description { get; set; } = string.Empty;
    [LoadColumn(2)] public string Category    { get; set; } = string.Empty;
    [LoadColumn(3)] public string TicketType  { get; set; } = string.Empty;
    // Label: 0=None,1=Low,2=Medium,3=High,4=Critical
    [LoadColumn(4)] [ColumnName("Label")] public uint Priority { get; set; }
}

public sealed class PriorityPrediction
{
    [ColumnName("PredictedLabel")]
    public uint PredictedPriority { get; set; }

    [ColumnName("Score")]
    public float[] Scores { get; set; } = [];
}

// ── Feature 2: Assignee Suggestion ───────────────────────────────────────────

public sealed class AssigneeTrainingData
{
    [LoadColumn(0)] public float UserId      { get; set; }  // encoded user index
    [LoadColumn(1)] public float TicketId    { get; set; }  // encoded ticket index
    // Label: was this a successful assignment? (1 = completed on time, 0 = not)
    [LoadColumn(2)] [ColumnName("Label")] public float Success { get; set; }
}

public sealed class AssigneeFeatureData
{
    public string TicketCategory { get; set; } = string.Empty;
    public string TicketType     { get; set; } = string.Empty;
    public string TicketPriority { get; set; } = string.Empty;
    public string UserSkills     { get; set; } = string.Empty;
    public float  UserWorkload   { get; set; }  // open tickets count
    public float  UserSuccessRate { get; set; } // historical 0-1
    [ColumnName("Label")] public float Label { get; set; }
}

public sealed class AssigneePrediction
{
    [ColumnName("Score")]
    public float Score { get; set; }  // probability 0-1
}

// ── Feature 3: Completion Time Prediction ────────────────────────────────────

public sealed class CompletionTimeData
{
    public string TicketType   { get; set; } = string.Empty;
    public string Priority     { get; set; } = string.Empty;
    public string Category     { get; set; } = string.Empty;
    public float  TitleLength  { get; set; }
    public float  DescLength   { get; set; }
    public float  UserAvgDays  { get; set; }  // assignee's historical avg
    // Label: actual days to complete
    [ColumnName("Label")] public float DaysToComplete { get; set; }
}

public sealed class CompletionTimePrediction
{
    [ColumnName("Score")]
    public float PredictedDays { get; set; }
}

// ── Feature 4: Deadline Risk Prediction ──────────────────────────────────────

public sealed class DeadlineRiskData
{
    public float DaysUntilDue      { get; set; }
    public float ProgressPercent   { get; set; }  // 0-1 (hours logged / estimated)
    public float AssigneeWorkload  { get; set; }  // open tickets count
    public float AssigneeOnTimeRate{ get; set; }  // historical 0-1
    public float EstimatedDays     { get; set; }
    public string Priority         { get; set; } = string.Empty;
    // Label: 1 = missed deadline, 0 = completed on time
    [ColumnName("Label")] public bool MissedDeadline { get; set; }
}

public sealed class DeadlineRiskPrediction
{
    [ColumnName("PredictedLabel")]
    public bool WillMiss { get; set; }

    [ColumnName("Probability")]
    public float Probability { get; set; }

    [ColumnName("Score")]
    public float Score { get; set; }
}
