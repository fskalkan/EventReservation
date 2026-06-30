using EventReservation.Domain.Entities;
using EventReservation.Domain.Enums;
using FluentAssertions;

namespace EventReservation.UnitTests.Domain.Reservations;

public sealed class ReservationTests
{
    [Fact]
    public void Constructor_ShouldCreatePendingPaymentReservation_WithZeroTotalAmount()
    {
        // Arrange
        var reservationCode = "RSV-TEST";
        var customerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddMinutes(10);

        // Act
        var reservation = new Reservation(
            reservationCode,
            customerId,
            eventId,
            expiresAt);

        // Assert
        reservation.ReservationCode.Should().Be(reservationCode);
        reservation.CustomerId.Should().Be(customerId);
        reservation.EventId.Should().Be(eventId);
        reservation.ExpiresAt.Should().Be(expiresAt);
        reservation.Status.Should().Be(ReservationStatus.PendingPayment);
        reservation.TotalAmount.Should().Be(0);
        reservation.ReservationSeats.Should().BeEmpty();
    }

    [Fact]
    public void AddSeat_ShouldAddReservationSeat_AndIncreaseTotalAmount()
    {
        // Arrange
        var reservation = new Reservation(
            "RSV-TEST",
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddMinutes(10));

        var eventSeatId = Guid.NewGuid();
        var price = 1000m;

        // Act
        reservation.AddSeat(eventSeatId, price);

        // Assert
        reservation.ReservationSeats.Should().HaveCount(1);
        reservation.TotalAmount.Should().Be(price);

        var reservationSeat = reservation.ReservationSeats.First();

        reservationSeat.ReservationId.Should().Be(reservation.Id);
        reservationSeat.EventSeatId.Should().Be(eventSeatId);
        reservationSeat.Price.Should().Be(price);
    }

    [Fact]
    public void AddSeat_ShouldIncreaseTotalAmount_WhenMultipleSeatsAdded()
    {
        // Arrange
        var reservation = new Reservation(
            "RSV-TEST",
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddMinutes(10));

        // Act
        reservation.AddSeat(Guid.NewGuid(), 1000m);
        reservation.AddSeat(Guid.NewGuid(), 750m);

        // Assert
        reservation.ReservationSeats.Should().HaveCount(2);
        reservation.TotalAmount.Should().Be(1750m);
    }

    [Fact]
    public void Confirm_ShouldSetStatusToConfirmed_AndSetConfirmedAt()
    {
        // Arrange
        var reservation = new Reservation(
            "RSV-TEST",
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddMinutes(10));

        // Act
        reservation.Confirm();

        // Assert
        reservation.Status.Should().Be(ReservationStatus.Confirmed);
        reservation.ConfirmedAt.Should().NotBeNull();
        reservation.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Cancel_ShouldSetStatusToCancelled_AndSetCancelledAt()
    {
        // Arrange
        var reservation = new Reservation(
            "RSV-TEST",
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddMinutes(10));

        // Act
        reservation.Cancel();

        // Assert
        reservation.Status.Should().Be(ReservationStatus.Cancelled);
        reservation.CancelledAt.Should().NotBeNull();
        reservation.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Expire_ShouldSetStatusToExpired_AndSetExpiredAt()
    {
        // Arrange
        var reservation = new Reservation(
            "RSV-TEST",
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddMinutes(10));

        // Act
        reservation.Expire();

        // Assert
        reservation.Status.Should().Be(ReservationStatus.Expired);
        reservation.ExpiredAt.Should().NotBeNull();
        reservation.UpdatedAt.Should().NotBeNull();
    }
}