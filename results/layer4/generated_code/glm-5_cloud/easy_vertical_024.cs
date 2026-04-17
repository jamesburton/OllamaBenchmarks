using System;
using System.Collections.Generic;
using System.Linq;

public record Shipment(string TrackingNumber, string Status, DateTime EstimatedDelivery);

public class ShipmentTracker
{
    private readonly List<Shipment> _shipments = new();

    public void AddShipment(Shipment shipment)
    {
        _shipments.Add(shipment);
    }

    public Shipment? GetByTracking(string trackingNumber)
    {
        return _shipments.FirstOrDefault(s => s.TrackingNumber == trackingNumber);
    }

    public List<Shipment> GetDelayed(DateTime now)
    {
        return _shipments
            .Where(s => s.EstimatedDelivery < now && s.Status != "Delivered")
            .ToList();
    }

    public List<Shipment> GetAll()
    {
        return new List<Shipment>(_shipments);
    }
}