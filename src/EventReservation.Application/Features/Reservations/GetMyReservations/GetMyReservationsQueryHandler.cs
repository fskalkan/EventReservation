using EventReservation.Application.Abstractions.Authentication;
using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Common.Exceptions;
using EventReservation.Application.Features.Reservations.Common;

namespace EventReservation.Application.Features.Reservations.GetMyReservations;

public sealed class GetMyReservationsQueryHandler
    : IQueryHandler<GetMyReservationsQuery, IReadOnlyList<ReservationResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IReservationRepository _reservationRepository;

    public GetMyReservationsQueryHandler(
        ICurrentUserService currentUserService,
        IReservationRepository reservationRepository)
    {
        _currentUserService = currentUserService;
        _reservationRepository = reservationRepository;
    }

    public async Task<IReadOnlyList<ReservationResponse>> Handle(GetMyReservationsQuery query, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        return await _reservationRepository.GetResponsesByCustomerIdAsync(_currentUserService.UserId.Value, cancellationToken);
    }
}