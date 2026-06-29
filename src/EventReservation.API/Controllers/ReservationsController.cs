using EventReservation.API.Contracts.Reservations;
using EventReservation.Application.Features.Reservations.CreateReservation;
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
}