using EventReservation.Domain.Common;

namespace EventReservation.Domain.Entities;

public class Venue : BaseEntity
{
    public string Name { get; private set; } = null!;

    public string City { get; private set; } = null!;

    public string Address { get; private set; } = null!;

    public int Capacity { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public User CreatedByUser { get; private set; } = null!;

    public ICollection<Seat> Seats { get; private set; } = new List<Seat>();

    public ICollection<Event> Events { get; private set; } = new List<Event>();

    private Venue()
    {
    }

    public Venue(
        string name,
        string city,
        string address,
        int capacity,
        Guid createdByUserId)
    {
        Name = name;
        City = city;
        Address = address;
        Capacity = capacity;
        CreatedByUserId = createdByUserId;
    }

    public void UpdateDetails(
        string name,
        string city,
        string address,
        int capacity)
    {
        Name = name;
        City = city;
        Address = address;
        Capacity = capacity;

        MarkAsUpdated();
    }
}