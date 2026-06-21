using FluentAssertions;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Features.Auth.Login;
using iNdex.Todo.Contracts.Requests;
using iNdex.Todo.Domain.Entities;
using NSubstitute;

namespace iNdex.Todo.UnitTests.Features.Auth;

public sealed class LoginHandlerTests
{
    private readonly IUserRepository         _userRepo     = Substitute.For<IUserRepository>();
    private readonly IPasswordService        _passwordSvc  = Substitute.For<IPasswordService>();
    private readonly IJwtService             _jwtSvc       = Substitute.For<IJwtService>();
    private readonly IRefreshTokenRepository _rtRepo       = Substitute.For<IRefreshTokenRepository>();
    private readonly IUnitOfWork             _uow          = Substitute.For<IUnitOfWork>();
    private readonly LoginHandler            _handler;

    public LoginHandlerTests()
    {
        _handler = new LoginHandler(_userRepo, _passwordSvc, _jwtSvc, _rtRepo, _uow);

        // Defaults
        _jwtSvc.GenerateAccessToken(Arg.Any<User>()).Returns("access-token");
        _jwtSvc.GenerateRefreshToken().Returns("refresh-token");
        _jwtSvc.AccessTokenExpiry.Returns(DateTime.UtcNow.AddMinutes(15));
        _jwtSvc.RefreshTokenExpiry.Returns(DateTime.UtcNow.AddDays(7));
        _rtRepo.GetActiveByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
               .Returns([]);
    }

    [Fact]
    public async Task HandleAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var user = new User
        {
            Id           = Guid.NewGuid(),
            FirstName    = "Alice",
            LastName     = "Smith",
            Email        = "alice@example.com",
            PasswordHash = "hashed",
            CreatedBy    = "system"
        };

        _userRepo.GetByEmailAsync("alice@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _passwordSvc.Verify("Password1", "hashed").Returns(true);

        // Act
        var result = await _handler.HandleAsync(new LoginRequest("alice@example.com", "Password1"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-token");
        result.Value.RefreshToken.Should().Be("refresh-token");
        result.Value.User.Email.Should().Be("alice@example.com");
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ReturnsFailure()
    {
        _userRepo.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _handler.HandleAsync(new LoginRequest("x@x.com", "Password1"));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
    }

    [Fact]
    public async Task HandleAsync_WrongPassword_ReturnsFailure()
    {
        var user = new User
        {
            Id           = Guid.NewGuid(),
            Email        = "alice@example.com",
            PasswordHash = "hashed",
            CreatedBy    = "system"
        };

        _userRepo.GetByEmailAsync("alice@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _passwordSvc.Verify("WrongPass1", "hashed").Returns(false);

        var result = await _handler.HandleAsync(new LoginRequest("alice@example.com", "WrongPass1"));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
