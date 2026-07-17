using iNdex.Todo.AI.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

namespace iNdex.Todo.AI.Services;

public sealed class MlPredictionService
{
    private readonly MLContext _ml = new(seed: 42);
    private readonly ILogger<MlPredictionService> _log;
    private readonly AiSettings _settings;

    private PredictionEngine<PriorityTrainingData, PriorityPrediction>?         _priorityEngine;
    private PredictionEngine<AssigneeFeatureData,  AssigneePrediction>?         _assigneeEngine;
    private PredictionEngine<CompletionTimeData,   CompletionTimePrediction>?   _completionEngine;
    private PredictionEngine<DeadlineRiskData,     DeadlineRiskPrediction>?     _riskEngine;

    private static readonly string[] PriorityLabels = ["None", "Low", "Medium", "High", "Critical"];
    private static readonly string[] RiskLabels     = ["Low", "Medium", "High"];

    public MlPredictionService(ILogger<MlPredictionService> log, IOptions<AiSettings> settings)
    {
        _log      = log;
        _settings = settings.Value;
        LoadModels();
    }

    private void LoadModels()
    {
        TryLoad(_settings.PriorityModelPath,    schema => _priorityEngine    = CreateEngine<PriorityTrainingData, PriorityPrediction>(schema));
        TryLoad(_settings.AssigneeModelPath,    schema => _assigneeEngine    = CreateEngine<AssigneeFeatureData,  AssigneePrediction>(schema));
        TryLoad(_settings.CompletionModelPath,  schema => _completionEngine  = CreateEngine<CompletionTimeData,   CompletionTimePrediction>(schema));
        TryLoad(_settings.DeadlineRiskModelPath,schema => _riskEngine        = CreateEngine<DeadlineRiskData,     DeadlineRiskPrediction>(schema));
    }

    private void TryLoad(string path, Action<DataViewSchema> onLoaded)
    {
        if (!File.Exists(path)) { _log.LogWarning("ML model not found at {Path} — will use fallback.", path); return; }
        try
        {
            _ml.Model.Load(path, out var schema);
            onLoaded(schema);
            _log.LogInformation("Loaded ML model from {Path}", path);
        }
        catch (Exception ex) { _log.LogError(ex, "Failed to load ML model from {Path}", path); }
    }

    private PredictionEngine<TInput, TOutput> CreateEngine<TInput, TOutput>(DataViewSchema schema)
        where TInput  : class, new()
        where TOutput : class, new()
    {
        var model = _ml.Model.Load(_settings.PriorityModelPath, out _);
        return _ml.Model.CreatePredictionEngine<TInput, TOutput>(model);
    }

    // ── Feature 1: Predict Priority ──────────────────────────────────────────

    public (string Priority, float Confidence) PredictPriority(
        string title, string description, string category, string ticketType)
    {
        if (_priorityEngine is null) return FallbackPriority(title);

        var pred = _priorityEngine.Predict(new PriorityTrainingData
        {
            Title       = title,
            Description = description ?? string.Empty,
            Category    = category    ?? string.Empty,
            TicketType  = ticketType  ?? string.Empty
        });

        var label      = PriorityLabels[Math.Min((int)pred.PredictedPriority, PriorityLabels.Length - 1)];
        var confidence = pred.Scores?.Length > pred.PredictedPriority
            ? pred.Scores[pred.PredictedPriority]
            : 0.5f;

        return (label, confidence);
    }

    // ── Feature 2: Score an assignee candidate ───────────────────────────────

    public float ScoreAssignee(
        string ticketCategory, string ticketType, string ticketPriority,
        string userSkills, float userWorkload, float userSuccessRate)
    {
        if (_assigneeEngine is null) return 0.5f;

        var pred = _assigneeEngine.Predict(new AssigneeFeatureData
        {
            TicketCategory  = ticketCategory,
            TicketType      = ticketType,
            TicketPriority  = ticketPriority,
            UserSkills      = userSkills,
            UserWorkload    = userWorkload,
            UserSuccessRate = userSuccessRate
        });

        return Math.Clamp(pred.Score, 0f, 1f);
    }

    // ── Feature 3: Predict completion time ───────────────────────────────────

    public float PredictCompletionDays(
        string ticketType, string priority, string category,
        float titleLength, float descLength, float userAvgDays)
    {
        if (_completionEngine is null) return EstimateFallbackDays(priority);

        var pred = _completionEngine.Predict(new CompletionTimeData
        {
            TicketType  = ticketType,
            Priority    = priority,
            Category    = category,
            TitleLength = titleLength,
            DescLength  = descLength,
            UserAvgDays = userAvgDays
        });

        return Math.Max(0.5f, pred.PredictedDays);
    }

    // ── Feature 4: Predict deadline risk ─────────────────────────────────────

    public (string Risk, float Probability) PredictDeadlineRisk(
        float daysUntilDue, float progressPercent, float assigneeWorkload,
        float assigneeOnTimeRate, float estimatedDays, string priority)
    {
        if (_riskEngine is null) return FallbackRisk(daysUntilDue, progressPercent);

        var pred = _riskEngine.Predict(new DeadlineRiskData
        {
            DaysUntilDue       = daysUntilDue,
            ProgressPercent    = progressPercent,
            AssigneeWorkload   = assigneeWorkload,
            AssigneeOnTimeRate = assigneeOnTimeRate,
            EstimatedDays      = estimatedDays,
            Priority           = priority
        });

        var prob = pred.Probability;
        var risk = prob switch { > 0.70f => "High", > 0.40f => "Medium", _ => "Low" };
        return (risk, prob);
    }

    // ── Fallbacks (used when models aren't trained yet) ───────────────────────

    private static (string, float) FallbackPriority(string title)
    {
        var lower = title.ToLowerInvariant();
        if (lower.Contains("crash") || lower.Contains("payment") || lower.Contains("cannot login"))
            return ("Critical", 0.70f);
        if (lower.Contains("slow") || lower.Contains("error") || lower.Contains("fail"))
            return ("High", 0.65f);
        return ("Medium", 0.55f);
    }

    private static float EstimateFallbackDays(string priority) => priority switch
    {
        "Critical" => 1f,
        "High"     => 2f,
        "Medium"   => 4f,
        "Low"      => 7f,
        _          => 3f
    };

    private static (string, float) FallbackRisk(float daysLeft, float progress)
    {
        if (daysLeft < 1 && progress < 0.5f) return ("High", 0.85f);
        if (daysLeft < 3 && progress < 0.3f) return ("Medium", 0.55f);
        return ("Low", 0.20f);
    }
}
