using EventReservation.Application.Abstractions.Messaging;
using EventReservation.Application.Abstractions.Persistence;
using EventReservation.Application.Common.Exceptions;
using EventReservation.Application.Features.EventSeats.Common;
using EventReservation.Domain.Entities;
using EventReservation.Domain.Enums;

namespace EventReservation.Application.Features.EventSeats.GenerateEventSeats;

public sealed class GenerateEventSeatsCommandHandler
    : ICommandHandler<GenerateEventSeatsCommand, IReadOnlyList<EventSeatResponse>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly IEventSeatRepository _eventSeatRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GenerateEventSeatsCommandHandler(
        IEventRepository eventRepository,
        ISeatRepository seatRepository,
        IEventSeatRepository eventSeatRepository,
        IUnitOfWork unitOfWork)
    {
        _eventRepository = eventRepository;
        _seatRepository = seatRepository;
        _eventSeatRepository = eventSeatRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<EventSeatResponse>> Handle(GenerateEventSeatsCommand command, CancellationToken cancellationToken)
    {
        var eventEntity = await _eventRepository.GetByIdAsync(command.EventId, cancellationToken);

        if (eventEntity is null)
        {
            throw new NotFoundException("Event not found.");
        }

        if (eventEntity.Status != EventStatus.Draft)
        {
            throw new BadRequestException("Event seats can only be generated for draft events.");
        }

        var eventSeatsExist = await _eventSeatRepository.ExistsByEventIdAsync(command.EventId, cancellationToken);

        if (eventSeatsExist)
        {
            throw new BadRequestException("Event seats already generated for this event.");
        }

        var seats = await _seatRepository.GetForEventSeatGenerationByVenueIdAsync(eventEntity.VenueId, cancellationToken);

        if (seats.Count == 0)
        {
            throw new BadRequestException("Venue has no seats.");
        }

        var sectionPrices = command.SectionPrices
            .ToDictionary(
                x => x.Key.Trim(),
                x => x.Value,
                StringComparer.OrdinalIgnoreCase);

        var eventSeats = seats
            .Select(seat =>
            {
                var price = sectionPrices.TryGetValue(seat.Section.Trim(), out var sectionPrice)
                    ? sectionPrice
                    : command.DefaultPrice;

                return new EventSeat(
                    eventEntity.Id,
                    seat.Id,
                    price);
            })
            .ToList();

        await _eventSeatRepository.AddRangeAsync(eventSeats, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _eventSeatRepository.GetResponsesByEventIdAsync(eventEntity.Id, cancellationToken);
    }
}