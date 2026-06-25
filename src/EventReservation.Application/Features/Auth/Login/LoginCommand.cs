using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Features.Auth.Common;

namespace EventReservation.Application.Features.Auth.Login;

public sealed record LoginCommand(
    string Email,
    string Password) : ICommand<AuthResponse>;