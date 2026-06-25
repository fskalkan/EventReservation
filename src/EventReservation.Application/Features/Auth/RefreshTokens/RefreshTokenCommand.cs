using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Features.Auth.Common;

namespace EventReservation.Application.Features.Auth.RefreshTokens;

public sealed record RefreshTokenCommand(
    string RefreshToken) : ICommand<AuthResponse>;