using System;
using System.Collections.Generic;
using System.Linq;

public record BookingRequest(string UserId, string ResourceId, DateTime StartTime, DateTime EndTime);

public record Booking(int Id, string UserId, string ResourceId, DateTime StartTime, DateTime EndTime);

public class BookingService
{
    private readonly List<Booking> _bookings = new List<Booking>();

    public int NextId { get; private set; } = 1;

    public Booking Book(BookingRequest request)
    {
        var booking = new Booking(NextId, request.UserId, request.ResourceId, request.StartTime, request.EndTime);
        _bookings.Add(booking);
        NextId++;
        return booking;
    }

    public List<Booking> GetForUser(string userId)
    {
        return _bookings.Where(b => b.UserId == userId).ToList();
    }

    public List<Booking> GetAll()
    {
        return new List<Booking>(_bookings);
    }
}