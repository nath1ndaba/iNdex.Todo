using iNdex.Todo.Mobile.Models;
using Refit;

namespace iNdex.Todo.Mobile.Services;

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
    [Post("/api/users")]
    Task<ApiResponse<CreatedResponse>> RegisterUserAsync([Body] RegisterUserRequest request);

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
