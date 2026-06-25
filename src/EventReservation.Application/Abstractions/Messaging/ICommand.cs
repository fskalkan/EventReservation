using MediatR;

namespace EventReservation.Application.Abstractions.Messaging;

public interface ICommand<TResponse> : IRequest<TResponse>
{
}