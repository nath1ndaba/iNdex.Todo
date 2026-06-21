using iNdex.Todo.Application.Common.Result;

namespace iNdex.Todo.Application.Common.Interfaces;

public interface IHandler<TRequest, TResponse>
{
    Task<Result<TResponse>> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}
