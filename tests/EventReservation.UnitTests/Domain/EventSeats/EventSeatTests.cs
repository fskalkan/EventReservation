using EventReservation.Domain.Entities;
using EventReservation.Domain.Enums;
using FluentAssertions;

namespace EventReservation.UnitTests.Domain.EventSeats;

public sealed class EventSeatTests
{
    [Fact]
    public void Constructor_ShouldCreateAvailableEventSeat()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var seatId = Guid.NewGuid();
        var price = 1000m;

        // Act
        var eventSeat = new EventSeat(
            eventId,
            seatId,
            price);

        // Assert
        eventSeat.EventId.Should().Be(eventId);
        eventSeat.SeatId.Should().Be(seatId);
        eventSeat.Price.Should().Be(price);
        eventSeat.Status.Should().Be(EventSeatStatus.Available);
    }

    [Fact]
    public void Lock_ShouldSetStatusToLocked()
    {
        // Arrange
        var eventSeat = new EventSeat(
            Guid.NewGuid(),
            Guid.NewGuid(),
            1000m);

        // Act
        eventSeat.Lock();

        // Assert
        eventSeat.Status.Should().Be(EventSeatStatus.Locked);
        eventSeat.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Reserve_ShouldSetStatusToReserved()
    {
        // Arrange
        var eventSeat = new EventSeat(
            Guid.NewGuid(),
            Guid.NewGuid(),
            1000m);

        // Act
        eventSeat.Reserve();

        // Assert
        eventSeat.Status.Should().Be(EventSeatStatus.Reserved);
        eventSeat.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Release_ShouldSetStatusToAvailable()
    {
        // Arrange
        var eventSeat = new EventSeat(
            Guid.NewGuid(),
            Guid.NewGuid(),
            1000m);

        eventSeat.Lock();

        // Act
        eventSeat.Release();

        // Assert
        eventSeat.Status.Should().Be(EventSeatStatus.Available);
        eventSeat.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdatePrice_ShouldChangePrice()
    {
        // Arrange
        var eventSeat = new EventSeat(
            Guid.NewGuid(),
            Guid.NewGuid(),
            1000m);

        // Act
        eventSeat.UpdatePrice(1500m);

        // Assert
        eventSeat.Price.Should().Be(1500m);
        eventSeat.UpdatedAt.Should().NotBeNull();
    }
}