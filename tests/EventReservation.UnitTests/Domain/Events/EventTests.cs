using EventReservation.Domain.Entities;
using EventReservation.Domain.Enums;
using FluentAssertions;

namespace EventReservation.UnitTests.Domain.Events;

public sealed class EventTests
{
    [Fact]
    public void Constructor_ShouldCreateEvent_WithDraftStatus()
    {
        // Arrange
        var venueId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();
        var title = "Test Event";
        var description = "Test Description";
        var startDate = DateTime.UtcNow.AddDays(10);
        var endDate = startDate.AddHours(2);

        // Act
        var eventEntity = new Event(
            venueId,
            organizerId,
            title,
            description,
            startDate,
            endDate);

        // Assert
        eventEntity.VenueId.Should().Be(venueId);
        eventEntity.OrganizerId.Should().Be(organizerId);
        eventEntity.Title.Should().Be(title);
        eventEntity.Description.Should().Be(description);
        eventEntity.StartDate.Should().Be(startDate);
        eventEntity.EndDate.Should().Be(endDate);
        eventEntity.Status.Should().Be(EventStatus.Draft);
    }

    [Fact]
    public void Publish_ShouldSetStatusToPublished_AndUpdateUpdatedAt()
    {
        // Arrange
        var eventEntity = CreateEvent();

        // Act
        eventEntity.Publish();

        // Assert
        eventEntity.Status.Should().Be(EventStatus.Published);
        eventEntity.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Cancel_ShouldSetStatusToCancelled_AndUpdateUpdatedAt()
    {
        // Arrange
        var eventEntity = CreateEvent();

        // Act
        eventEntity.Cancel();

        // Assert
        eventEntity.Status.Should().Be(EventStatus.Cancelled);
        eventEntity.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Complete_ShouldSetStatusToCompleted_AndUpdateUpdatedAt()
    {
        // Arrange
        var eventEntity = CreateEvent();

        // Act
        eventEntity.Complete();

        // Assert
        eventEntity.Status.Should().Be(EventStatus.Completed);
        eventEntity.UpdatedAt.Should().NotBeNull();
    }

    private static Event CreateEvent()
    {
        var startDate = DateTime.UtcNow.AddDays(10);

        return new Event(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Event",
            "Test Description",
            startDate,
            startDate.AddHours(2));
    }
}