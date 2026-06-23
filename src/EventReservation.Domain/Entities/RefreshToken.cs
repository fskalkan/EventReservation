using EventReservation.Domain.Common;

namespace EventReservation.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }

    public User User { get; private set; } = null!;

    public string Token { get; private set; } = null!;

    public DateTime ExpiresAt { get; private set; }

    public DateTime? RevokedAt { get; private set; }

    public bool IsRevoked => RevokedAt.HasValue;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    public bool IsActive => !IsRevoked && !IsExpired;

    private RefreshToken()
    {
    }

    public RefreshToken(Guid userId, string token, DateTime expiresAt)
    {
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
    }

    public void Revoke()
    {
        if (IsRevoked)
            return;

        RevokedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }
}