using System;
using System.Collections.Generic;

public static class ListExtensions
{
    private static readonly Random _random = new Random();

    public static List<T> Shuffle<T>(this List<T> list)
    {
        if (list == null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        // Create a copy of the original list to avoid modifying it
        List<T> shuffledList = new List<T>(list);
        int n = shuffledList.Count;

        // Fisher-Yates algorithm
        for (int i = n - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            T temp = shuffledList[i];
            shuffledList[i] = shuffledList[j];
            shuffledList[j] = temp;
        }

        return shuffledList;
    }

    public static T RandomElement<T>(this List<T> list)
    {
        if (list == null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        if (list.Count == 0)
        {
            throw new InvalidOperationException("The list is empty.");
        }

        int index = _random.Next(list.Count);
        return list[index];
    }
}