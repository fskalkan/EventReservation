using EventReservation.Domain.Entities;
using EventReservation.Domain.Enums;
using FluentAssertions;

namespace EventReservation.UnitTests.Domain.Payments;

public sealed class PaymentTests
{
    [Fact]
    public void Constructor_ShouldCreatePendingPayment()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var amount = 1750m;
        var method = PaymentMethod.CreditCard;

        // Act
        var payment = new Payment(
            reservationId,
            amount,
            method);

        // Assert
        payment.ReservationId.Should().Be(reservationId);
        payment.Amount.Should().Be(amount);
        payment.Method.Should().Be(method);
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.PaidAt.Should().BeNull();
        payment.FailureReason.Should().BeNull();
    }

    [Fact]
    public void MarkAsSuccess_ShouldSetStatusToSuccess_AndSetPaidAt()
    {
        // Arrange
        var payment = CreatePayment();

        // Act
        payment.MarkAsSuccess();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Success);
        payment.PaidAt.Should().NotBeNull();
        payment.FailureReason.Should().BeNull();
        payment.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsFailed_ShouldSetStatusToFailed_AndSetFailureReason()
    {
        // Arrange
        var payment = CreatePayment();
        var failureReason = "Insufficient balance.";

        // Act
        payment.MarkAsFailed(failureReason);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.FailureReason.Should().Be(failureReason);
        payment.PaidAt.Should().BeNull();
        payment.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Refund_ShouldSetStatusToRefunded_AndUpdateUpdatedAt()
    {
        // Arrange
        var payment = CreatePayment();
        payment.MarkAsSuccess();

        // Act
        payment.Refund();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Refunded);
        payment.UpdatedAt.Should().NotBeNull();
    }

    private static Payment CreatePayment()
    {
        return new Payment(
            Guid.NewGuid(),
            1750m,
            PaymentMethod.CreditCard);
    }
}