using System;

public static class ListPattern
{
    public static string DescribeList(int[] numbers)
    {
        return numbers switch
        {
            [] => "empty",
            [int x] => $"single: {x}",
            [int a, int b] => $"pair: {a}, {b}",
            _ => "many"
        };
    }
}