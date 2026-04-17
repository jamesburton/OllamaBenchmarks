using System.Collections.Generic;

public static class NumberSequences
{
    public static IEnumerable<int> Range(int start, int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return start + i;
        }
    }

    public static IEnumerable<int> EvenNumbers(int count)
    {
        for (int i = 1; i <= count; i++)
        {
            yield return i * 2;
        }
    }
}