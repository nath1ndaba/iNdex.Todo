using FluentAssertions;
using iNdex.Todo.Application.Common.Result;
using iNdex.Todo.Domain.Errors;

namespace iNdex.Todo.UnitTests.Common;

public sealed class ResultTests
{
    [Fact]
    public void Success_IsSuccessTrue_IsFailureFalse()
    {
        var result = Result.Success("hello");
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public void Failure_IsSuccessFalse_IsFailureTrue()
    {
        var error  = new Error("Test.Error", "Something went wrong");
        var result = Result.Failure<string>(error);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Value_OnFailure_ThrowsInvalidOperationException()
    {
        var result = Result.Failure<int>(new Error("X", "Y"));
        var act    = () => { var _ = result.Value; };
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ImplicitConversion_FromValue_IsSuccess()
    {
        Result<int> result = 42;
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void NotFound_Error_ContainsEntityNameAndId()
    {
        var id    = Guid.NewGuid();
        var error = Error.NotFound("TodoList", id);
        error.Code.Should().Contain("NotFound");
        error.Message.Should().Contain(id.ToString());
    }
}
