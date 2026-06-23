using FluentValidation;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Features.Auth.Login;
using iNdex.Todo.Application.Features.Auth.RefreshToken;
using iNdex.Todo.Application.Features.Tickets.CreateTicket;
using iNdex.Todo.Application.Features.Tickets.GetTickets;
using iNdex.Todo.Application.Features.TimeLog;
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
using iNdex.Todo.Application.Features.Users.GetAllUsers;
using iNdex.Todo.Application.Features.Users.GetUserById;
using iNdex.Todo.Contracts.Requests;
using iNdex.Todo.Contracts.Responses;
using Microsoft.Extensions.DependencyInjection;

namespace iNdex.Todo.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateTodoListValidator>();

        // Auth
        services.AddScoped<IHandler<RegisterUserRequest, AuthResponse>, RegisterHandler>();
        services.AddScoped<IHandler<LoginRequest, AuthResponse>, LoginHandler>();
        services.AddScoped<IHandler<RefreshTokenRequest, AuthResponse>, RefreshTokenHandler>();
        services.AddScoped<IHandler<RevokeTokenRequest, bool>, RevokeTokenHandler>();

        // TodoLists
        services.AddScoped<IHandler<CreateTodoListRequest, CreatedResponse>, CreateTodoListHandler>();
        services.AddScoped<IHandler<GetAllTodoListsQuery, List<TodoListResponse>>, GetAllTodoListsHandler>();
        services.AddScoped<IHandler<GetTodoListByIdQuery, TodoListResponse>, GetTodoListByIdHandler>();
        services.AddScoped<IHandler<UpdateTodoListCommand, TodoListResponse>, UpdateTodoListHandler>();
        services.AddScoped<IHandler<DeleteTodoListCommand, bool>, DeleteTodoListHandler>();

        // TodoTasks
        services.AddScoped<IHandler<CreateTodoTaskRequest, CreatedResponse>, CreateTodoTaskHandler>();
        services.AddScoped<IHandler<GetTasksByListQuery, List<TodoTaskResponse>>, GetTasksByListHandler>();
        services.AddScoped<IHandler<GetTodoTaskByIdQuery, TodoTaskResponse>, GetTodoTaskByIdHandler>();
        services.AddScoped<IHandler<UpdateTodoTaskCommand, TodoTaskResponse>, UpdateTodoTaskHandler>();
        services.AddScoped<IHandler<DeleteTodoTaskCommand, bool>, DeleteTodoTaskHandler>();
        services.AddScoped<IHandler<CompleteTaskCommand, TodoTaskResponse>, CompleteTaskHandler>();

        // Users
        services.AddScoped<IHandler<GetUserByIdQuery, UserResponse>, GetUserByIdHandler>();
        services.AddScoped<IHandler<GetAllUsersQuery, List<UserResponse>>, GetAllUsersHandler>();

        // Tickets
        services.AddScoped<IHandler<CreateTicketRequest, TicketResponse>, CreateTicketHandler>();
        services.AddScoped<IHandler<GetAllTicketsQuery, List<TicketResponse>>, GetAllTicketsHandler>();
        services.AddScoped<IHandler<GetTicketsByUserQuery, List<TicketResponse>>, GetTicketsByUserHandler>();
        services.AddScoped<IHandler<GetTicketByIdQuery, TicketResponse>, GetTicketByIdHandler>();
        services.AddScoped<IHandler<UpdateTicketCommand, TicketResponse>, UpdateTicketHandler>();
        services.AddScoped<IHandler<DeleteTicketCommand, bool>, DeleteTicketHandler>();

        // TimeLogs
        services.AddScoped<IHandler<LogTimeRequest, TimeLogResponse>, LogTimeHandler>();
        services.AddScoped<IHandler<GetTimeLogsByTicketQuery, TimeLogSummaryResponse>, GetTimeLogsByTicketHandler>();
        services.AddScoped<IHandler<GetTimeLogsByUserQuery, List<TimeLogResponse>>, GetTimeLogsByUserHandler>();
        services.AddScoped<IHandler<DeleteTimeLogCommand, bool>, DeleteTimeLogHandler>();

        return services;
    }
}
