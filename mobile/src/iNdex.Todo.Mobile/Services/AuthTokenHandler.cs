namespace iNdex.Todo.Mobile.Services;

/// <summary>
/// DelegatingHandler that attaches the stored JWT access token to every outgoing
/// Refit request. If the token is missing (user not signed in) the request goes
/// through without an Authorization header — the server will return 401.
/// </summary>
public sealed class AuthTokenHandler(AppState appState) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = appState.AccessToken;
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(request, cancellationToken);
    }
}
