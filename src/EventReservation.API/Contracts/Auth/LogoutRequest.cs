namespace EventReservation.API.Contracts.Auth;

public sealed record LogoutRequest(
    string RefreshToken);