using EventReservation.Domain.Common;

namespace EventReservation.Domain.Entities;

public class Seat : BaseEntity
{
    public Guid VenueId { get; private set; }

    public Venue Venue { get; private set; } = null!;

    public string Section { get; private set; } = null!;

    public string Row { get; private set; } = null!;

    public int Number { get; private set; }

    public string Label => $"{Section}-{Row}-{Number}";

    private Seat()
    {
    }

    public Seat(
        Guid venueId,
        string section,
        string row,
        int number)
    {
        VenueId = venueId;
        Section = section;
        Row = row;
        Number = number;
    }

    public void UpdateLocation(
        string section,
        string row,
        int number)
    {
        Section = section;
        Row = row;
        Number = number;

        MarkAsUpdated();
    }
}