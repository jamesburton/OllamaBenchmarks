public record Temperature(double Value, string Unit);

public static class TempConverter
{
    public static double ToCelsius(Temperature temp)
    {
        return temp.Unit.ToUpperInvariant() switch
        {
            "F" => (temp.Value - 32) * 5.0 / 9.0,
            "K" => temp.Value - 273.15,
            "C" => temp.Value,
            _ => temp.Value // Default case: return as-is for unknown units
        };
    }
}