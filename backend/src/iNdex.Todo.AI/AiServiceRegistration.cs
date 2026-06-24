using iNdex.Todo.AI.Gemini;
using iNdex.Todo.AI.Services;
using iNdex.Todo.AI.Training;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Features.AI;
using iNdex.Todo.Contracts.AI;
using Microsoft.Extensions.DependencyInjection;

namespace iNdex.Todo.AI;

public static class AiServiceRegistration
{
    public static IServiceCollection AddAiServices(
        this IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        // Settings
        services.Configure<AiSettings>(configuration.GetSection(AiSettings.SectionName));

        // Core AI services
        services.AddSingleton<MlPredictionService>();  // singleton — engines are expensive to load
        services.AddScoped<GeminiService>();
        services.AddScoped<ModelTrainer>();

        // ── Handler registrations ────────────────────────────────────────────

        // Sprint 1 — ML.NET
        services.AddScoped<IHandler<PredictPriorityRequest,    PriorityPredictionResponse>,   PredictPriorityHandler>();
        services.AddScoped<IHandler<SuggestAssigneeRequest,    AssigneeSuggestionResponse>,   SuggestAssigneeHandler>();
        services.AddScoped<IHandler<PredictCompletionRequest,  CompletionPredictionResponse>, PredictCompletionHandler>();
        services.AddScoped<IHandler<PredictDeadlineRiskRequest,DeadlineRiskResponse>,         PredictDeadlineRiskHandler>();

        // Sprint 3 — Gemini
        services.AddScoped<IHandler<SummarizeTicketRequest,          TicketSummaryResponse>,      SummarizeTicketHandler>();
        services.AddScoped<IHandler<NaturalLanguageTicketRequest,    NlTicketResponse>,            NaturalLanguageTicketHandler>();
        services.AddScoped<IHandler<SuggestCategoryRequest,          CategorySuggestionResponse>,  SuggestCategoryHandler>();

        // Sprint 6 — AI Project Manager
        services.AddScoped<IHandler<GenerateExecutiveSummaryRequest, ExecutiveSummaryResponse>, ExecutiveSummaryHandler>();
        services.AddScoped<IHandler<ReleaseReadinessRequest,         ReleaseReadinessResponse>,  ReleaseReadinessHandler>();

        return services;
    }
}
