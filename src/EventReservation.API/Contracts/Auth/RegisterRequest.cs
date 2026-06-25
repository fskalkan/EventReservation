namespace EventReservation.API.Contracts.Auth;

public sealed record RegisterRequest(
    string FullName,
    string Email,
    string Password);