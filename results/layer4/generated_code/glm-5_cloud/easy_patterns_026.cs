public static class TupleHelper
{
    public static (int Sum, int Product) SumAndProduct(int a, int b)
    {
        return (a + b, a * b);
    }
}