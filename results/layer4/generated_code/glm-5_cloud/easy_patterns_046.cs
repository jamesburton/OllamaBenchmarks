using System;

public record Size(int Width, int Height);

public static class SizeHelper
{
    public static Size Scale(Size size, double factor)
    {
        int newWidth = (int)Math.Round(size.Width * factor);
        int newHeight = (int)Math.Round(size.Height * factor);
        return new Size(newWidth, newHeight);
    }
}