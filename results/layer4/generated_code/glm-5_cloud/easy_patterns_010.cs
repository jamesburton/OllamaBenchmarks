using System;

public class Circle
{
    public double Radius { get; set; }
}

public class Rectangle
{
    public double Width { get; set; }
    public double Height { get; set; }
}

public static class ShapeDescriber
{
    public static string Describe(object shape)
    {
        return shape switch
        {
            Circle c => $"circle with radius {c.Radius}",
            Rectangle r => $"rectangle {r.Width}x{r.Height}",
            _ => "unknown shape"
        };
    }
}