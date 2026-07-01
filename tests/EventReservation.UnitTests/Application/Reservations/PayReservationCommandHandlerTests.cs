using System.Reflection;
using EventReservation.Application.Abstractions.Authentication;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Abstractions.Realtime;
using EventReservation.Application.Common.Exceptions;
using EventReservation.Application.Features.Reservations.PayReservation;
using EventReservation.Domain.Entities;
using EventReservation.Domain.Enums;
using FluentAssertions;
using Moq;

namespace EventReservation.UnitTests.Application.Reservations;

public sealed class PayReservationCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<IReservationRepository> _reservationRepositoryMock = new();
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IRealtimeNotifier> _realtimeNotifierMock = new();

    [Fact]
    public async Task Handle_ShouldConfirmReservation_CreateSuccessfulPayment_AndReserveSeats_WhenRequestIsValid()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        var reservation = CreatePendingReservationWithSeats(
            customerId,
            expiresAt: DateTime.UtcNow.AddMinutes(10),
            firstSeatPrice: 1000m,
            secondSeatPrice: 750m);

        var command = new PayReservationCommand(
            reservation.Id,
            reservation.TotalAmount,
            PaymentMethod.CreditCard);

        _currentUserServiceMock
            .Setup(x => x.IsAuthenticated)
            .Returns(true);

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(customerId);

        _reservationRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        Payment? capturedPayment = null;

        _paymentRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Callback<Payment, CancellationToken>((payment, _) =>
            {
                capturedPayment = payment;
            })
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = CreateHandler();

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        reservation.Status.Should().Be(ReservationStatus.Confirmed);
        reservation.ConfirmedAt.Should().NotBeNull();

        reservation.ReservationSeats
            .Select(x => x.EventSeat.Status)
            .Should()
            .OnlyContain(status => status == EventSeatStatus.Reserved);

        capturedPayment.Should().NotBeNull();
        capturedPayment!.ReservationId.Should().Be(reservation.Id);
        capturedPayment.Amount.Should().Be(reservation.TotalAmount);
        capturedPayment.Method.Should().Be(PaymentMethod.CreditCard);
        capturedPayment.Status.Should().Be(PaymentStatus.Success);
        capturedPayment.PaidAt.Should().NotBeNull();

        response.ReservationId.Should().Be(reservation.Id);
        response.ReservationCode.Should().Be(reservation.ReservationCode);
        response.ReservationStatus.Should().Be(ReservationStatus.Confirmed);
        response.PaymentId.Should().Be(capturedPayment.Id);
        response.PaymentStatus.Should().Be(PaymentStatus.Success);
        response.Amount.Should().Be(reservation.TotalAmount);
        response.Method.Should().Be(PaymentMethod.CreditCard);
        response.PaidAt.Should().NotBeNull();

        _paymentRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _realtimeNotifierMock.Verify(
            x => x.NotifyEventSeatsChangedAsync(
                reservation.EventId,
                It.Is<IReadOnlyList<EventSeatStatusChangedMessage>>(seats =>
                    seats.Count == 2 &&
                    seats.All(seat => seat.Status == EventSeatStatus.Reserved)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenReservationDoesNotExist()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var reservationId = Guid.NewGuid();

        var command = new PayReservationCommand(
            reservationId,
            1000m,
            PaymentMethod.CreditCard);

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

        _paymentRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()),
            Times.Never);

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
            expiresAt: DateTime.UtcNow.AddMinutes(10),
            firstSeatPrice: 1000m,
            secondSeatPrice: 750m);

        var command = new PayReservationCommand(
            reservation.Id,
            reservation.TotalAmount,
            PaymentMethod.CreditCard);

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
            .WithMessage("You cannot pay another customer's reservation.");

        _paymentRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()),
            Times.Never);

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
            expiresAt: DateTime.UtcNow.AddMinutes(10),
            firstSeatPrice: 1000m,
            secondSeatPrice: 750m);

        reservation.Confirm();

        var command = new PayReservationCommand(
            reservation.Id,
            reservation.TotalAmount,
            PaymentMethod.CreditCard);

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
            .WithMessage("Only pending payment reservations can be paid.");

        _paymentRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenPaymentAlreadyExists()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        var reservation = CreatePendingReservationWithSeats(
            customerId,
            expiresAt: DateTime.UtcNow.AddMinutes(10),
            firstSeatPrice: 1000m,
            secondSeatPrice: 750m);

        var existingPayment = new Payment(
            reservation.Id,
            reservation.TotalAmount,
            PaymentMethod.CreditCard);

        SetPrivateProperty(reservation, nameof(Reservation.Payment), existingPayment);

        var command = new PayReservationCommand(
            reservation.Id,
            reservation.TotalAmount,
            PaymentMethod.CreditCard);

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
            .WithMessage("Payment already exists for this reservation.");

        _paymentRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldExpireReservation_ReleaseSeats_SaveChanges_AndThrowBadRequestException_WhenReservationExpired()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        var reservation = CreatePendingReservationWithSeats(
            customerId,
            expiresAt: DateTime.UtcNow.AddMinutes(-1),
            firstSeatPrice: 1000m,
            secondSeatPrice: 750m);

        var command = new PayReservationCommand(
            reservation.Id,
            reservation.TotalAmount,
            PaymentMethod.CreditCard);

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
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Reservation has expired.");

        reservation.Status.Should().Be(ReservationStatus.Expired);
        reservation.ExpiredAt.Should().NotBeNull();

        reservation.ReservationSeats
            .Select(x => x.EventSeat.Status)
            .Should()
            .OnlyContain(status => status == EventSeatStatus.Available);

        _paymentRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()),
            Times.Never);

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
    public async Task Handle_ShouldThrowBadRequestException_WhenAmountDoesNotMatchReservationTotalAmount()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        var reservation = CreatePendingReservationWithSeats(
            customerId,
            expiresAt: DateTime.UtcNow.AddMinutes(10),
            firstSeatPrice: 1000m,
            secondSeatPrice: 750m);

        var command = new PayReservationCommand(
            reservation.Id,
            1m,
            PaymentMethod.CreditCard);

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
            .WithMessage("Payment amount does not match reservation total amount.");

        _paymentRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private PayReservationCommandHandler CreateHandler()
    {
        return new PayReservationCommandHandler(
            _currentUserServiceMock.Object,
            _reservationRepositoryMock.Object,
            _paymentRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _realtimeNotifierMock.Object);
    }

    private static Reservation CreatePendingReservationWithSeats(
        Guid customerId,
        DateTime expiresAt,
        decimal firstSeatPrice,
        decimal secondSeatPrice)
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
            price: firstSeatPrice);

        var secondEventSeat = CreateLockedEventSeatWithSeat(
            section: "A",
            row: "1",
            number: 2,
            price: secondSeatPrice);

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
        var eventId = Guid.NewGuid();

        var seat = new Seat(
            Guid.NewGuid(),
            section,
            row,
            number);

        var eventSeat = new EventSeat(
            eventId,
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