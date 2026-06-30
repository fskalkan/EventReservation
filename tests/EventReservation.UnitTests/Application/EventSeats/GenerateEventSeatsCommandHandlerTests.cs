using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Common.Exceptions;
using EventReservation.Application.Features.EventSeats.Common;
using EventReservation.Application.Features.EventSeats.GenerateEventSeats;
using EventReservation.Application.Features.Seats.Common;
using EventReservation.Domain.Entities;
using EventReservation.Domain.Enums;
using FluentAssertions;
using Moq;

namespace EventReservation.UnitTests.Application.EventSeats;

public sealed class GenerateEventSeatsCommandHandlerTests
{
    private readonly Mock<IEventRepository> _eventRepositoryMock = new();
    private readonly Mock<ISeatRepository> _seatRepositoryMock = new();
    private readonly Mock<IEventSeatRepository> _eventSeatRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task Handle_ShouldGenerateEventSeats_WithSectionPrices_WhenRequestIsValid()
    {
        // Arrange
        var eventEntity = CreateDraftEvent();

        var seats = new List<SeatForEventSeatGenerationDto>
        {
            new(Guid.NewGuid(), "A"),
            new(Guid.NewGuid(), "B"),
            new(Guid.NewGuid(), "C")
        };

        var command = new GenerateEventSeatsCommand(
            eventEntity.Id,
            DefaultPrice: 500m,
            SectionPrices: new Dictionary<string, decimal>
            {
                { "A", 1000m },
                { "B", 750m }
            });

        var responses = new List<EventSeatResponse>
        {
            new(Guid.NewGuid(), eventEntity.Id, seats[0].Id, "A", "1", 1, "A-1-1", 1000m, EventSeatStatus.Available, DateTime.UtcNow),
            new(Guid.NewGuid(), eventEntity.Id, seats[1].Id, "B", "1", 2, "B-1-2", 750m, EventSeatStatus.Available, DateTime.UtcNow),
            new(Guid.NewGuid(), eventEntity.Id, seats[2].Id, "C", "1", 3, "C-1-3", 500m, EventSeatStatus.Available, DateTime.UtcNow)
        };

        IReadOnlyList<EventSeat>? capturedEventSeats = null;

        _eventRepositoryMock
            .Setup(x => x.GetByIdAsync(eventEntity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        _eventSeatRepositoryMock
            .Setup(x => x.ExistsByEventIdAsync(eventEntity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _seatRepositoryMock
            .Setup(x => x.GetForEventSeatGenerationByVenueIdAsync(eventEntity.VenueId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(seats);

        _eventSeatRepositoryMock
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<EventSeat>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<EventSeat>, CancellationToken>((eventSeats, _) =>
            {
                capturedEventSeats = eventSeats.ToList();
            })
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _eventSeatRepositoryMock
            .Setup(x => x.GetResponsesByEventIdAsync(eventEntity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(responses);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        capturedEventSeats.Should().NotBeNull();
        capturedEventSeats.Should().HaveCount(3);

        capturedEventSeats!
            .Single(x => x.SeatId == seats[0].Id)
            .Price
            .Should()
            .Be(1000m);

        capturedEventSeats
            .Single(x => x.SeatId == seats[1].Id)
            .Price
            .Should()
            .Be(750m);

        capturedEventSeats
            .Single(x => x.SeatId == seats[2].Id)
            .Price
            .Should()
            .Be(500m);

        capturedEventSeats
            .Should()
            .OnlyContain(x => x.EventId == eventEntity.Id);

        capturedEventSeats
            .Should()
            .OnlyContain(x => x.Status == EventSeatStatus.Available);

        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(responses);

        _eventSeatRepositoryMock.Verify(
            x => x.AddRangeAsync(It.IsAny<IEnumerable<EventSeat>>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _eventSeatRepositoryMock.Verify(
            x => x.GetResponsesByEventIdAsync(eventEntity.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldApplySectionPrices_CaseInsensitiveAndTrimmed()
    {
        // Arrange
        var eventEntity = CreateDraftEvent();

        var seats = new List<SeatForEventSeatGenerationDto>
        {
            new(Guid.NewGuid(), "A"),
            new(Guid.NewGuid(), " b "),
            new(Guid.NewGuid(), "C")
        };

        var command = new GenerateEventSeatsCommand(
            eventEntity.Id,
            DefaultPrice: 500m,
            SectionPrices: new Dictionary<string, decimal>
            {
                { " a ", 1000m },
                { "B", 750m }
            });

        IReadOnlyList<EventSeat>? capturedEventSeats = null;

        _eventRepositoryMock
            .Setup(x => x.GetByIdAsync(eventEntity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        _eventSeatRepositoryMock
            .Setup(x => x.ExistsByEventIdAsync(eventEntity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _seatRepositoryMock
            .Setup(x => x.GetForEventSeatGenerationByVenueIdAsync(eventEntity.VenueId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(seats);

        _eventSeatRepositoryMock
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<EventSeat>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<EventSeat>, CancellationToken>((eventSeats, _) =>
            {
                capturedEventSeats = eventSeats.ToList();
            })
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _eventSeatRepositoryMock
            .Setup(x => x.GetResponsesByEventIdAsync(eventEntity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EventSeatResponse>());

        var handler = CreateHandler();

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        capturedEventSeats.Should().NotBeNull();
        capturedEventSeats.Should().HaveCount(3);

        capturedEventSeats!
            .Single(x => x.SeatId == seats[0].Id)
            .Price
            .Should()
            .Be(1000m);

        capturedEventSeats
            .Single(x => x.SeatId == seats[1].Id)
            .Price
            .Should()
            .Be(750m);

        capturedEventSeats
            .Single(x => x.SeatId == seats[2].Id)
            .Price
            .Should()
            .Be(500m);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenEventDoesNotExist()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        var command = new GenerateEventSeatsCommand(
            eventId,
            DefaultPrice: 500m,
            SectionPrices: new Dictionary<string, decimal>());

        _eventRepositoryMock
            .Setup(x => x.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);

        var handler = CreateHandler();

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Event not found.");

        _eventSeatRepositoryMock.Verify(
            x => x.AddRangeAsync(It.IsAny<IReadOnlyList<EventSeat>>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenEventIsNotDraft()
    {
        // Arrange
        var eventEntity = CreateDraftEvent();
        eventEntity.Publish();

        var command = new GenerateEventSeatsCommand(
            eventEntity.Id,
            DefaultPrice: 500m,
            SectionPrices: new Dictionary<string, decimal>());

        _eventRepositoryMock
            .Setup(x => x.GetByIdAsync(eventEntity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        var handler = CreateHandler();

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Event seats can only be generated for draft events.");

        _eventSeatRepositoryMock.Verify(
            x => x.AddRangeAsync(It.IsAny<IReadOnlyList<EventSeat>>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenEventSeatsAlreadyGenerated()
    {
        // Arrange
        var eventEntity = CreateDraftEvent();

        var command = new GenerateEventSeatsCommand(
            eventEntity.Id,
            DefaultPrice: 500m,
            SectionPrices: new Dictionary<string, decimal>());

        _eventRepositoryMock
            .Setup(x => x.GetByIdAsync(eventEntity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        _eventSeatRepositoryMock
            .Setup(x => x.ExistsByEventIdAsync(eventEntity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = CreateHandler();

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Event seats already generated for this event.");

        _eventSeatRepositoryMock.Verify(
            x => x.AddRangeAsync(It.IsAny<IReadOnlyList<EventSeat>>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenVenueHasNoSeats()
    {
        // Arrange
        var eventEntity = CreateDraftEvent();

        var command = new GenerateEventSeatsCommand(
            eventEntity.Id,
            DefaultPrice: 500m,
            SectionPrices: new Dictionary<string, decimal>());

        _eventRepositoryMock
            .Setup(x => x.GetByIdAsync(eventEntity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        _eventSeatRepositoryMock
            .Setup(x => x.ExistsByEventIdAsync(eventEntity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _seatRepositoryMock
            .Setup(x => x.GetForEventSeatGenerationByVenueIdAsync(eventEntity.VenueId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SeatForEventSeatGenerationDto>());

        var handler = CreateHandler();

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Venue has no seats.");

        _eventSeatRepositoryMock.Verify(
            x => x.AddRangeAsync(It.IsAny<IReadOnlyList<EventSeat>>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private GenerateEventSeatsCommandHandler CreateHandler()
    {
        return new GenerateEventSeatsCommandHandler(
            _eventRepositoryMock.Object,
            _seatRepositoryMock.Object,
            _eventSeatRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    private static Event CreateDraftEvent()
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