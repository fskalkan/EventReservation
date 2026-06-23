using EventReservation.Domain.Common;
using EventReservation.Domain.Enums;

namespace EventReservation.Domain.Entities;

public class Event : BaseEntity
{
    public Guid VenueId { get; private set; }

    public Venue Venue { get; private set; } = null!;

    public Guid OrganizerId { get; private set; }

    public User Organizer { get; private set; } = null!;

    public string Title { get; private set; } = null!;

    public string Description { get; private set; } = null!;

    public DateTime StartDate { get; private set; }

    public DateTime EndDate { get; private set; }

    public EventStatus Status { get; private set; } = EventStatus.Draft;

    public ICollection<EventSeat> EventSeats { get; private set; } = new List<EventSeat>();

    private Event()
    {
    }

    public Event(
        Guid venueId,
        Guid organizerId,
        string title,
        string description,
        DateTime startDate,
        DateTime endDate)
    {
        VenueId = venueId;
        OrganizerId = organizerId;
        Title = title;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        Status = EventStatus.Draft;
    }

    public void UpdateDetails(
        string title,
        string description,
        DateTime startDate,
        DateTime endDate)
    {
        Title = title;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;

        MarkAsUpdated();
    }

    public void Publish()
    {
        Status = EventStatus.Published;
        MarkAsUpdated();
    }

    public void Cancel()
    {
        Status = EventStatus.Cancelled;
        MarkAsUpdated();
    }

    public void Complete()
    {
        Status = EventStatus.Completed;
        MarkAsUpdated();
    }
}