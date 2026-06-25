namespace EventReservation.Application.Features.Auth.Common;

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt,
    AuthUserResponse User);