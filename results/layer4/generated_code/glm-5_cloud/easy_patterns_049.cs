using System;

public static class ArrayHelper
{
    public static int[] Merge(int[] a, int[] b)
    {
        if (a == null) throw new ArgumentNullException(nameof(a));
        if (b == null) throw new ArgumentNullException(nameof(b));

        int[] result = new int[a.Length + b.Length];
        int i = 0, j = 0, k = 0;

        // Traverse both arrays while elements exist in both
        while (i < a.Length && j < b.Length)
        {
            if (a[i] <= b[j])
            {
                result[k++] = a[i++];
            }
            else
            {
                result[k++] = b[j++];
            }
        }

        // Copy remaining elements from array a, if any
        while (i < a.Length)
        {
            result[k++] = a[i++];
        }

        // Copy remaining elements from array b, if any
        while (j < b.Length)
        {
            result[k++] = b[j++];
        }

        return result;
    }
}