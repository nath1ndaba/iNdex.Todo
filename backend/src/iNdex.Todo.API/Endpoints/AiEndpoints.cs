using iNdex.Todo.API.Extensions;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Features.AI;
using iNdex.Todo.Contracts.AI;
using Microsoft.AspNetCore.Mvc;

namespace iNdex.Todo.API.Endpoints;

public static class AiEndpoints
{
    public static IEndpointRouteBuilder MapAiEndpoints(this IEndpointRouteBuilder app)
    {
        var ai = app.MapGroup("/api/ai")
            .WithTags("AI")
            .WithOpenApi()
            .RequireAuthorization();

        // ── Sprint 1 — ML.NET Predictions ────────────────────────────────────

        ai.MapPost("/predict/priority", PredictPriority)
            .WithName("PredictPriority")
            .WithSummary("Predict ticket priority using ML.NET")
            .Produces<PriorityPredictionResponse>();

        ai.MapPost("/predict/assignee", SuggestAssignee)
            .WithName("SuggestAssignee")
            .WithSummary("Suggest best assignee for a ticket using ML.NET")
            .Produces<AssigneeSuggestionResponse>();

        ai.MapPost("/predict/completion", PredictCompletion)
            .WithName("PredictCompletion")
            .WithSummary("Predict completion time for a ticket")
            .Produces<CompletionPredictionResponse>();

        ai.MapPost("/predict/deadline-risk", PredictDeadlineRisk)
            .WithName("PredictDeadlineRisk")
            .WithSummary("Predict deadline miss risk for a ticket")
            .Produces<DeadlineRiskResponse>();

        // ── Sprint 3 — Gemini Language Features ──────────────────────────────

        ai.MapPost("/summarize", SummarizeTicket)
            .WithName("SummarizeTicket")
            .WithSummary("Summarize a ticket using Gemini AI")
            .Produces<TicketSummaryResponse>();

        ai.MapPost("/create-from-nl", CreateFromNaturalLanguage)
            .WithName("CreateFromNaturalLanguage")
            .WithSummary("Create a ticket from a natural language instruction")
            .Produces<NlTicketResponse>(StatusCodes.Status201Created);

        ai.MapPost("/suggest-category", SuggestCategory)
            .WithName("SuggestCategory")
            .WithSummary("Suggest a ticket category using Gemini AI")
            .Produces<CategorySuggestionResponse>();

        // ── Sprint 6 — AI Project Manager ────────────────────────────────────

        ai.MapGet("/executive-summary", GetExecutiveSummary)
            .WithName("GetExecutiveSummary")
            .WithSummary("Generate today's AI executive summary")
            .Produces<ExecutiveSummaryResponse>();

        ai.MapGet("/release-readiness", GetReleaseReadiness)
            .WithName("GetReleaseReadiness")
            .WithSummary("Assess current release readiness")
            .Produces<ReleaseReadinessResponse>();

        return app;
    }

    // ── Handlers ──────────────────────────────────────────────────────────────

    private static async Task<IResult> PredictPriority(
        [FromBody]     PredictPriorityRequest request,
        [FromServices] IHandler<PredictPriorityRequest, PriorityPredictionResponse> handler,
        CancellationToken ct)
        => (await handler.HandleAsync(request, ct)).ToHttpResult();

    private static async Task<IResult> SuggestAssignee(
        [FromBody]     SuggestAssigneeRequest request,
        [FromServices] IHandler<SuggestAssigneeRequest, AssigneeSuggestionResponse> handler,
        CancellationToken ct)
        => (await handler.HandleAsync(request, ct)).ToHttpResult();

    private static async Task<IResult> PredictCompletion(
        [FromBody]     PredictCompletionRequest request,
        [FromServices] IHandler<PredictCompletionRequest, CompletionPredictionResponse> handler,
        CancellationToken ct)
        => (await handler.HandleAsync(request, ct)).ToHttpResult();

    private static async Task<IResult> PredictDeadlineRisk(
        [FromBody]     PredictDeadlineRiskRequest request,
        [FromServices] IHandler<PredictDeadlineRiskRequest, DeadlineRiskResponse> handler,
        CancellationToken ct)
        => (await handler.HandleAsync(request, ct)).ToHttpResult();

    private static async Task<IResult> SummarizeTicket(
        [FromBody]     SummarizeTicketRequest request,
        [FromServices] IHandler<SummarizeTicketRequest, TicketSummaryResponse> handler,
        CancellationToken ct)
        => (await handler.HandleAsync(request, ct)).ToHttpResult();

    private static async Task<IResult> CreateFromNaturalLanguage(
        [FromBody]     NaturalLanguageTicketRequest request,
        [FromServices] IHandler<NaturalLanguageTicketRequest, NlTicketResponse> handler,
        CancellationToken ct)
        => (await handler.HandleAsync(request, ct)).ToHttpResult(StatusCodes.Status201Created);

    private static async Task<IResult> SuggestCategory(
        [FromBody]     SuggestCategoryRequest request,
        [FromServices] IHandler<SuggestCategoryRequest, CategorySuggestionResponse> handler,
        CancellationToken ct)
        => (await handler.HandleAsync(request, ct)).ToHttpResult();

    private static async Task<IResult> GetExecutiveSummary(
        [FromServices] IHandler<GenerateExecutiveSummaryRequest, ExecutiveSummaryResponse> handler,
        CancellationToken ct)
        => (await handler.HandleAsync(new GenerateExecutiveSummaryRequest(null), ct)).ToHttpResult();

    private static async Task<IResult> GetReleaseReadiness(
        [FromServices] IHandler<ReleaseReadinessRequest, ReleaseReadinessResponse> handler,
        CancellationToken ct)
        => (await handler.HandleAsync(new ReleaseReadinessRequest(), ct)).ToHttpResult();
}
