using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using iNdex.Todo.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace iNdex.Todo.Infrastructure.Auth;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? Principal =>
        httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var value = Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Email =>
        Principal?.FindFirstValue(JwtRegisteredClaimNames.Email);

    public bool IsAuthenticated =>
        Principal?.Identity?.IsAuthenticated ?? false;
}
