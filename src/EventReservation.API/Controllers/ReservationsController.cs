using EventReservation.API.Contracts.Reservations;
using EventReservation.Application.Features.Reservations.CancelReservation;
using EventReservation.Application.Features.Reservations.CreateReservation;
using EventReservation.Application.Features.Reservations.GetMyReservations;
using EventReservation.Application.Features.Reservations.GetReservationById;
using EventReservation.Application.Features.Reservations.PayReservation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventReservation.API.Controllers;

[ApiController]
[Route("api/reservations")]
public sealed class ReservationsController : ControllerBase
{
    private readonly ISender _sender;

    public ReservationsController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Roles = "Customer")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateReservationRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateReservationCommand(
            request.EventId,
            request.EventSeatIds);

        var response = await _sender.Send(command, cancellationToken);
        return Ok(response);
    }

    [Authorize(Roles = "Customer")]
    [HttpPost("{reservationId:guid}/pay")]
    public async Task<IActionResult> Pay(Guid reservationId, PayReservationRequest request, CancellationToken cancellationToken)
    {
        var command = new PayReservationCommand(
            reservationId,
            request.Amount,
            request.Method);

        var response = await _sender.Send(command, cancellationToken);

        return Ok(response);
    }

    [Authorize(Roles = "Customer")]
    [HttpPost("{reservationId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid reservationId, CancellationToken cancellationToken)
    {
        var command = new CancelReservationCommand(reservationId);
        var response = await _sender.Send(command, cancellationToken);
        return Ok(response);
    }

    [Authorize(Roles = "Customer")]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyReservations(CancellationToken cancellationToken)
    {
        var query = new GetMyReservationsQuery();
        var response = await _sender.Send(query, cancellationToken);
        return Ok(response);
    }

    [Authorize(Roles = "Customer")]
    [HttpGet("{reservationId:guid}")]
    public async Task<IActionResult> GetById(Guid reservationId, CancellationToken cancellationToken)
    {
        var query = new GetReservationByIdQuery(reservationId);
        var response = await _sender.Send(query, cancellationToken);
        return Ok(response);
    }
}