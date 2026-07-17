using iNdex.Todo.Application.Common.Interfaces;
using iNdex.Todo.Application.Common.Result;
using iNdex.Todo.Contracts.Responses;

namespace iNdex.Todo.Application.Features.Users.GetAllUsers;

public sealed record GetAllUsersQuery;

public sealed class GetAllUsersHandler(IUserRepository repository)
    : IHandler<GetAllUsersQuery, List<UserResponse>>
{
    public async Task<Result<List<UserResponse>>> HandleAsync(
        GetAllUsersQuery request, CancellationToken cancellationToken = default)
    {
        var users = await repository.GetAllAsync(cancellationToken);

        return Result.Success(users.Select(u => new UserResponse(
            u.Id, u.FirstName, u.LastName, u.Email,
            u.ProfileImageUrl, u.LastLoginAt, u.CreatedAt,
            u.Role, u.Department)).ToList());
    }
}
