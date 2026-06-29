using EventReservation.Domain.Common;
using EventReservation.Domain.Enums;

namespace EventReservation.Domain.Entities;

public class Reservation : BaseEntity
{
    public string ReservationCode { get; private set; } = null!;

    public Guid CustomerId { get; private set; }

    public User Customer { get; private set; } = null!;

    public Guid EventId { get; private set; }

    public Event Event { get; private set; } = null!;

    public ReservationStatus Status { get; private set; } = ReservationStatus.PendingPayment;

    public decimal TotalAmount { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public DateTime? ConfirmedAt { get; private set; }

    public DateTime? CancelledAt { get; private set; }

    public DateTime? ExpiredAt { get; private set; }

    public ICollection<ReservationSeat> ReservationSeats { get; private set; } = new List<ReservationSeat>();

    public Payment? Payment { get; private set; }


    private Reservation()
    {
    }

    public Reservation(
        string reservationCode,
        Guid customerId,
        Guid eventId,
        DateTime expiresAt)
    {
        ReservationCode = reservationCode;
        CustomerId = customerId;
        EventId = eventId;
        ExpiresAt = expiresAt;
        Status = ReservationStatus.PendingPayment;
        TotalAmount = 0;
    }

    public void AddSeat(Guid eventSeatId, decimal price)
    {
        ReservationSeats.Add(new ReservationSeat(
            Id,
            eventSeatId,
            price));

        TotalAmount += price;

        MarkAsUpdated();
    }

    public void Confirm()
    {
        Status = ReservationStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;

        MarkAsUpdated();
    }

    public void Cancel()
    {
        Status = ReservationStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;

        MarkAsUpdated();
    }

    public void Expire()
    {
        Status = ReservationStatus.Expired;
        ExpiredAt = DateTime.UtcNow;

        MarkAsUpdated();
    }
}