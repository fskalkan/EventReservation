using EventReservation.Application.Abstractions.Authentication;
using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Common.Exceptions;
using EventReservation.Application.Features.Reservations.Common;

namespace EventReservation.Application.Features.Reservations.GetReservationById;

public sealed class GetReservationByIdQueryHandler
    : IQueryHandler<GetReservationByIdQuery, ReservationResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IReservationRepository _reservationRepository;

    public GetReservationByIdQueryHandler(
        ICurrentUserService currentUserService,
        IReservationRepository reservationRepository)
    {
        _currentUserService = currentUserService;
        _reservationRepository = reservationRepository;
    }

    public async Task<ReservationResponse> Handle(GetReservationByIdQuery query, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var reservation = await _reservationRepository.GetResponseByIdAsync(query.ReservationId, cancellationToken);

        if (reservation is null)
        {
            throw new NotFoundException("Reservation not found.");
        }

        if (reservation.CustomerId != _currentUserService.UserId.Value)
        {
            throw new ForbiddenAccessException("You cannot view another customer's reservation.");
        }

        return reservation;
    }
}