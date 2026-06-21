using FluentAssertions;
using FluentValidation.TestHelper;
using iNdex.Todo.Application.Features.Auth.Login;
using iNdex.Todo.Application.Features.TodoLists.CreateTodoList;
using iNdex.Todo.Application.Features.TodoTasks.CreateTodoTask;
using iNdex.Todo.Contracts.Requests;

namespace iNdex.Todo.UnitTests.Common;

public sealed class ValidatorTests
{
    // ── RegisterValidator ─────────────────────────────────────────────────────
    private readonly RegisterValidator _registerValidator = new();

    [Fact]
    public void Register_ValidRequest_PassesValidation()
    {
        var req = new RegisterUserRequest("Alice", "Smith", "alice@example.com", "Password1");
        _registerValidator.TestValidate(req).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "Smith", "a@b.com",  "Password1", "FirstName")]
    [InlineData("A", "",     "a@b.com",  "Password1", "LastName")]
    [InlineData("A", "S",   "not-email", "Password1", "Email")]
    [InlineData("A", "S",   "a@b.com",  "short",     "Password")]
    [InlineData("A", "S",   "a@b.com",  "alllower1", "Password")]
    [InlineData("A", "S",   "a@b.com",  "NoDigitPass","Password")]
    public void Register_InvalidFields_FailsValidation(
        string first, string last, string email, string password, string expectedField)
    {
        var req = new RegisterUserRequest(first, last, email, password);
        _registerValidator.TestValidate(req).ShouldHaveValidationErrorFor(expectedField);
    }

    // ── CreateTodoListValidator ───────────────────────────────────────────────
    private readonly CreateTodoListValidator _listValidator = new();

    [Fact]
    public void CreateTodoList_ValidRequest_PassesValidation()
    {
        var req = new CreateTodoListRequest("Work", null, "#00AEEF", null, Guid.NewGuid());
        _listValidator.TestValidate(req).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateTodoList_EmptyName_FailsValidation()
    {
        var req = new CreateTodoListRequest("", null, null, null, Guid.NewGuid());
        _listValidator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateTodoList_InvalidColor_FailsValidation()
    {
        var req = new CreateTodoListRequest("Work", null, "notacolor", null, Guid.NewGuid());
        _listValidator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Color);
    }

    [Fact]
    public void CreateTodoList_ValidHexColor_PassesValidation()
    {
        var req = new CreateTodoListRequest("Work", null, "#F5A623", null, Guid.NewGuid());
        _listValidator.TestValidate(req).ShouldNotHaveAnyValidationErrors();
    }

    // ── CreateTodoTaskValidator ───────────────────────────────────────────────
    private readonly CreateTodoTaskValidator _taskValidator = new();

    [Fact]
    public void CreateTodoTask_ValidRequest_PassesValidation()
    {
        var req = new CreateTodoTaskRequest("Buy milk", null, null, 0, null, Guid.NewGuid());
        _taskValidator.TestValidate(req).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateTodoTask_EmptyTitle_FailsValidation()
    {
        var req = new CreateTodoTaskRequest("", null, null, 0, null, Guid.NewGuid());
        _taskValidator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    public void CreateTodoTask_InvalidPriority_FailsValidation(int priority)
    {
        var req = new CreateTodoTaskRequest("Task", null, null, priority, null, Guid.NewGuid());
        _taskValidator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Priority);
    }

    [Fact]
    public void CreateTodoTask_PastDueDate_FailsValidation()
    {
        var req = new CreateTodoTaskRequest("Task", null, DateTime.UtcNow.AddDays(-1), 0, null, Guid.NewGuid());
        _taskValidator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.DueDate);
    }
}
