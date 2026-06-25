using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Features.Auth.Common;

namespace EventReservation.Application.Features.Auth.Me;

public sealed record MeQuery : IQuery<AuthUserResponse>;