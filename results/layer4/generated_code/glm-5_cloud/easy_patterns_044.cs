using System;

public record Fraction(int Numerator, int Denominator)
{
    public double ToDouble() => (double)Numerator / Denominator;

    public static Fraction Simplify(Fraction f)
    {
        int gcd = GCD(f.Numerator, f.Denominator);
        return new Fraction(f.Numerator / gcd, f.Denominator / gcd);
    }

    private static int GCD(int a, int b)
    {
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }
}