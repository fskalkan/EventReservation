using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Features.Auth.Common;

namespace EventReservation.Application.Features.Auth.Register;

public sealed record RegisterCommand(
    string FullName,
    string Email,
    string Password) : ICommand<AuthResponse>;