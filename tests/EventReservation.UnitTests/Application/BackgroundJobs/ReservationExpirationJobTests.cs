using System.Reflection;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Domain.Entities;
using EventReservation.Domain.Enums;
using EventReservation.Infrastructure.BackgroundJobs;
using FluentAssertions;
using Moq;

namespace EventReservation.UnitTests.Application.BackgroundJobs;

public sealed class ReservationExpirationJobTests
{
    private readonly Mock<IReservationRepository> _reservationRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task ExpireAsync_ShouldExpireReservation_ReleaseSeats_AndSaveChanges_WhenReservationIsPendingPaymentAndExpired()
    {
        // Arrange
        var reservation = CreatePendingReservationWithLockedSeats(
            expiresAt: DateTime.UtcNow.AddMinutes(-1));

        _reservationRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var job = CreateJob();

        // Act
        await job.ExpireAsync(reservation.Id);

        // Assert
        reservation.Status.Should().Be(ReservationStatus.Expired);
        reservation.ExpiredAt.Should().NotBeNull();
        reservation.UpdatedAt.Should().NotBeNull();

        reservation.ReservationSeats
            .Select(x => x.EventSeat.Status)
            .Should()
            .OnlyContain(status => status == EventSeatStatus.Available);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExpireAsync_ShouldDoNothing_WhenReservationDoesNotExist()
    {
        // Arrange
        var reservationId = Guid.NewGuid();

        _reservationRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation?)null);

        var job = CreateJob();

        // Act
        await job.ExpireAsync(reservationId);

        // Assert
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExpireAsync_ShouldDoNothing_WhenReservationIsNotPendingPayment()
    {
        // Arrange
        var reservation = CreatePendingReservationWithLockedSeats(
            expiresAt: DateTime.UtcNow.AddMinutes(-1));

        reservation.Confirm();

        _reservationRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        var job = CreateJob();

        // Act
        await job.ExpireAsync(reservation.Id);

        // Assert
        reservation.Status.Should().Be(ReservationStatus.Confirmed);

        reservation.ReservationSeats
            .Select(x => x.EventSeat.Status)
            .Should()
            .OnlyContain(status => status == EventSeatStatus.Locked);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExpireAsync_ShouldDoNothing_WhenReservationHasNotExpiredYet()
    {
        // Arrange
        var reservation = CreatePendingReservationWithLockedSeats(
            expiresAt: DateTime.UtcNow.AddMinutes(10));

        _reservationRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        var job = CreateJob();

        // Act
        await job.ExpireAsync(reservation.Id);

        // Assert
        reservation.Status.Should().Be(ReservationStatus.PendingPayment);

        reservation.ReservationSeats
            .Select(x => x.EventSeat.Status)
            .Should()
            .OnlyContain(status => status == EventSeatStatus.Locked);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private ReservationExpirationJob CreateJob()
    {
        return new ReservationExpirationJob(
            _reservationRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    private static Reservation CreatePendingReservationWithLockedSeats(DateTime expiresAt)
    {
        var reservation = new Reservation(
            "RSV-TEST",
            Guid.NewGuid(),
            Guid.NewGuid(),
            expiresAt);

        var firstEventSeat = CreateLockedEventSeat(
            price: 1000m);

        var secondEventSeat = CreateLockedEventSeat(
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

    private static EventSeat CreateLockedEventSeat(decimal price)
    {
        var seat = new Seat(
            Guid.NewGuid(),
            "A",
            "1",
            1);

        var eventSeat = new EventSeat(
            Guid.NewGuid(),
            seat.Id,
            price);

        eventSeat.Lock();

        SetPrivateProperty(
            eventSeat,
            nameof(EventSeat.Seat),
            seat);

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