using EventReservation.API.Contracts.Events;
using EventReservation.Application.Features.Events.CancelEvent;
using EventReservation.Application.Features.Events.CompleteEvent;
using EventReservation.Application.Features.Events.CreateEvent;
using EventReservation.Application.Features.Events.DeleteEvent;
using EventReservation.Application.Features.Events.GetEventById;
using EventReservation.Application.Features.Events.GetEvents;
using EventReservation.Application.Features.Events.PublishEvent;
using EventReservation.Application.Features.Events.UpdateEvent;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventReservation.API.Controllers;

[ApiController]
[Route("api/events")]
public sealed class EventsController : ControllerBase
{
    private readonly ISender _sender;

    public EventsController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Roles = "Organizer,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateEventRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateEventCommand(
            request.VenueId,
            request.Title,
            request.Description,
            request.StartDate,
            request.EndDate);

        var response = await _sender.Send(command, cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetEventsQuery();
        var response = await _sender.Send(query, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetEventByIdQuery(id);
        var response = await _sender.Send(query, cancellationToken);
        return Ok(response);
    }

    [Authorize(Roles = "Organizer,Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateEventRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateEventCommand(
            id,
            request.Title,
            request.Description,
            request.StartDate,
            request.EndDate);

        var response = await _sender.Send(command, cancellationToken);
        return Ok(response);
    }

    [Authorize(Roles = "Organizer,Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteEventCommand(id);
        var response = await _sender.Send(command, cancellationToken);
        return Ok(response);
    }

    [Authorize(Roles = "Organizer,Admin")]
    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> Publish(Guid id, CancellationToken cancellationToken)
    {
        var command = new PublishEventCommand(id);
        var response = await _sender.Send(command, cancellationToken);
        return Ok(response);
    }

    [Authorize(Roles = "Organizer,Admin")]
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var command = new CancelEventCommand(id);
        var response = await _sender.Send(command, cancellationToken);
        return Ok(response);
    }

    [Authorize(Roles = "Organizer,Admin")]
    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        var command = new CompleteEventCommand(id);
        var response = await _sender.Send(command, cancellationToken);
        return Ok(response);
    }
}