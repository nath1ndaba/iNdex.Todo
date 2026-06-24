using System.Text;
using System.Text.Json;
using iNdex.Todo.AI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mscc.GenerativeAI;

namespace iNdex.Todo.AI.Gemini;

public sealed class GeminiService(
    IOptions<AiSettings> settings,
    ILogger<GeminiService> logger)
{
    private readonly AiSettings _cfg = settings.Value;
    private GoogleAI? _client;

    private GenerativeModel GetModel()
    {
        _client ??= new GoogleAI(_cfg.GeminiApiKey);
        return _client.GenerativeModel(_cfg.GeminiModel);
    }

    // ── Feature 5: Ticket Summarization ──────────────────────────────────────

    public async Task<string> SummarizeTicketAsync(
        string title, string description, IEnumerable<string> comments,
        CancellationToken ct = default)
    {
        var commentBlock = string.Join("\n- ", comments);
        var prompt = $"""
            You are a project management assistant. Summarize the following ticket concisely.
            Return ONLY a bullet-point summary (3-5 bullets). No preamble.

            Ticket: {title}
            Description: {description}
            Comments:
            - {commentBlock}
            """;

        return await CallGeminiAsync(prompt, ct);
    }

    // ── Feature 6: Natural Language Ticket Creation ───────────────────────────

    public async Task<NlTicketResult> CreateTicketFromNaturalLanguageAsync(
        string userInput, IEnumerable<string> teamMemberNames,
        CancellationToken ct = default)
    {
        var names = string.Join(", ", teamMemberNames);
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var prompt = $"""
            You are a project management assistant. Extract ticket details from this instruction.
            Today is {today}. Known team members: {names}.

            Instruction: "{userInput}"

            Respond ONLY with valid JSON (no markdown, no explanation):
            {{
              "title": "...",
              "description": "...",
              "assigneeName": "..." or null,
              "priority": "None|Low|Medium|High|Critical",
              "type": "Task|Bug|Feature|Improvement|Research",
              "dueDate": "YYYY-MM-DD" or null,
              "category": "..."
            }}
            """;

        var raw = await CallGeminiAsync(prompt, ct);

        try
        {
            // Strip any accidental markdown fences
            var json = raw.Trim().TrimStart('`').TrimEnd('`');
            if (json.StartsWith("json", StringComparison.OrdinalIgnoreCase))
                json = json[4..].TrimStart();

            return JsonSerializer.Deserialize<NlTicketResult>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new NlTicketResult { Title = userInput };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to parse Gemini NL ticket JSON. Raw: {Raw}", raw);
            return new NlTicketResult { Title = userInput };
        }
    }

    // ── Feature 7: Category Suggestion ───────────────────────────────────────

    public async Task<(string Category, float Confidence)> SuggestCategoryAsync(
        string title, string description, CancellationToken ct = default)
    {
        var prompt = $"""
            You are a software project categorization assistant.
            Suggest ONE category for the following ticket and a confidence 0-100.

            Valid categories: Authentication, Payments, UI, Performance, API, Android, iOS,
            Database, Security, Notifications, Onboarding, Reporting, Infrastructure, Other

            Ticket: {title}
            Description: {description}

            Respond ONLY with JSON (no markdown):
            {{"category":"...", "confidence": 95}}
            """;

        var raw = await CallGeminiAsync(prompt, ct);
        try
        {
            var clean = raw.Trim().TrimStart('`').TrimEnd('`');
            if (clean.StartsWith("json", StringComparison.OrdinalIgnoreCase))
                clean = clean[4..].TrimStart();

            using var doc = JsonDocument.Parse(clean);
            var cat  = doc.RootElement.GetProperty("category").GetString() ?? "Other";
            var conf = doc.RootElement.TryGetProperty("confidence", out var c)
                ? c.GetSingle() / 100f
                : 0.75f;
            return (cat, Math.Clamp(conf, 0f, 1f));
        }
        catch
        {
            return ("Other", 0.5f);
        }
    }

    // ── Feature 10: Daily Executive Summary ──────────────────────────────────

    public async Task<string> GenerateExecutiveSummaryAsync(
        int openTickets, int criticalTickets, int completedThisWeek,
        IEnumerable<string> topRisks, IEnumerable<string> recommendedActions,
        float releaseReadiness,
        CancellationToken ct = default)
    {
        var risks   = string.Join("\n- ", topRisks);
        var actions = string.Join("\n- ", recommendedActions);

        var prompt = $"""
            You are an AI project manager generating a daily executive summary.
            Write a concise, professional summary (max 200 words) for the management team.

            Metrics:
            - Open tickets: {openTickets}
            - Critical tickets: {criticalTickets}
            - Completed this week: {completedThisWeek}
            - Release readiness: {releaseReadiness:P0}

            Top risks:
            - {risks}

            Recommended actions:
            - {actions}

            Write the summary now:
            """;

        return await CallGeminiAsync(prompt, ct);
    }

    // ── Feature 11: Release Readiness ────────────────────────────────────────

    public async Task<(float Score, string Analysis)> AnalyzeReleaseReadinessAsync(
        int openBugs, int criticalBugs, int pendingApprovals,
        int totalTickets, int completedTickets,
        CancellationToken ct = default)
    {
        var completionRate = totalTickets > 0
            ? (float)completedTickets / totalTickets
            : 0f;

        var prompt = $"""
            You are a release manager AI. Analyze release readiness.

            Data:
            - Open bugs: {openBugs}
            - Critical bugs: {criticalBugs}
            - Pending approvals: {pendingApprovals}
            - Ticket completion rate: {completionRate:P0}

            Respond ONLY with JSON:
            {{"score": 85, "analysis": "Brief analysis here (2-3 sentences max)"}}

            Score is 0-100. Be strict: any critical bug = score below 60.
            """;

        var raw = await CallGeminiAsync(prompt, ct);
        try
        {
            var clean = raw.Trim().TrimStart('`').TrimEnd('`');
            if (clean.StartsWith("json", StringComparison.OrdinalIgnoreCase))
                clean = clean[4..].TrimStart();

            using var doc = JsonDocument.Parse(clean);
            var score    = doc.RootElement.GetProperty("score").GetSingle();
            var analysis = doc.RootElement.GetProperty("analysis").GetString() ?? string.Empty;
            return (Math.Clamp(score, 0f, 100f), analysis);
        }
        catch
        {
            var fallback = criticalBugs > 0 ? 30f : completionRate * 100f;
            return (fallback, "Unable to generate detailed analysis.");
        }
    }

    // ── Internal Gemini call ──────────────────────────────────────────────────

    private async Task<string> CallGeminiAsync(string prompt, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(_cfg.GeminiApiKey))
        {
            logger.LogWarning("GeminiApiKey is not configured. Returning placeholder response.");
            return "[AI response unavailable — configure GeminiApiKey in AiSettings]";
        }

        try
        {
            var model    = GetModel();
            var response = await model.GenerateContentAsync(prompt);
            return response.Text?.Trim() ?? string.Empty;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Gemini API call failed");
            return $"[AI error: {ex.Message}]";
        }
    }
}

// ── Result types ──────────────────────────────────────────────────────────────

public sealed class NlTicketResult
{
    public string  Title        { get; set; } = string.Empty;
    public string? Description  { get; set; }
    public string? AssigneeName { get; set; }
    public string  Priority     { get; set; } = "Medium";
    public string  Type         { get; set; } = "Task";
    public string? DueDate      { get; set; }
    public string? Category     { get; set; }
}
