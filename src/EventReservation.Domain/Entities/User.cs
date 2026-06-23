using EventReservation.Domain.Common;
using EventReservation.Domain.Enums;

namespace EventReservation.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; private set; } = null!;

    public string Email { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

    public UserRole Role { get; private set; }

    public bool IsActive { get; private set; } = true;

    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    public ICollection<Venue> CreatedVenues { get; private set; } = new List<Venue>();

    public ICollection<Event> OrganizedEvents { get; private set; } = new List<Event>();

    public ICollection<Reservation> Reservations { get; private set; } = new List<Reservation>();

    private User()
    {
    }

    public User(string fullName, string email, string passwordHash, UserRole role)
    {
        FullName = fullName;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
    }

    public void ChangeRole(UserRole role)
    {
        Role = role;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    public void ChangePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        MarkAsUpdated();
    }
}