using EventReservation.API.Contracts.Venues;
using EventReservation.Application.Features.Venues.CreateVenue;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventReservation.API.Controllers;

[ApiController]
[Route("api/venues")]
public sealed class VenuesController : ControllerBase
{
    private readonly ISender _sender;

    public VenuesController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Roles = "Organizer,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateVenueRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateVenueCommand(
            request.Name,
            request.City,
            request.Address,
            request.Capacity);

        var response = await _sender.Send(command, cancellationToken);

        return Ok(response);
    }
}