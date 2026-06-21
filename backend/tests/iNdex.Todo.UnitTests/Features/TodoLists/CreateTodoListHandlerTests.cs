using FluentAssertions;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Features.TodoLists.CreateTodoList;
using iNdex.Todo.Contracts.Requests;
using iNdex.Todo.Domain.Entities;
using NSubstitute;

namespace iNdex.Todo.UnitTests.Features.TodoLists;

public sealed class CreateTodoListHandlerTests
{
    private readonly ITodoListRepository _listRepo  = Substitute.For<ITodoListRepository>();
    private readonly IUserRepository     _userRepo  = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork         _uow       = Substitute.For<IUnitOfWork>();
    private readonly CreateTodoListHandler _handler;

    public CreateTodoListHandlerTests()
        => _handler = new CreateTodoListHandler(_listRepo, _userRepo, _uow);

    [Fact]
    public async Task HandleAsync_ValidRequest_ReturnsCreatedResponse()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        _userRepo.ExistsAsync(ownerId, Arg.Any<CancellationToken>()).Returns(true);

        var request = new CreateTodoListRequest("Work", "My work tasks", "#00AEEF", null, ownerId);

        // Act
        var result = await _handler.HandleAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();

        await _listRepo.Received(1).AddAsync(
            Arg.Is<TodoList>(l => l.Name == "Work" && l.OwnerId == ownerId),
            Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_OwnerNotFound_ReturnsFailure()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        _userRepo.ExistsAsync(ownerId, Arg.Any<CancellationToken>()).Returns(false);

        var request = new CreateTodoListRequest("Work", null, null, null, ownerId);

        // Act
        var result = await _handler.HandleAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");

        await _listRepo.DidNotReceive().AddAsync(Arg.Any<TodoList>(), Arg.Any<CancellationToken>());
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
