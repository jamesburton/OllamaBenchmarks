public record Rgb(byte R, byte G, byte B);

public static class ColorHelper
{
    public static bool IsDark(Rgb color)
    {
        int luminance = (color.R * 299 + color.G * 587 + color.B * 114) / 1000;
        return luminance < 128;
    }
}