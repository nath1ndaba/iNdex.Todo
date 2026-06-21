using FluentAssertions;
using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Features.TodoTasks.CompleteTask;
using iNdex.Todo.Contracts.Requests;
using iNdex.Todo.Domain.Entities;
using iNdex.Todo.Domain.Enums;
using NSubstitute;

namespace iNdex.Todo.UnitTests.Features.TodoTasks;

public sealed class CompleteTaskHandlerTests
{
    private readonly ITodoTaskRepository _repo    = Substitute.For<ITodoTaskRepository>();
    private readonly IUnitOfWork         _uow     = Substitute.For<IUnitOfWork>();
    private readonly CompleteTaskHandler _handler;

    public CompleteTaskHandlerTests()
        => _handler = new CompleteTaskHandler(_repo, _uow);

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task HandleAsync_ExistingTask_TogglesCompletion(bool isCompleted)
    {
        // Arrange
        var task = new TodoTask
        {
            Id         = Guid.NewGuid(),
            Title      = "Write tests",
            TodoListId = Guid.NewGuid(),
            Priority   = TaskPriority.High,
            IsCompleted = !isCompleted,   // starts in opposite state
            CreatedBy  = "system"
        };

        _repo.GetByIdAsync(task.Id, Arg.Any<CancellationToken>()).Returns(task);

        var command = new CompleteTaskCommand(task.Id, new CompleteTaskRequest(isCompleted));

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsCompleted.Should().Be(isCompleted);

        if (isCompleted)
            result.Value.CompletedAt.Should().NotBeNull();
        else
            result.Value.CompletedAt.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_TaskNotFound_ReturnsFailure()
    {
        // Arrange
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((TodoTask?)null);

        // Act
        var result = await _handler.HandleAsync(
            new CompleteTaskCommand(Guid.NewGuid(), new CompleteTaskRequest(true)));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
