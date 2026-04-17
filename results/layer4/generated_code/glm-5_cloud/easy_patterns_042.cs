using System;

public record Vector2D(double X, double Y)
{
    public double Magnitude => Math.Sqrt(X * X + Y * Y);

    public Vector2D Normalize()
    {
        double mag = Magnitude;
        if (mag == 0)
        {
            throw new InvalidOperationException("Cannot normalize a vector with zero magnitude.");
        }
        return new Vector2D(X / mag, Y / mag);
    }
}