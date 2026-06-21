using iNdex.Todo.Domain.Entities;

namespace iNdex.Todo.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    DateTime AccessTokenExpiry { get; }
    DateTime RefreshTokenExpiry { get; }
}

public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task<List<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default);
}

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}
