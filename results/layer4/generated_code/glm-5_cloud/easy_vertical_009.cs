using System;
using System.Collections.Generic;
using System.Linq;

public record Sensor(string Id, string Location);

public record Reading(string SensorId, double Value, DateTime Timestamp);

public class SensorMonitor
{
    private readonly Dictionary<string, Sensor> _sensors = new();
    private readonly List<Reading> _readings = new();

    public void RegisterSensor(Sensor sensor)
    {
        if (sensor != null && !_sensors.ContainsKey(sensor.Id))
        {
            _sensors[sensor.Id] = sensor;
        }
    }

    public void RecordReading(Reading reading)
    {
        if (reading != null)
        {
            _readings.Add(reading);
        }
    }

    public double? GetLatestReading(string sensorId)
    {
        var latestReading = _readings
            .Where(r => r.SensorId == sensorId)
            .MaxBy(r => r.Timestamp);

        return latestReading?.Value;
    }

    public List<Reading> GetReadingsForSensor(string sensorId)
    {
        return _readings
            .Where(r => r.SensorId == sensorId)
            .ToList();
    }
}