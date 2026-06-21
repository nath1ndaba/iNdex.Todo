using FluentValidation;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Features.Auth.Login;
using iNdex.Todo.Application.Features.Auth.RefreshToken;
using iNdex.Todo.Application.Features.TodoLists.CreateTodoList;
using iNdex.Todo.Application.Features.TodoLists.DeleteTodoList;
using iNdex.Todo.Application.Features.TodoLists.GetAllTodoLists;
using iNdex.Todo.Application.Features.TodoLists.GetTodoListById;
using iNdex.Todo.Application.Features.TodoLists.UpdateTodoList;
using iNdex.Todo.Application.Features.TodoTasks.CompleteTask;
using iNdex.Todo.Application.Features.TodoTasks.CreateTodoTask;
using iNdex.Todo.Application.Features.TodoTasks.DeleteTodoTask;
using iNdex.Todo.Application.Features.TodoTasks.GetTasksByList;
using iNdex.Todo.Application.Features.TodoTasks.GetTodoTaskById;
using iNdex.Todo.Application.Features.TodoTasks.UpdateTodoTask;
using iNdex.Todo.Application.Features.Users.GetUserById;
using iNdex.Todo.Contracts.Requests;
using iNdex.Todo.Contracts.Responses;
using Microsoft.Extensions.DependencyInjection;

namespace iNdex.Todo.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Validators (auto-discovered from assembly)
        services.AddValidatorsFromAssemblyContaining<CreateTodoListValidator>();

        // ── Auth Handlers ──────────────────────────────────────────────────────
        services.AddScoped<IHandler<RegisterUserRequest, AuthResponse>, RegisterHandler>();
        services.AddScoped<IHandler<LoginRequest, AuthResponse>, LoginHandler>();
        services.AddScoped<IHandler<RefreshTokenRequest, AuthResponse>, RefreshTokenHandler>();
        services.AddScoped<IHandler<RevokeTokenRequest, bool>, RevokeTokenHandler>();

        // ── TodoList Handlers ──────────────────────────────────────────────────
        services.AddScoped<IHandler<CreateTodoListRequest, CreatedResponse>, CreateTodoListHandler>();
        services.AddScoped<IHandler<GetAllTodoListsQuery, List<TodoListResponse>>, GetAllTodoListsHandler>();
        services.AddScoped<IHandler<GetTodoListByIdQuery, TodoListResponse>, GetTodoListByIdHandler>();
        services.AddScoped<IHandler<UpdateTodoListCommand, TodoListResponse>, UpdateTodoListHandler>();
        services.AddScoped<IHandler<DeleteTodoListCommand, bool>, DeleteTodoListHandler>();

        // ── TodoTask Handlers ──────────────────────────────────────────────────
        services.AddScoped<IHandler<CreateTodoTaskRequest, CreatedResponse>, CreateTodoTaskHandler>();
        services.AddScoped<IHandler<GetTasksByListQuery, List<TodoTaskResponse>>, GetTasksByListHandler>();
        services.AddScoped<IHandler<GetTodoTaskByIdQuery, TodoTaskResponse>, GetTodoTaskByIdHandler>();
        services.AddScoped<IHandler<UpdateTodoTaskCommand, TodoTaskResponse>, UpdateTodoTaskHandler>();
        services.AddScoped<IHandler<DeleteTodoTaskCommand, bool>, DeleteTodoTaskHandler>();
        services.AddScoped<IHandler<CompleteTaskCommand, TodoTaskResponse>, CompleteTaskHandler>();

        // ── User Handlers ──────────────────────────────────────────────────────
        services.AddScoped<IHandler<GetUserByIdQuery, UserResponse>, GetUserByIdHandler>();

        return services;
    }
}
