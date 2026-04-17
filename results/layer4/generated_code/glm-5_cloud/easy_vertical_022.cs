public record Vehicle(string Plate, string Owner, string Type);

public class ParkingLot
{
    private readonly List<Vehicle> _parkedVehicles = new();

    public int Capacity { get; }

    public ParkingLot(int capacity)
    {
        Capacity = capacity;
    }

    public bool Enter(Vehicle vehicle)
    {
        if (_parkedVehicles.Count >= Capacity)
        {
            return false;
        }

        if (_parkedVehicles.Any(v => v.Plate == vehicle.Plate))
        {
            return false;
        }

        _parkedVehicles.Add(vehicle);
        return true;
    }

    public bool Exit(string plate)
    {
        var vehicle = _parkedVehicles.FirstOrDefault(v => v.Plate == plate);

        if (vehicle == null)
        {
            return false;
        }

        _parkedVehicles.Remove(vehicle);
        return true;
    }

    public int Available => Capacity - _parkedVehicles.Count;
}