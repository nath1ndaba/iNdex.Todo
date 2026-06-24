using iNdex.Todo.AI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace iNdex.Todo.AI.Gemini;

public sealed class GeminiService(
    IOptions<AiSettings> settings,
    ILogger<GeminiService> logger)
{
    private readonly AiSettings _cfg = settings.Value;
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
      string userInput,
      IEnumerable<string> teamMemberNames,
      CancellationToken ct = default)
    {
        var names = string.Join(", ", teamMemberNames);
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var prompt = $$"""
You are a project management assistant.

Today's date is {{today}}.

Known team members:
{{names}}

Instruction:
"{{userInput}}"

Respond ONLY with valid JSON.

{
  "title": "",
  "description": "",
  "assigneeName": null,
  "priority": "None",
  "type": "Task",
  "dueDate": null,
  "category": ""
}
""";

        var raw = await CallGeminiAsync(prompt, ct);

        try
        {
            var json = raw.Trim();

            if (json.StartsWith("```"))
            {
                json = json.Replace("```json", "")
                           .Replace("```", "")
                           .Trim();
            }

            return JsonSerializer.Deserialize<NlTicketResult>(
                       json,
                       new JsonSerializerOptions
                       {
                           PropertyNameCaseInsensitive = true
                       })
                   ?? new NlTicketResult
                   {
                       Title = userInput
                   };
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Failed to parse Gemini ticket response. Raw response: {Raw}",
                raw);

            return new NlTicketResult
            {
                Title = userInput
            };
        }
    }

    // ── Feature 7: Category Suggestion ───────────────────────────────────────

    public async Task<(string Category, float Confidence)> SuggestCategoryAsync(
     string title,
     string description,
     CancellationToken ct = default)
    {
        var prompt = $$"""
You are a software ticket categorization assistant.

Choose ONE category from:

Authentication
Payments
UI
Performance
API
Android
iOS
Database
Security
Notifications
Onboarding
Reporting
Infrastructure
Other

Ticket Title:
{{title}}

Ticket Description:
{{description}}

Respond ONLY with JSON.

{
  "category": "",
  "confidence": 95
}
""";

        var raw = await CallGeminiAsync(prompt, ct);

        try
        {
            var json = raw.Trim();

            if (json.StartsWith("```"))
            {
                json = json.Replace("```json", "")
                           .Replace("```", "")
                           .Trim();
            }

            using var doc = JsonDocument.Parse(json);

            var category =
                doc.RootElement.GetProperty("category").GetString()
                ?? "Other";

            var confidence =
                doc.RootElement.TryGetProperty("confidence", out var c)
                    ? c.GetSingle() / 100f
                    : 0.75f;

            return (
                category,
                Math.Clamp(confidence, 0f, 1f));
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Failed to parse category response. Raw response: {Raw}",
                raw);

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
        var risks = string.Join("\n- ", topRisks);
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
    int openBugs,
    int criticalBugs,
    int pendingApprovals,
    int totalTickets,
    int completedTickets,
    CancellationToken ct = default)
    {
        var completionRate =
            totalTickets > 0
                ? (float)completedTickets / totalTickets
                : 0f;

        var prompt = $$"""
You are an experienced software release manager.

Analyze release readiness using:

Open Bugs: {{openBugs}}
Critical Bugs: {{criticalBugs}}
Pending Approvals: {{pendingApprovals}}
Completion Rate: {{completionRate:P0}}

Respond ONLY with JSON.

{
  "score": 85,
  "analysis": ""
}

Rules:
- Score between 0 and 100.
- If critical bugs exist, score should generally be below 60.
- Analysis should be 2-3 concise sentences.
""";

        var raw = await CallGeminiAsync(prompt, ct);

        try
        {
            var json = raw.Trim();

            if (json.StartsWith("```"))
            {
                json = json.Replace("```json", "")
                           .Replace("```", "")
                           .Trim();
            }

            using var doc = JsonDocument.Parse(json);

            var score =
                doc.RootElement.GetProperty("score").GetSingle();

            var analysis =
                doc.RootElement.GetProperty("analysis").GetString()
                ?? string.Empty;

            return (
                Math.Clamp(score, 0f, 100f),
                analysis);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Failed to parse release readiness response. Raw response: {Raw}",
                raw);

            var fallbackScore =
                criticalBugs > 0
                    ? 30f
                    : completionRate * 100f;

            return (
                fallbackScore,
                "Unable to generate detailed release readiness analysis.");
        }
    }
    // ── Internal Gemini call ──────────────────────────────────────────────────

    private async Task<string> CallGeminiAsync(string prompt, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_cfg.GeminiApiKey))
        {
            logger.LogWarning("GeminiApiKey is not configured.");
            return "[AI response unavailable]";
        }

        try
        {
            using var client = new HttpClient();

            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/{_cfg.GeminiModel}:generateContent?key={_cfg.GeminiApiKey}";

            var body = new
            {
                contents = new[]
                {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = prompt
                        }
                    }
                }
            }
            };

            var response = await client.PostAsJsonAsync(url, body, ct);

            var json = await response.Content.ReadAsStringAsync(ct);

            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(json);

            return doc
                .RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString()
                ?.Trim() ?? string.Empty;
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
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AssigneeName { get; set; }
    public string Priority { get; set; } = "Medium";
    public string Type { get; set; } = "Task";
    public string? DueDate { get; set; }
    public string? Category { get; set; }
}
