using EventReservation.Application.Abstractions.Messaging;

namespace EventReservation.Application.Features.Auth.Logout;

public sealed record LogoutCommand(
    string RefreshToken) : ICommand<LogoutResponse>;