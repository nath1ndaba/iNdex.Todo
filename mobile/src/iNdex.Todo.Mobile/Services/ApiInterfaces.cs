using iNdex.Todo.Mobile.Models;
using Refit;

namespace iNdex.Todo.Mobile.Services;

// ── Ticket API ────────────────────────────────────────────────────────────────
public interface ITicketApi
{
    [Post("/api/tickets")]
    Task<ApiResponse<TicketResponse>> CreateTicketAsync([Body] CreateTicketRequest request);

    [Get("/api/tickets")]
    Task<ApiResponse<List<TicketResponse>>> GetAllTicketsAsync();

    [Get("/api/tickets/assigned/{userId}")]
    Task<ApiResponse<List<TicketResponse>>> GetTicketsByUserAsync(Guid userId);

    [Get("/api/tickets/{id}")]
    Task<ApiResponse<TicketResponse>> GetTicketByIdAsync(Guid id);

    [Put("/api/tickets/{id}")]
    Task<ApiResponse<TicketResponse>> UpdateTicketAsync(Guid id, [Body] UpdateTicketRequest request);

    [Delete("/api/tickets/{id}")]
    Task<IApiResponse> DeleteTicketAsync(Guid id);
}

// ── TimeLog API ────────────────────────────────────────────────────────────────
public interface ITimeLogApi
{
    [Post("/api/timelogs")]
    Task<ApiResponse<TimeLogResponse>> LogTimeAsync([Body] LogTimeRequest request);

    [Get("/api/timelogs/ticket/{ticketId}")]
    Task<ApiResponse<TimeLogSummaryResponse>> GetTimeLogsByTicketAsync(Guid ticketId);

    [Get("/api/timelogs/user/{userId}")]
    Task<ApiResponse<List<TimeLogResponse>>> GetTimeLogsByUserAsync(Guid userId);

    [Delete("/api/timelogs/{id}")]
    Task<IApiResponse> DeleteTimeLogAsync(Guid id);
}

// ── Auth API ──────────────────────────────────────────────────────────────────
public interface IAuthApi
{
    [Post("/api/auth/register")]
    Task<ApiResponse<AuthResponse>> RegisterAsync([Body] RegisterUserRequest request);

    [Post("/api/auth/login")]
    Task<ApiResponse<AuthResponse>> LoginAsync([Body] LoginRequest request);

    [Post("/api/auth/refresh")]
    Task<ApiResponse<AuthResponse>> RefreshAsync([Body] RefreshTokenRequest request);

    [Post("/api/auth/revoke")]
    Task<IApiResponse> RevokeAsync([Body] RevokeTokenRequest request);

    [Get("/api/auth/me")]
    Task<ApiResponse<UserResponse>> GetMeAsync();
}

// ── Users API ─────────────────────────────────────────────────────────────────
public interface IUserApi
{
    [Get("/api/users")]
    Task<ApiResponse<List<UserResponse>>> GetAllUsersAsync();

    [Get("/api/users/{id}")]
    Task<ApiResponse<UserResponse>> GetUserByIdAsync(Guid id);
}

// ── TodoLists API ─────────────────────────────────────────────────────────────
public interface ITodoListApi
{
    [Post("/api/lists")]
    Task<ApiResponse<CreatedResponse>> CreateTodoListAsync([Body] CreateTodoListRequest request);

    [Get("/api/lists")]
    Task<ApiResponse<List<TodoListResponse>>> GetAllTodoListsAsync([Query] Guid ownerId);

    [Get("/api/lists/{id}")]
    Task<ApiResponse<TodoListResponse>> GetTodoListByIdAsync(Guid id);

    [Put("/api/lists/{id}")]
    Task<ApiResponse<TodoListResponse>> UpdateTodoListAsync(Guid id, [Body] UpdateTodoListRequest request);

    [Delete("/api/lists/{id}")]
    Task<IApiResponse> DeleteTodoListAsync(Guid id);
}

// ── TodoTasks API ─────────────────────────────────────────────────────────────
public interface ITodoTaskApi
{
    [Post("/api/tasks")]
    Task<ApiResponse<CreatedResponse>> CreateTodoTaskAsync([Body] CreateTodoTaskRequest request);

    [Get("/api/tasks/by-list/{listId}")]
    Task<ApiResponse<List<TodoTaskResponse>>> GetTasksByListAsync(Guid listId);

    [Get("/api/tasks/{id}")]
    Task<ApiResponse<TodoTaskResponse>> GetTodoTaskByIdAsync(Guid id);

    [Put("/api/tasks/{id}")]
    Task<ApiResponse<TodoTaskResponse>> UpdateTodoTaskAsync(Guid id, [Body] UpdateTodoTaskRequest request);

    [Patch("/api/tasks/{id}/complete")]
    Task<ApiResponse<TodoTaskResponse>> CompleteTaskAsync(Guid id, [Body] CompleteTaskRequest request);

    [Delete("/api/tasks/{id}")]
    Task<IApiResponse> DeleteTodoTaskAsync(Guid id);
}
