using FluentValidation;

namespace EventReservation.Application.Features.Reservations.PayReservation;

public sealed class PayReservationCommandValidator : AbstractValidator<PayReservationCommand>
{
    public PayReservationCommandValidator()
    {
        RuleFor(x => x.ReservationId)
            .NotEmpty()
            .WithMessage("Reservation id is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Method)
            .IsInEnum()
            .WithMessage("Payment method is invalid.");
    }
}