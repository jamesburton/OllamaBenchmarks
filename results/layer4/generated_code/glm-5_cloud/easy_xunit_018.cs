using System;
using System.Collections.Generic;

public static class ArrayUtils
{
    public static int[] Reverse(int[] arr)
    {
        if (arr == null)
        {
            return null;
        }

        int[] result = new int[arr.Length];
        for (int i = 0; i < arr.Length; i++)
        {
            result[i] = arr[arr.Length - 1 - i];
        }
        return result;
    }

    public static int[] Unique(int[] arr)
    {
        if (arr == null)
        {
            return null;
        }

        HashSet<int> seen = new HashSet<int>();
        List<int> uniqueList = new List<int>();

        foreach (int item in arr)
        {
            if (seen.Add(item))
            {
                uniqueList.Add(item);
            }
        }

        return uniqueList.ToArray();
    }
}