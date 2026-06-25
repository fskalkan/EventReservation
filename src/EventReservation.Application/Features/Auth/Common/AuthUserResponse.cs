namespace EventReservation.Application.Features.Auth.Common;

public sealed record AuthUserResponse(
    Guid Id,
    string FullName,
    string Email,
    string Role);