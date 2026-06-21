using iNdex.Todo.Mobile.Models;

namespace iNdex.Todo.Mobile.Services;

/// <summary>
/// Lightweight in-memory app state holding the authenticated session.
/// On a production app, persist AccessToken + RefreshToken in SecureStorage
/// so the user stays logged in across app restarts.
/// </summary>
public class AppState
{
    public UserResponse? CurrentUser  { get; private set; }
    public string?       AccessToken  { get; private set; }
    public string?       RefreshToken { get; private set; }
    public DateTime?     TokenExpiry  { get; private set; }

    public bool IsDarkMode     { get; private set; } = true;
    public bool IsAuthenticated => CurrentUser is not null && !string.IsNullOrEmpty(AccessToken);
    public bool IsTokenExpired  => TokenExpiry.HasValue && DateTime.UtcNow >= TokenExpiry.Value;

    public event Action? OnChange;

    public void SetSession(UserResponse user, string accessToken, string refreshToken, DateTime expiry)
    {
        CurrentUser  = user;
        AccessToken  = accessToken;
        RefreshToken = refreshToken;
        TokenExpiry  = expiry;
        NotifyChanged();
    }

    public void UpdateTokens(string accessToken, string refreshToken, DateTime expiry)
    {
        AccessToken  = accessToken;
        RefreshToken = refreshToken;
        TokenExpiry  = expiry;
        NotifyChanged();
    }

    public void ClearSession()
    {
        CurrentUser  = null;
        AccessToken  = null;
        RefreshToken = null;
        TokenExpiry  = null;
        NotifyChanged();
    }

    // Keep old name as alias for pages that used it
    public void ClearUser() => ClearSession();

    public void ToggleTheme()
    {
        IsDarkMode = !IsDarkMode;
        NotifyChanged();
    }

    private void NotifyChanged() => OnChange?.Invoke();
}
