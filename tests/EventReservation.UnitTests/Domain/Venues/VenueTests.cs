using EventReservation.Domain.Entities;
using FluentAssertions;

namespace EventReservation.UnitTests.Domain.Venues;

public sealed class VenueTests
{
    [Fact]
    public void Constructor_ShouldCreateVenue_WithGivenValues()
    {
        // Arrange
        var name = "Test Venue";
        var city = "Istanbul";
        var address = "Test Address";
        var capacity = 100;
        var createdByUserId = Guid.NewGuid();

        // Act
        var venue = new Venue(
            name,
            city,
            address,
            capacity,
            createdByUserId);

        // Assert
        venue.Name.Should().Be(name);
        venue.City.Should().Be(city);
        venue.Address.Should().Be(address);
        venue.Capacity.Should().Be(capacity);
        venue.CreatedByUserId.Should().Be(createdByUserId);
        venue.Seats.Should().BeEmpty();
        venue.Events.Should().BeEmpty();
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateVenueFields_AndSetUpdatedAt()
    {
        // Arrange
        var venue = CreateVenue();

        // Act
        venue.UpdateDetails(
            "Updated Venue",
            "Ankara",
            "Updated Address",
            250);

        // Assert
        venue.Name.Should().Be("Updated Venue");
        venue.City.Should().Be("Ankara");
        venue.Address.Should().Be("Updated Address");
        venue.Capacity.Should().Be(250);
        venue.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void SoftDelete_ShouldMarkVenueAsDeleted()
    {
        // Arrange
        var venue = CreateVenue();

        // Act
        venue.SoftDelete();

        // Assert
        venue.IsDeleted.Should().BeTrue();
        venue.UpdatedAt.Should().NotBeNull();
    }

    private static Venue CreateVenue()
    {
        return new Venue(
            "Test Venue",
            "Istanbul",
            "Test Address",
            100,
            Guid.NewGuid());
    }
}