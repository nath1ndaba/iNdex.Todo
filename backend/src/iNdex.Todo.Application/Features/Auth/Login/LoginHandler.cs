using FluentValidation;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Common.Result;
using iNdex.Todo.Contracts.Requests;
using iNdex.Todo.Contracts.Responses;
using iNdex.Todo.Domain.Entities;
using iNdex.Todo.Domain.Errors;

namespace iNdex.Todo.Application.Features.Auth.Login;

public sealed class LoginHandler(
    IUserRepository userRepository,
    IPasswordService passwordService,
    IJwtService jwtService,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork)
    : IHandler<LoginRequest, AuthResponse>
{
    public async Task<Result<AuthResponse>> HandleAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null || !passwordService.Verify(request.Password, user.PasswordHash))
            return Result.Failure<AuthResponse>(
                Error.Validation("Auth.InvalidCredentials", "Invalid email or password."));

        var accessToken  = jwtService.GenerateAccessToken(user);
        var refreshToken = new Domain.Entities. RefreshToken
        {
            UserId    = user.Id,
            Token     = jwtService.GenerateRefreshToken(),
            ExpiresAt = jwtService.RefreshTokenExpiry,
            CreatedBy = user.Id.ToString()
        };

        // Revoke old active tokens (single-session model; remove if multi-device needed)
        var active = await refreshTokenRepository.GetActiveByUserIdAsync(user.Id, cancellationToken);
        foreach (var old in active)
        {
            old.IsRevoked     = true;
            old.RevokedReason = "Replaced on new login";
            await refreshTokenRepository.UpdateAsync(old, cancellationToken);
        }

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        user.LastLoginAt = DateTime.UtcNow;
        await userRepository.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new AuthResponse(
            accessToken,
            refreshToken.Token,
            jwtService.AccessTokenExpiry,
            new UserResponse(user.Id, user.FirstName, user.LastName, user.Email,
                             user.ProfileImageUrl, user.LastLoginAt, user.CreatedAt, user.Role, user.Department)));
    }
}

public sealed class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
