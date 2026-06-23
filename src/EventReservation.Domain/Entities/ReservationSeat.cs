using EventReservation.Domain.Common;

namespace EventReservation.Domain.Entities;

public class ReservationSeat : BaseEntity
{
    public Guid ReservationId { get; private set; }

    public Reservation Reservation { get; private set; } = null!;

    public Guid EventSeatId { get; private set; }

    public EventSeat EventSeat { get; private set; } = null!;

    public decimal Price { get; private set; }

    private ReservationSeat()
    {
    }

    public ReservationSeat(
        Guid reservationId,
        Guid eventSeatId,
        decimal price)
    {
        ReservationId = reservationId;
        EventSeatId = eventSeatId;
        Price = price;
    }
}