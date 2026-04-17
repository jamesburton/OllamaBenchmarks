using System;
using System.Collections.Generic;

namespace Contracts
{
    public record TemperatureReading(string SensorId, double Temperature, DateTime Timestamp);
}

public interface IConsumer<T>
{
    void Consume(T message);
}

public class TemperatureReadingConsumer : IConsumer<Contracts.TemperatureReading>
{
    public Dictionary<string, double> LatestReadings = new();

    public void Consume(Contracts.TemperatureReading message)
    {
        LatestReadings[message.SensorId] = message.Temperature;
    }
}