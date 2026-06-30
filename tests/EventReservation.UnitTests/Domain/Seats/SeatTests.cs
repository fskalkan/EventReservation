using EventReservation.Domain.Entities;
using FluentAssertions;

namespace EventReservation.UnitTests.Domain.Seats;

public sealed class SeatTests
{
    [Fact]
    public void Constructor_ShouldCreateSeat_WithGivenValues()
    {
        // Arrange
        var venueId = Guid.NewGuid();
        var section = "A";
        var row = "1";
        var number = 10;

        // Act
        var seat = new Seat(
            venueId,
            section,
            row,
            number);

        // Assert
        seat.VenueId.Should().Be(venueId);
        seat.Section.Should().Be(section);
        seat.Row.Should().Be(row);
        seat.Number.Should().Be(number);
    }

    [Fact]
    public void Label_ShouldReturnSectionRowAndNumber()
    {
        // Arrange
        var seat = new Seat(
            Guid.NewGuid(),
            "A",
            "1",
            10);

        // Act
        var label = seat.Label;

        // Assert
        label.Should().Be("A-1-10");
    }

    [Fact]
    public void UpdateLocation_ShouldUpdateSeatFields_AndSetUpdatedAt()
    {
        // Arrange
        var seat = new Seat(
            Guid.NewGuid(),
            "A",
            "1",
            10);

        // Act
        seat.UpdateLocation(
            "B",
            "2",
            20);

        // Assert
        seat.Section.Should().Be("B");
        seat.Row.Should().Be("2");
        seat.Number.Should().Be(20);
        seat.Label.Should().Be("B-2-20");
        seat.UpdatedAt.Should().NotBeNull();
    }
}