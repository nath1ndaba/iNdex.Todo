using iNdex.Todo.Application.Common.Interfaces;

namespace iNdex.Todo.Infrastructure.Auth;

public sealed class PasswordService : IPasswordService
{
    private const int WorkFactor = 12;

    public string Hash(string password)
        => BCrypt.Net.BCrypt.EnhancedHashPassword(password, WorkFactor);

    public bool Verify(string password, string hash)
        => BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
}
