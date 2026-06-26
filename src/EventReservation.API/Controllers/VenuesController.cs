using EventReservation.API.Contracts.Venues;
using EventReservation.Application.Features.Venues.CreateVenue;
using EventReservation.Application.Features.Venues.DeleteVenue;
using EventReservation.Application.Features.Venues.GetVenueById;
using EventReservation.Application.Features.Venues.GetVenues;
using EventReservation.Application.Features.Venues.UpdateVenue;
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

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetVenuesQuery();
        var response = await _sender.Send(query, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetVenueByIdQuery(id);
        var response = await _sender.Send(query, cancellationToken);
        return Ok(response);
    }


    [Authorize(Roles = "Organizer,Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateVenueRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateVenueCommand(
            id,
            request.Name,
            request.City,
            request.Address,
            request.Capacity);

        var response = await _sender.Send(command, cancellationToken);

        return Ok(response);
    }


    [Authorize(Roles = "Organizer,Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteVenueCommand(id);

        var response = await _sender.Send(command, cancellationToken);

        return Ok(response);
    }
}