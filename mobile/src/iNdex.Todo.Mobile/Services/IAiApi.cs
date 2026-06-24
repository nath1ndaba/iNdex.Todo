using Refit;

namespace iNdex.Todo.Mobile.Services;

public interface IAiApi
{
    [Post("/api/ai/predict/priority")]
    Task<ApiResponse<PriorityPredictionResponse>> PredictPriorityAsync([Body] PredictPriorityRequest request);

    [Post("/api/ai/predict/assignee")]
    Task<ApiResponse<AssigneeSuggestionResponse>> SuggestAssigneeAsync([Body] SuggestAssigneeRequest request);

    [Post("/api/ai/predict/completion")]
    Task<ApiResponse<CompletionPredictionResponse>> PredictCompletionAsync([Body] PredictCompletionRequest request);

    [Post("/api/ai/predict/deadline-risk")]
    Task<ApiResponse<DeadlineRiskResponse>> PredictDeadlineRiskAsync([Body] PredictDeadlineRiskRequest request);

    [Post("/api/ai/summarize")]
    Task<ApiResponse<TicketSummaryResponse>> SummarizeTicketAsync([Body] SummarizeTicketRequest request);

    [Post("/api/ai/create-from-nl")]
    Task<ApiResponse<NlTicketResponse>> CreateFromNaturalLanguageAsync([Body] NaturalLanguageTicketRequest request);

    [Post("/api/ai/suggest-category")]
    Task<ApiResponse<CategorySuggestionResponse>> SuggestCategoryAsync([Body] SuggestCategoryRequest request);

    [Get("/api/ai/executive-summary")]
    Task<ApiResponse<ExecutiveSummaryResponse>> GetExecutiveSummaryAsync();

    [Get("/api/ai/release-readiness")]
    Task<ApiResponse<ReleaseReadinessResponse>> GetReleaseReadinessAsync();
}
