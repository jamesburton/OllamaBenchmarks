public record Color(byte R, byte G, byte B);

public static class ColorHelper
{
    public static string ToHex(Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}