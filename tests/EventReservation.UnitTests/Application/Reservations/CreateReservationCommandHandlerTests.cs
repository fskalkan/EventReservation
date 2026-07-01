using System.Reflection;
using EventReservation.Application.Abstractions.Authentication;
using EventReservation.Application.Abstractions.BackgroundJobs;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Abstractions.Realtime;
using EventReservation.Application.Common.Exceptions;
using EventReservation.Application.Features.Reservations.CreateReservation;
using EventReservation.Domain.Entities;
using EventReservation.Domain.Enums;
using FluentAssertions;
using Moq;

namespace EventReservation.UnitTests.Application.Reservations;

public sealed class CreateReservationCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<IEventRepository> _eventRepositoryMock = new();
    private readonly Mock<IEventSeatRepository> _eventSeatRepositoryMock = new();
    private readonly Mock<IReservationRepository> _reservationRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IReservationExpirationScheduler> _reservationExpirationSchedulerMock = new();
    private readonly Mock<IRealtimeNotifier> _realtimeNotifierMock = new();

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenSelectedSeatIsNotAvailable()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        var eventEntity = new Event(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Published Event",
            "Test Description",
            DateTime.UtcNow.AddDays(10),
            DateTime.UtcNow.AddDays(10).AddHours(2));

        eventEntity.Publish();

        var eventSeat = CreateEventSeatWithSeat(
            eventEntity.Id,
            section: "A",
            row: "1",
            number: 1,
            price: 1000m);

        eventSeat.Lock();

        var selectedSeatIds = new List<Guid>
    {
        eventSeat.Id
    };

        var command = new CreateReservationCommand(
            eventEntity.Id,
            selectedSeatIds);

        _currentUserServiceMock
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(customerId);

        _eventRepositoryMock
            .Setup(x => x.GetByIdAsync(eventEntity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        _eventSeatRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EventSeat>
            {
            eventSeat
            });

        var handler = CreateHandler();

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("One or more selected seats are not available.");

        _reservationRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenEventIsNotPublished()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        var eventEntity = new Event(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Draft Event",
            "Test Description",
            DateTime.UtcNow.AddDays(10),
            DateTime.UtcNow.AddDays(10).AddHours(2));

        // Publish çağırmıyoruz. Event Draft kalıyor.

        var command = new CreateReservationCommand(
            eventEntity.Id,
            new List<Guid> { Guid.NewGuid() });

        _currentUserServiceMock
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(customerId);

        _eventRepositoryMock
            .Setup(x => x.GetByIdAsync(eventEntity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        var handler = CreateHandler();

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Reservations can only be created for published events.");

        _reservationRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenEventDoesNotExist()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var command = new CreateReservationCommand(
            eventId,
            new List<Guid> { Guid.NewGuid() });

        _currentUserServiceMock
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(customerId);

        _eventRepositoryMock
            .Setup(x => x.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);

        var handler = CreateHandler();

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Event not found.");

        _reservationRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedException_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var command = new CreateReservationCommand(
            Guid.NewGuid(),
            new List<Guid> { Guid.NewGuid() });

        _currentUserServiceMock
            .Setup(x => x.IsAuthenticated)
            .Returns(false);

        var handler = CreateHandler();

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User is not authenticated.");

        _reservationRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreateReservation_AndLockSelectedSeats_WhenRequestIsValid()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();

        var eventEntity = new Event(
            venueId,
            organizerId,
            "Test Event",
            "Test Description",
            DateTime.UtcNow.AddDays(10),
            DateTime.UtcNow.AddDays(10).AddHours(2));

        eventEntity.Publish();

        var firstEventSeat = CreateEventSeatWithSeat(
            eventEntity.Id,
            section: "A",
            row: "1",
            number: 1,
            price: 1000m);

        var secondEventSeat = CreateEventSeatWithSeat(
            eventEntity.Id,
            section: "A",
            row: "1",
            number: 2,
            price: 750m);

        var selectedSeatIds = new List<Guid>
        {
            firstEventSeat.Id,
            secondEventSeat.Id
        };

        var command = new CreateReservationCommand(
            eventEntity.Id,
            selectedSeatIds);

        _currentUserServiceMock
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(customerId);

        _eventRepositoryMock
            .Setup(x => x.GetByIdAsync(eventEntity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        _eventSeatRepositoryMock
            .Setup(x => x.GetByIdsAsync(selectedSeatIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EventSeat>
            {
                firstEventSeat,
                secondEventSeat
            });

        Reservation? capturedReservation = null;

        _reservationRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()))
            .Callback<Reservation, CancellationToken>((reservation, _) =>
            {
                capturedReservation = reservation;
            })
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = CreateHandler();

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        response.CustomerId.Should().Be(customerId);
        response.EventId.Should().Be(eventEntity.Id);
        response.Status.Should().Be(ReservationStatus.PendingPayment);
        response.TotalAmount.Should().Be(1750m);
        response.Seats.Should().HaveCount(2);

        firstEventSeat.Status.Should().Be(EventSeatStatus.Locked);
        secondEventSeat.Status.Should().Be(EventSeatStatus.Locked);

        capturedReservation.Should().NotBeNull();
        capturedReservation!.CustomerId.Should().Be(customerId);
        capturedReservation.EventId.Should().Be(eventEntity.Id);
        capturedReservation.Status.Should().Be(ReservationStatus.PendingPayment);
        capturedReservation.TotalAmount.Should().Be(1750m);
        capturedReservation.ReservationSeats.Should().HaveCount(2);

        _reservationRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _reservationExpirationSchedulerMock.Verify(
            x => x.ScheduleExpiration(
                capturedReservation.Id,
                capturedReservation.ExpiresAt),
            Times.Once);

        _realtimeNotifierMock.Verify(
            x => x.NotifyEventSeatsChangedAsync(
                eventEntity.Id,
                It.Is<IReadOnlyList<EventSeatStatusChangedMessage>>(seats =>
                    seats.Count == 2 &&
                    seats.All(seat => seat.Status == EventSeatStatus.Locked)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private CreateReservationCommandHandler CreateHandler()
    {
        return new CreateReservationCommandHandler(
            _currentUserServiceMock.Object,
            _eventRepositoryMock.Object,
            _eventSeatRepositoryMock.Object,
            _reservationRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _reservationExpirationSchedulerMock.Object,
            _realtimeNotifierMock.Object);
    }

    private static EventSeat CreateEventSeatWithSeat(
        Guid eventId,
        string section,
        string row,
        int number,
        decimal price)
    {
        var seat = new Seat(
            Guid.NewGuid(),
            section,
            row,
            number);

        var eventSeat = new EventSeat(
            eventId,
            seat.Id,
            price);

        SetPrivateProperty(eventSeat, nameof(EventSeat.Seat), seat);

        return eventSeat;
    }

    private static void SetPrivateProperty<T>(
        T instance,
        string propertyName,
        object value)
    {
        var propertyInfo = typeof(T).GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        propertyInfo!.SetValue(instance, value);
    }
}