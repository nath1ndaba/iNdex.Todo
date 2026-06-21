using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace iNdex.Todo.Infrastructure.Auth;

public sealed class JwtService(IOptions<JwtSettings> options) : IJwtService
{
    private readonly JwtSettings _settings = options.Value;

    public DateTime AccessTokenExpiry  => DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutes);
    public DateTime RefreshTokenExpiry => DateTime.UtcNow.AddDays(_settings.RefreshTokenDays);

    public string GenerateAccessToken(User user)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new Claim(JwtRegisteredClaimNames.Jti,  Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer:            _settings.Issuer,
            audience:          _settings.Audience,
            claims:            claims,
            expires:           AccessTokenExpiry,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
