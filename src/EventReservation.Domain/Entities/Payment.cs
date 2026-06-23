using EventReservation.Domain.Common;
using EventReservation.Domain.Enums;

namespace EventReservation.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid ReservationId { get; private set; }

    public Reservation Reservation { get; private set; } = null!;

    public decimal Amount { get; private set; }

    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;

    public PaymentMethod Method { get; private set; }

    public DateTime? PaidAt { get; private set; }

    public string? FailureReason { get; private set; }

    private Payment()
    {
    }

    public Payment(
        Guid reservationId,
        decimal amount,
        PaymentMethod method)
    {
        ReservationId = reservationId;
        Amount = amount;
        Method = method;
        Status = PaymentStatus.Pending;
    }

    public void MarkAsSuccess()
    {
        Status = PaymentStatus.Success;
        PaidAt = DateTime.UtcNow;
        FailureReason = null;

        MarkAsUpdated();
    }

    public void MarkAsFailed(string failureReason)
    {
        Status = PaymentStatus.Failed;
        FailureReason = failureReason;

        MarkAsUpdated();
    }

    public void Refund()
    {
        Status = PaymentStatus.Refunded;

        MarkAsUpdated();
    }
}