using System;
using System.Collections.Generic;
using System.Linq;

public record EventRegistration(string EventId, string UserId, DateTime RegisteredAt);

public class EventRegistrar
{
    private readonly List<EventRegistration> _registrations;
    private readonly string _eventId;

    public int Capacity { get; }

    public EventRegistrar(string eventId, int capacity)
    {
        _eventId = eventId;
        Capacity = capacity;
        _registrations = new List<EventRegistration>();
    }

    public bool Register(string userId)
    {
        if (_registrations.Any(r => r.UserId == userId))
        {
            return false;
        }

        if (_registrations.Count >= Capacity)
        {
            return false;
        }

        var registration = new EventRegistration(_eventId, userId, DateTime.UtcNow);
        _registrations.Add(registration);
        return true;
    }

    public List<EventRegistration> GetRegistrations()
    {
        return new List<EventRegistration>(_registrations);
    }

    public int AvailableSpots => Capacity - _registrations.Count;
}