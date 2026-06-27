using EventReservation.API.Contracts.Seats;
using EventReservation.Application.Features.Seats.GenerateVenueSeats;
using EventReservation.Application.Features.Seats.GetVenueSeats;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventReservation.API.Controllers;

[ApiController]
[Route("api/venues/{venueId:guid}/seats")]
public sealed class VenueSeatsController : ControllerBase
{
    private readonly ISender _sender;

    public VenueSeatsController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Roles = "Organizer,Admin")]
    [HttpPost("generate")]
    public async Task<IActionResult> Generate(Guid venueId, GenerateVenueSeatsRequest request, CancellationToken cancellationToken)
    {
        var command = new GenerateVenueSeatsCommand(
            venueId,
            request.Section,
            request.Row,
            request.StartNumber,
            request.EndNumber);

        var response = await _sender.Send(command, cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetByVenueId(Guid venueId, CancellationToken cancellationToken)
    {
        var query = new GetVenueSeatsQuery(venueId);
        var response = await _sender.Send(query, cancellationToken);
        return Ok(response);
    }
}