public record Rgb(byte R, byte G, byte B);

public static class RgbHelper
{
    public static Rgb Invert(Rgb color)
    {
        return new Rgb(
            (byte)(255 - color.R),
            (byte)(255 - color.G),
            (byte)(255 - color.B)
        );
    }
}