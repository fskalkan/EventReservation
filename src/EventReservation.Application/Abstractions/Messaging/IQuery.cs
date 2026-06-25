using MediatR;

namespace EventReservation.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<TResponse>
{
}