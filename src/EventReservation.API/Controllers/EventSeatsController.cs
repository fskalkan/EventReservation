using EventReservation.API.Contracts.EventSeats;
using EventReservation.Application.Features.EventSeats.GenerateEventSeats;
using EventReservation.Application.Features.EventSeats.GetEventSeats;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventReservation.API.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}/seats")]
public sealed class EventSeatsController : ControllerBase
{
    private readonly ISender _sender;

    public EventSeatsController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Roles = "Organizer,Admin")]
    [HttpPost("generate")]
    public async Task<IActionResult> Generate(Guid eventId, GenerateEventSeatsRequest request, CancellationToken cancellationToken)
    {
        var command = new GenerateEventSeatsCommand(
            eventId,
            request.DefaultPrice,
            request.SectionPrices);

        var response = await _sender.Send(command, cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetByEventId(Guid eventId, CancellationToken cancellationToken)
    {
        var query = new GetEventSeatsQuery(eventId);
        var response = await _sender.Send(query, cancellationToken);
        return Ok(response);
    }
}