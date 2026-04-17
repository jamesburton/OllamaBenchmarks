using System;
using System.Collections.Generic;

namespace Contracts
{
    public record ShipmentTracking(Guid ShipmentId, string Status, string Location);
}

public interface IConsumer<in T>
{
    void Consume(T message);
}

public class ShipmentTrackingConsumer : IConsumer<Contracts.ShipmentTracking>
{
    public List<string> StatusUpdates = new();

    public void Consume(Contracts.ShipmentTracking message)
    {
        StatusUpdates.Add($"{message.ShipmentId}: {message.Status} at {message.Location}");
    }
}