namespace iNdex.Todo.AI.Services;

public sealed class AiSettings
{
    public const string SectionName = "AiSettings";

    // ML.NET model file paths (relative to ContentRootPath)
    public string PriorityModelPath     { get; init; } = "ml-models/priority.zip";
    public string AssigneeModelPath     { get; init; } = "ml-models/assignee.zip";
    public string CompletionModelPath   { get; init; } = "ml-models/completion.zip";
    public string DeadlineRiskModelPath { get; init; } = "ml-models/deadline-risk.zip";

    // Google Gemini
    public string GeminiApiKey   { get; init; } = string.Empty;
    public string GeminiModel    { get; init; } = "gemini-1.5-flash";  // free tier
    public int    GeminiMaxTokens { get; init; } = 1024;

    // Retraining schedule
    public bool   AutoRetrain        { get; init; } = true;
    public int    RetrainIntervalDays { get; init; } = 7;
    public int    MinTrainingSamples  { get; init; } = 50;  // don't train with less than this
}
