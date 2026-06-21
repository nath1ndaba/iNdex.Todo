using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Infrastructure.Auth;
using iNdex.Todo.Infrastructure.Persistence;
using iNdex.Todo.Infrastructure.Persistence.Interceptors;
using iNdex.Todo.Infrastructure.Persistence.Repositories;
using iNdex.Todo.Infrastructure.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace iNdex.Todo.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Interceptors
        services.AddSingleton<AuditInterceptor>();

        // EF Core + PostgreSQL
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));

            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITodoListRepository, TodoListRepository>();
        services.AddScoped<ITodoTaskRepository, TodoTaskRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Auth services
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // SignalR
        services.AddSignalR();
        services.AddScoped<ITodoHubService, TodoHubService>();

        return services;
    }
}
