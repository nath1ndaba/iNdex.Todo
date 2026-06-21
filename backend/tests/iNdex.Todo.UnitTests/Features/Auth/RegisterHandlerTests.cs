using FluentAssertions;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Features.Auth.Login;
using iNdex.Todo.Contracts.Requests;
using iNdex.Todo.Domain.Entities;
using NSubstitute;

namespace iNdex.Todo.UnitTests.Features.Auth;

public sealed class RegisterHandlerTests
{
    private readonly IUserRepository         _userRepo    = Substitute.For<IUserRepository>();
    private readonly IPasswordService        _passwordSvc = Substitute.For<IPasswordService>();
    private readonly IJwtService             _jwtSvc      = Substitute.For<IJwtService>();
    private readonly IRefreshTokenRepository _rtRepo      = Substitute.For<IRefreshTokenRepository>();
    private readonly IUnitOfWork             _uow         = Substitute.For<IUnitOfWork>();
    private readonly RegisterHandler         _handler;

    public RegisterHandlerTests()
    {
        _handler = new RegisterHandler(_userRepo, _passwordSvc, _jwtSvc, _rtRepo, _uow);

        _passwordSvc.Hash(Arg.Any<string>()).Returns("hashed-pw");
        _jwtSvc.GenerateAccessToken(Arg.Any<User>()).Returns("access-token");
        _jwtSvc.GenerateRefreshToken().Returns("refresh-token");
        _jwtSvc.AccessTokenExpiry.Returns(DateTime.UtcNow.AddMinutes(15));
        _jwtSvc.RefreshTokenExpiry.Returns(DateTime.UtcNow.AddDays(7));
    }

    [Fact]
    public async Task HandleAsync_NewEmail_CreatesUserAndReturnsTokens()
    {
        _userRepo.GetByEmailAsync("bob@example.com", Arg.Any<CancellationToken>()).Returns((User?)null);

        var req    = new RegisterUserRequest("Bob", "Jones", "bob@example.com", "Password1");
        var result = await _handler.HandleAsync(req);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-token");
        result.Value.User.Email.Should().Be("bob@example.com");

        await _userRepo.Received(1).AddAsync(
            Arg.Is<User>(u => u.Email == "bob@example.com" && u.PasswordHash == "hashed-pw"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_DuplicateEmail_ReturnsConflict()
    {
        var existing = new User { Id = Guid.NewGuid(), Email = "bob@example.com", CreatedBy = "system" };
        _userRepo.GetByEmailAsync("bob@example.com", Arg.Any<CancellationToken>()).Returns(existing);

        var result = await _handler.HandleAsync(
            new RegisterUserRequest("Bob", "Jones", "bob@example.com", "Password1"));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.EmailTaken");
        await _userRepo.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }
}
