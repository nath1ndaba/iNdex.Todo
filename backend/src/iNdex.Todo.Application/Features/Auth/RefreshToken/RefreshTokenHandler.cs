using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Common.Result;
using iNdex.Todo.Contracts.Requests;
using iNdex.Todo.Contracts.Responses;
using iNdex.Todo.Domain.Entities;
using iNdex.Todo.Domain.Errors;

namespace iNdex.Todo.Application.Features.Auth.RefreshToken;

public sealed class RefreshTokenHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUserRepository userRepository,
    IJwtService jwtService,
    IUnitOfWork unitOfWork)
    : IHandler<RefreshTokenRequest, AuthResponse>
{
    public async Task<Result<AuthResponse>> HandleAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);

        if (existing is null || !existing.IsActive)
            return Result.Failure<AuthResponse>(
                Error.Validation("Auth.InvalidRefreshToken", "Refresh token is invalid or expired."));

        var user = await userRepository.GetByIdAsync(existing.UserId, cancellationToken);
        if (user is null)
            return Result.Failure<AuthResponse>(Error.NotFound(nameof(User), existing.UserId));

        // Rotate the refresh token
        var newRefresh = new RefreshToken
        {
            UserId    = user.Id,
            Token     = jwtService.GenerateRefreshToken(),
            ExpiresAt = jwtService.RefreshTokenExpiry,
            CreatedBy = user.Id.ToString()
        };

        existing.IsRevoked       = true;
        existing.ReplacedByToken = newRefresh.Token;
        existing.RevokedReason   = "Rotated";

        await refreshTokenRepository.UpdateAsync(existing, cancellationToken);
        await refreshTokenRepository.AddAsync(newRefresh, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new AuthResponse(
            jwtService.GenerateAccessToken(user),
            newRefresh.Token,
            jwtService.AccessTokenExpiry,
            new UserResponse(user.Id, user.FirstName, user.LastName, user.Email,
                             user.ProfileImageUrl, user.LastLoginAt, user.CreatedAt)));
    }
}

public sealed class RevokeTokenHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork)
    : IHandler<RevokeTokenRequest, bool>
{
    public async Task<Result<bool>> HandleAsync(
        RevokeTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var token = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);

        if (token is null || !token.IsActive)
            return Result.Failure<bool>(
                Error.Validation("Auth.InvalidRefreshToken", "Token not found or already revoked."));

        token.IsRevoked     = true;
        token.RevokedReason = "Revoked by user";

        await refreshTokenRepository.UpdateAsync(token, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
