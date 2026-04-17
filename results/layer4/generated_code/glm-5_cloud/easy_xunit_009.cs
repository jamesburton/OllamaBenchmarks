using System;

public interface ICalculator
{
    double Add(double a, double b);
    double Multiply(double a, double b);
}

public class Calculator : ICalculator
{
    public double Add(double a, double b)
    {
        return a + b;
    }

    public double Multiply(double a, double b)
    {
        return a * b;
    }
}