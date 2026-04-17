public record Rectangle(double Width, double Height)
{
    public double Area => Width * Height;
}

public static class RectangleHelper
{
    public static bool IsSquare(Rectangle r) => r.Width == r.Height;
}