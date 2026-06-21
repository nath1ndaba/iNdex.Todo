namespace iNdex.Todo.Mobile.Services;

/// <summary>
/// Background service that silently refreshes the JWT access token
/// 60 seconds before it expires, so the user never gets logged out mid-session.
/// Register as a singleton and call StartAsync() from MainLayout.
/// </summary>
public sealed class TokenRefreshService(
    AppState appState,
    IAuthApi authApi) : IAsyncDisposable
{
    private Timer? _timer;

    public void Start()
    {
        // Check every 30 seconds
        _timer = new Timer(async _ => await TryRefreshAsync(), null,
            TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    private async Task TryRefreshAsync()
    {
        if (!appState.IsAuthenticated) return;
        if (appState.TokenExpiry is null) return;

        // Refresh if token expires within 60 seconds
        var timeLeft = appState.TokenExpiry.Value - DateTime.UtcNow;
        if (timeLeft > TimeSpan.FromSeconds(60)) return;

        if (string.IsNullOrEmpty(appState.RefreshToken)) return;

        try
        {
            var resp = await authApi.RefreshAsync(new(appState.RefreshToken));
            if (resp.IsSuccessStatusCode && resp.Content is not null)
            {
                var auth = resp.Content;
                appState.UpdateTokens(auth.AccessToken, auth.RefreshToken, auth.ExpiresAt);
            }
            else
            {
                // Refresh token itself expired — force re-login
                appState.ClearSession();
            }
        }
        catch
        {
            // Network error — will retry in 30 seconds
        }
    }

    public ValueTask DisposeAsync()
    {
        _timer?.Dispose();
        return ValueTask.CompletedTask;
    }
}
