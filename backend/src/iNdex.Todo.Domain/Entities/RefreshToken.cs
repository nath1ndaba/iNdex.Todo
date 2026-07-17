using iNdex.Todo.Domain.Common;

namespace iNdex.Todo.Domain.Entities;

public sealed class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? RevokedReason { get; set; }

    public bool IsExpired    => DateTime.UtcNow >= ExpiresAt;
    public bool IsTokenActive => !IsRevoked && !IsExpired;

    public User User { get; set; } = null!;
}
