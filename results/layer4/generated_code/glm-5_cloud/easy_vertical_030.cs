using System;
using System.Collections.Generic;
using System.Linq;

public record FlightRoute(string Origin, string Destination, decimal BaseFare);

public class FlightSearch
{
    private readonly List<FlightRoute> _routes = new();

    public void AddRoute(FlightRoute route)
    {
        _routes.Add(route);
    }

    public List<FlightRoute> FindRoutes(string origin, string destination)
    {
        return _routes
            .Where(r => r.Origin == origin && r.Destination == destination)
            .ToList();
    }

    public FlightRoute? CheapestRoute(string origin, string destination)
    {
        return _routes
            .Where(r => r.Origin == origin && r.Destination == destination)
            .OrderBy(r => r.BaseFare)
            .FirstOrDefault();
    }
}