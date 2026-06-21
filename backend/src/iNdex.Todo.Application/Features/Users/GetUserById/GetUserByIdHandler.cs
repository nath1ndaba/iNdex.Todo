using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Common.Result;
using iNdex.Todo.Contracts.Responses;
using iNdex.Todo.Domain.Entities;
using iNdex.Todo.Domain.Errors;

namespace iNdex.Todo.Application.Features.Users.GetUserById;

public sealed record GetUserByIdQuery(Guid Id);

public sealed class GetUserByIdHandler(IUserRepository repository)
    : IHandler<GetUserByIdQuery, UserResponse>
{
    public async Task<Result<UserResponse>> HandleAsync(
        GetUserByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        var user = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (user is null)
            return Result.Failure<UserResponse>(Error.NotFound(nameof(User), request.Id));

        return Result.Success(new UserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.ProfileImageUrl,
            user.LastLoginAt,
            user.CreatedAt));
    }
}
