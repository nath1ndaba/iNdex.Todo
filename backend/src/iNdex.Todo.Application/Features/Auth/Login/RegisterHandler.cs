using FluentValidation;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Common.Result;
using iNdex.Todo.Contracts.Requests;
using iNdex.Todo.Contracts.Responses;
using iNdex.Todo.Domain.Entities;
using iNdex.Todo.Domain.Errors;

namespace iNdex.Todo.Application.Features.Auth.Login;

// ── Register ──────────────────────────────────────────────────────────────────

public sealed class RegisterHandler(
    IUserRepository userRepository,
    IPasswordService passwordService,
    IJwtService jwtService,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork)
    : IHandler<RegisterUserRequest, AuthResponse>
{
    public async Task<Result<AuthResponse>> HandleAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            return Result.Failure<AuthResponse>(
                Error.Conflict("Auth.EmailTaken", $"Email '{request.Email}' is already registered."));

        var user = new User
        {
            FirstName    = request.FirstName,
            LastName     = request.LastName,
            Email        = request.Email,
            PasswordHash = passwordService.Hash(request.Password),
            CreatedBy    = "system"
        };

        user.Settings.Add(new UserSettings { UserId = user.Id, CreatedBy = user.Id.ToString() });

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await IssueTokensAsync(user, cancellationToken);
    }

    private async Task<Result<AuthResponse>> IssueTokensAsync(User user, CancellationToken ct)
    {
        var accessToken   = jwtService.GenerateAccessToken(user);
        var refreshToken  = new RefreshToken
        {
            UserId    = user.Id,
            Token     = jwtService.GenerateRefreshToken(),
            ExpiresAt = jwtService.RefreshTokenExpiry,
            CreatedBy = user.Id.ToString()
        };

        await refreshTokenRepository.AddAsync(refreshToken, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new AuthResponse(
            accessToken,
            refreshToken.Token,
            jwtService.AccessTokenExpiry,
            new UserResponse(user.Id, user.FirstName, user.LastName, user.Email,
                             user.ProfileImageUrl, user.LastLoginAt, user.CreatedAt)));
    }
}

public sealed class RegisterValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.");
    }
}
