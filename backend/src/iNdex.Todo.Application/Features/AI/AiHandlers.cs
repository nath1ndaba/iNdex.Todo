using iNdex.Todo.AI.Gemini;
using iNdex.Todo.AI.Services;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Common.Result;
using iNdex.Todo.Contracts.AI;
using iNdex.Todo.Domain.Errors;
using iNdex.Todo.Domain.Entities;
using System.Text.Json;

namespace iNdex.Todo.Application.Features.AI;

// ── Feature 1: Predict Priority ──────────────────────────────────────────────

public sealed class PredictPriorityHandler(MlPredictionService ml)
    : IHandler<PredictPriorityRequest, PriorityPredictionResponse>
{
    public Task<Result<PriorityPredictionResponse>> HandleAsync(
        PredictPriorityRequest r, CancellationToken ct = default)
    {
        var (priority, confidence) = ml.PredictPriority(
            r.Title, r.Description ?? string.Empty,
            r.Category ?? string.Empty, r.TicketType ?? string.Empty);

        return Task.FromResult(Result.Success(new PriorityPredictionResponse(
            priority,
            confidence,
            $"{confidence * 100:F0}%")));
    }
}

// ── Feature 2: Suggest Assignee ───────────────────────────────────────────────

public sealed class SuggestAssigneeHandler(
    MlPredictionService ml,
    ITicketRepository ticketRepo,
    IUserRepository userRepo)
    : IHandler<SuggestAssigneeRequest, AssigneeSuggestionResponse>
{
    public async Task<Result<AssigneeSuggestionResponse>> HandleAsync(
        SuggestAssigneeRequest r, CancellationToken ct = default)
    {
        var users = await userRepo.GetAllAsync(ct);

        // Build a scored list of all active users
        var suggestions = new List<AssigneeSuggestion>();
        foreach (var user in users.Where(u => u.IsActive && !u.IsDeleted))
        {
            var skills      = user.SkillProfile ?? string.Empty;
            var workload    = (await ticketRepo.GetByAssignedUserAsync(user.Id, ct))
                                .Count(t => t.Status is "Open" or "InProgress");
            var successRate = user.PerformanceRating.HasValue
                ? user.PerformanceRating.Value / 5f
                : 0.7f;

            var score = ml.ScoreAssignee(
                r.TicketCategory, r.TicketType, r.TicketPriority,
                skills, workload, successRate);

            suggestions.Add(new AssigneeSuggestion(
                user.Id,
                $"{user.FirstName} {user.LastName}",
                score,
                workload == 0 ? "Available" : $"{workload} open tickets"));
        }

        var ranked = suggestions
            .OrderByDescending(s => s.Score)
            .Take(3)
            .ToList();

        return Result.Success(new AssigneeSuggestionResponse(ranked));
    }
}

// ── Feature 3: Predict Completion Time ───────────────────────────────────────

public sealed class PredictCompletionHandler(
    MlPredictionService ml,
    ITicketRepository ticketRepo,
    ITimeLogRepository timeLogRepo)
    : IHandler<PredictCompletionRequest, CompletionPredictionResponse>
{
    public async Task<Result<CompletionPredictionResponse>> HandleAsync(
        PredictCompletionRequest r, CancellationToken ct = default)
    {
        var ticket = await ticketRepo.GetByIdAsync(r.TicketId, ct);
        if (ticket is null)
            return Result.Failure<CompletionPredictionResponse>(
                Error.NotFound(nameof(Ticket), r.TicketId));

        // Get assignee's historical average days (from their closed tickets)
        var userAvgDays = 3f;
        if (r.AssigneeId.HasValue)
        {
            var theirLogs = await timeLogRepo.GetByUserIdAsync(r.AssigneeId.Value, ct);
            if (theirLogs.Count > 0)
                userAvgDays = (float)theirLogs.Average(l => (double)l.Hours) / 8f;
        }

        var days = ml.PredictCompletionDays(
            ticket.Type.ToString(),
            ticket.Priority.ToString(),
            ticket.AiCategory ?? string.Empty,
            ticket.Title.Length,
            ticket.Description?.Length ?? 0,
            userAvgDays);

        var label = days < 1 ? "Less than 1 day"
                  : days < 2 ? $"~{days:F1} day"
                  : $"~{days:F1} days";

        return Result.Success(new CompletionPredictionResponse(days, label));
    }
}

// ── Feature 4: Predict Deadline Risk ─────────────────────────────────────────

public sealed class PredictDeadlineRiskHandler(
    MlPredictionService ml,
    ITicketRepository ticketRepo,
    ITimeLogRepository timeLogRepo,
    IUserRepository userRepo)
    : IHandler<PredictDeadlineRiskRequest, DeadlineRiskResponse>
{
    public async Task<Result<DeadlineRiskResponse>> HandleAsync(
        PredictDeadlineRiskRequest r, CancellationToken ct = default)
    {
        var ticket = await ticketRepo.GetByIdAsync(r.TicketId, ct);
        if (ticket is null)
            return Result.Failure<DeadlineRiskResponse>(
                Error.NotFound(nameof(Ticket), r.TicketId));

        var daysLeft = ticket.DueDate.HasValue
            ? (float)(ticket.DueDate.Value - DateTime.UtcNow).TotalDays
            : 999f;

        var logged   = ticket.TimeLogs.Sum(l => l.Hours);
        var progress = ticket.EstimatedHours > 0
            ? Math.Min(1f, (float)logged / ticket.EstimatedHours)
            : 0.5f;

        var workload = 0f;
        var onTime   = 0.75f;

        if (ticket.AssignedToUserId.HasValue)
        {
            var open = await ticketRepo.GetByAssignedUserAsync(ticket.AssignedToUserId.Value, ct);
            workload = open.Count;
            var user = await userRepo.GetByIdAsync(ticket.AssignedToUserId.Value, ct);
            if (user?.PerformanceRating.HasValue == true)
                onTime = user.PerformanceRating.Value / 5f;
        }

        var (risk, probability) = ml.PredictDeadlineRisk(
            daysLeft, progress, workload, onTime,
            ticket.EstimatedHours, ticket.Priority.ToString());

        var explanation = risk switch
        {
            "High"   => $"Only {daysLeft:F0} days left with {progress:P0} progress and high assignee workload.",
            "Medium" => $"Moderate risk — {daysLeft:F0} days remaining, {progress:P0} complete.",
            _        => $"On track — {daysLeft:F0} days remaining, {progress:P0} complete."
        };

        // Persist risk back to ticket
        ticket.AiDeadlineRisk = risk;
        await ticketRepo.UpdateAsync(ticket, ct);

        return Result.Success(new DeadlineRiskResponse(risk, probability, explanation));
    }
}

// ── Feature 5: Summarize Ticket ───────────────────────────────────────────────

public sealed class SummarizeTicketHandler(
    GeminiService gemini,
    ITicketRepository ticketRepo,
    IUnitOfWork uow)
    : IHandler<SummarizeTicketRequest, TicketSummaryResponse>
{
    public async Task<Result<TicketSummaryResponse>> HandleAsync(
        SummarizeTicketRequest r, CancellationToken ct = default)
    {
        var ticket = await ticketRepo.GetByIdAsync(r.TicketId, ct);
        if (ticket is null)
            return Result.Failure<TicketSummaryResponse>(
                Error.NotFound(nameof(Ticket), r.TicketId));

        var comments = ticket.Comments.Select(c => c.Comment);
        var summary  = await gemini.SummarizeTicketAsync(
            ticket.Title, ticket.Description ?? string.Empty, comments, ct);

        ticket.AiSummary            = summary;
        ticket.AiSummaryGeneratedAt = DateTime.UtcNow;
        await ticketRepo.UpdateAsync(ticket, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(new TicketSummaryResponse(
            ticket.Id, summary, DateTime.UtcNow));
    }
}

// ── Feature 6: Natural Language Ticket Creation ───────────────────────────────

public sealed class NaturalLanguageTicketHandler(
    GeminiService gemini,
    IUserRepository userRepo,
    ITicketRepository ticketRepo,
    IUnitOfWork uow)
    : IHandler<NaturalLanguageTicketRequest, NlTicketResponse>
{
    public async Task<Result<NlTicketResponse>> HandleAsync(
        NaturalLanguageTicketRequest r, CancellationToken ct = default)
    {
        var users   = await userRepo.GetAllAsync(ct);
        var names   = users.Select(u => $"{u.FirstName} {u.LastName}");
        var nlResult = await gemini.CreateTicketFromNaturalLanguageAsync(r.Instruction, names, ct);

        // Try to resolve assignee name to a user ID
        Guid? assigneeId = null;
        if (!string.IsNullOrEmpty(nlResult.AssigneeName))
        {
            var match = users.FirstOrDefault(u =>
                $"{u.FirstName} {u.LastName}".Contains(nlResult.AssigneeName,
                    StringComparison.OrdinalIgnoreCase));
            assigneeId = match?.Id;
        }

        // Auto-create the ticket
        var number = await ticketRepo.GenerateTicketNumberAsync(ct);
        var ticket = new Ticket
        {
            Title            = nlResult.Title,
            Description      = nlResult.Description,
            TicketNumber     = number,
            AiCategory       = nlResult.Category,
            AssignedToUserId = assigneeId,
            CreatedByUserId  = r.CreatedByUserId,
            CreatedBy        = r.CreatedByUserId.ToString(),
            DueDate          = DateTime.TryParse(nlResult.DueDate, out var d)
                               ? d.ToUniversalTime()
                               : null,
        };

        await ticketRepo.AddAsync(ticket, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(new NlTicketResponse(
            nlResult.Title, nlResult.Description, nlResult.AssigneeName,
            assigneeId, nlResult.Priority, nlResult.Type,
            nlResult.DueDate, nlResult.Category));
    }
}

// ── Feature 7: Category Suggestion ───────────────────────────────────────────

public sealed class SuggestCategoryHandler(GeminiService gemini)
    : IHandler<SuggestCategoryRequest, CategorySuggestionResponse>
{
    public async Task<Result<CategorySuggestionResponse>> HandleAsync(
        SuggestCategoryRequest r, CancellationToken ct = default)
    {
        var (category, confidence) = await gemini.SuggestCategoryAsync(
            r.Title, r.Description, ct);

        return Result.Success(new CategorySuggestionResponse(
            category, confidence, $"{confidence * 100:F0}%"));
    }
}

// ── Feature 10: Executive Summary ─────────────────────────────────────────────

public sealed class ExecutiveSummaryHandler(
    GeminiService gemini,
    ITicketRepository ticketRepo)
    : IHandler<GenerateExecutiveSummaryRequest, ExecutiveSummaryResponse>
{
    public async Task<Result<ExecutiveSummaryResponse>> HandleAsync(
        GenerateExecutiveSummaryRequest r, CancellationToken ct = default)
    {
        var allTickets  = await ticketRepo.GetAllWithDetailsAsync(ct);
        var open        = allTickets.Count(t => t.Status is not (Domain.Enums.TicketStatus.Done or Domain.Enums.TicketStatus.Cancelled));
        var critical    = allTickets.Count(t => t.Priority == Domain.Enums.TaskPriority.Critical &&
                                                t.Status != Domain.Enums.TicketStatus.Done);
        var weekStart   = DateTime.UtcNow.AddDays(-7);
        var completedWk = allTickets.Count(t => t.UpdatedAt >= weekStart &&
                                                t.Status == Domain.Enums.TicketStatus.Done);

        var risks   = allTickets
            .Where(t => t.AiDeadlineRisk == "High" && t.Status != Domain.Enums.TicketStatus.Done)
            .Take(5)
            .Select(t => $"{t.TicketNumber}: {t.Title}")
            .ToList();

        var actions = risks.Select(risk => $"Review and reassign: {risk.Split(':')[0]}").ToList();
        if (critical > 0) actions.Insert(0, $"Address {critical} critical ticket(s) immediately");

        var readiness = open == 0 ? 100f : Math.Max(0f, 100f - (critical * 20f) - (open * 0.5f));

        var narrative = await gemini.GenerateExecutiveSummaryAsync(
            open, critical, completedWk, risks, actions, readiness / 100f, ct);

        return Result.Success(new ExecutiveSummaryResponse(
            r.Date ?? DateTime.UtcNow.Date,
            open, critical, completedWk,
            readiness, risks, actions, narrative));
    }
}

// ── Feature 11: Release Readiness ────────────────────────────────────────────

public sealed class ReleaseReadinessHandler(
    GeminiService gemini,
    ITicketRepository ticketRepo)
    : IHandler<ReleaseReadinessRequest, ReleaseReadinessResponse>
{
    public async Task<Result<ReleaseReadinessResponse>> HandleAsync(
        ReleaseReadinessRequest r, CancellationToken ct = default)
    {
        var all        = await ticketRepo.GetAllWithDetailsAsync(ct);
        var bugs       = all.Count(t => t.Type == Domain.Enums.TicketType.Bug &&
                                        t.Status != Domain.Enums.TicketStatus.Done);
        var critical   = all.Count(t => t.Priority == Domain.Enums.TaskPriority.Critical &&
                                        t.Status != Domain.Enums.TicketStatus.Done);
        var pending    = all.Count(t => t.Status == Domain.Enums.TicketStatus.InReview);
        var completed  = all.Count(t => t.Status == Domain.Enums.TicketStatus.Done);

        var (score, analysis) = await gemini.AnalyzeReleaseReadinessAsync(
            bugs, critical, pending, all.Count, completed, ct);

        var grade = score switch { >= 90 => "A", >= 75 => "B", >= 60 => "C", >= 40 => "D", _ => "F" };

        return Result.Success(new ReleaseReadinessResponse(
            score, grade, analysis, score >= 75 && critical == 0));
    }
}
