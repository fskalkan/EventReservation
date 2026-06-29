using EventReservation.Application.Features.EventReports.GetEventReservationSummary;
using EventReservation.Application.Features.EventReports.GetEventReservationsReport;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventReservation.API.Controllers;

[ApiController]
[Authorize(Roles = "Organizer,Admin")]
[Route("api/events/{eventId:guid}")]
public sealed class EventReportsController : ControllerBase
{
    private readonly ISender _sender;

    public EventReportsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("reservations")]
    public async Task<IActionResult> GetReservations(Guid eventId, CancellationToken cancellationToken)
    {
        var query = new GetEventReservationsReportQuery(eventId);
        var response = await _sender.Send(query, cancellationToken);
        return Ok(response);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(Guid eventId, CancellationToken cancellationToken)
    {
        var query = new GetEventReservationSummaryQuery(eventId);
        var response = await _sender.Send(query, cancellationToken);
        return Ok(response);
    }
}