using EventReservation.Domain.Common;
using EventReservation.Domain.Enums;

namespace EventReservation.Domain.Entities;

public class EventSeat : BaseEntity
{
    public Guid EventId { get; private set; }

    public Event Event { get; private set; } = null!;

    public Guid SeatId { get; private set; }

    public Seat Seat { get; private set; } = null!;

    public decimal Price { get; private set; }

    public EventSeatStatus Status { get; private set; } = EventSeatStatus.Available;

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    private EventSeat()
    {
    }

    public EventSeat(
        Guid eventId,
        Guid seatId,
        decimal price)
    {
        EventId = eventId;
        SeatId = seatId;
        Price = price;
        Status = EventSeatStatus.Available;
    }

    public void UpdatePrice(decimal price)
    {
        Price = price;
        MarkAsUpdated();
    }

    public void Lock()
    {
        Status = EventSeatStatus.Locked;
        MarkAsUpdated();
    }

    public void Reserve()
    {
        Status = EventSeatStatus.Reserved;
        MarkAsUpdated();
    }

    public void Release()
    {
        Status = EventSeatStatus.Available;
        MarkAsUpdated();
    }
}