using iNdex.Todo.Mobile.Services;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Refit;

namespace iNdex.Todo.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ── Blazor WebView ────────────────────────────────────────────────────
        builder.Services.AddMauiBlazorWebView();

        // ── MudBlazor ─────────────────────────────────────────────────────────
        builder.Services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
            config.SnackbarConfiguration.PreventDuplicates = true;
            config.SnackbarConfiguration.VisibleStateDuration = 3000;
        });

        // ── Configuration ─────────────────────────────────────────────────────
        var apiBaseUrl = "https://localhost:51447"; // Override in appsettings / env

        // ── Refit API clients ──────────────────────────────────────────────────
        var refitSettings = new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer()
        };

        // AuthTokenHandler injects the JWT on every protected request
        builder.Services.AddTransient<AuthTokenHandler>();

        // Auth client — no token handler (these are public endpoints)
        builder.Services
            .AddRefitClient<IAuthApi>(refitSettings)
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseUrl));

        // Protected clients — attach Bearer token automatically
        builder.Services
            .AddRefitClient<IUserApi>(refitSettings)
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseUrl))
            .AddHttpMessageHandler<AuthTokenHandler>();

        builder.Services
            .AddRefitClient<ITodoListApi>(refitSettings)
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseUrl))
            .AddHttpMessageHandler<AuthTokenHandler>();

        builder.Services
            .AddRefitClient<ITodoTaskApi>(refitSettings)
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseUrl))
            .AddHttpMessageHandler<AuthTokenHandler>();

        // ── App services ───────────────────────────────────────────────────────
        builder.Services.AddSingleton<AppState>();
        builder.Services.AddSingleton<TodoRealtimeService>();
        builder.Services.AddSingleton<NotificationService>();
        builder.Services.AddSingleton<SyncQueueService>();
        builder.Services.AddSingleton<TokenRefreshService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
