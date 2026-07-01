using System.Reflection;
using EventReservation.Application.Abstractions.Authentication;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Abstractions.Realtime;
using EventReservation.Application.Common.Exceptions;
using EventReservation.Application.Features.Reservations.CancelReservation;
using EventReservation.Domain.Entities;
using EventReservation.Domain.Enums;
using FluentAssertions;
using Moq;

namespace EventReservation.UnitTests.Application.Reservations;

public sealed class CancelReservationCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<IReservationRepository> _reservationRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IRealtimeNotifier> _realtimeNotifierMock = new();

    [Fact]
    public async Task Handle_ShouldCancelReservation_AndReleaseSeats_WhenRequestIsValid()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        var reservation = CreatePendingReservationWithSeats(
            customerId,
            expiresAt: DateTime.UtcNow.AddMinutes(10));

        var command = new CancelReservationCommand(reservation.Id);

        _currentUserServiceMock
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(customerId);

        _reservationRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = CreateHandler();

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        reservation.Status.Should().Be(ReservationStatus.Cancelled);
        reservation.CancelledAt.Should().NotBeNull();
        reservation.UpdatedAt.Should().NotBeNull();

        reservation.ReservationSeats
            .Select(x => x.EventSeat.Status)
            .Should()
            .OnlyContain(status => status == EventSeatStatus.Available);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _realtimeNotifierMock.Verify(
            x => x.NotifyEventSeatsChangedAsync(
                reservation.EventId,
                It.Is<IReadOnlyList<EventSeatStatusChangedMessage>>(seats =>
                    seats.Count == 2 &&
                    seats.All(seat => seat.Status == EventSeatStatus.Available)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedException_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var command = new CancelReservationCommand(Guid.NewGuid());

        _currentUserServiceMock
            .Setup(x => x.IsAuthenticated)
            .Returns(false);

        var handler = CreateHandler();

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User is not authenticated.");

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenReservationDoesNotExist()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var reservationId = Guid.NewGuid();

        var command = new CancelReservationCommand(reservationId);

        _currentUserServiceMock
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(customerId);

        _reservationRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation?)null);

        var handler = CreateHandler();

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Reservation not found.");

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenAccessException_WhenReservationBelongsToAnotherCustomer()
    {
        // Arrange
        var currentCustomerId = Guid.NewGuid();
        var reservationOwnerId = Guid.NewGuid();

        var reservation = CreatePendingReservationWithSeats(
            reservationOwnerId,
            expiresAt: DateTime.UtcNow.AddMinutes(10));

        var command = new CancelReservationCommand(reservation.Id);

        _currentUserServiceMock
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(currentCustomerId);

        _reservationRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        var handler = CreateHandler();

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("You cannot cancel another customer's reservation.");

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenReservationIsNotPendingPayment()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        var reservation = CreatePendingReservationWithSeats(
            customerId,
            expiresAt: DateTime.UtcNow.AddMinutes(10));

        reservation.Confirm();

        var command = new CancelReservationCommand(reservation.Id);

        _currentUserServiceMock
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(customerId);

        _reservationRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        var handler = CreateHandler();

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Only pending payment reservations can be cancelled.");

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private CancelReservationCommandHandler CreateHandler()
    {
        return new CancelReservationCommandHandler(
            _currentUserServiceMock.Object,
            _reservationRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _realtimeNotifierMock.Object);
    }

    private static Reservation CreatePendingReservationWithSeats(
        Guid customerId,
        DateTime expiresAt)
    {
        var reservation = new Reservation(
            "RSV-TEST",
            customerId,
            Guid.NewGuid(),
            expiresAt);

        var firstEventSeat = CreateLockedEventSeatWithSeat(
            section: "A",
            row: "1",
            number: 1,
            price: 1000m);

        var secondEventSeat = CreateLockedEventSeatWithSeat(
            section: "A",
            row: "1",
            number: 2,
            price: 750m);

        reservation.AddSeat(firstEventSeat.Id, firstEventSeat.Price);
        reservation.AddSeat(secondEventSeat.Id, secondEventSeat.Price);

        var reservationSeats = reservation.ReservationSeats.ToList();

        SetPrivateProperty(
            reservationSeats[0],
            nameof(ReservationSeat.EventSeat),
            firstEventSeat);

        SetPrivateProperty(
            reservationSeats[1],
            nameof(ReservationSeat.EventSeat),
            secondEventSeat);

        return reservation;
    }

    private static EventSeat CreateLockedEventSeatWithSeat(
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
            Guid.NewGuid(),
            seat.Id,
            price);

        eventSeat.Lock();

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