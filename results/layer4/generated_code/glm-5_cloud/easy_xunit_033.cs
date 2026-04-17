using System;
using System.Collections.Generic;

public static class Primes
{
    public static bool IsPrime(int n)
    {
        if (n < 2) return false;
        if (n == 2) return true;
        if (n % 2 == 0) return false;

        int boundary = (int)Math.Floor(Math.Sqrt(n));

        for (int i = 3; i <= boundary; i += 2)
        {
            if (n % i == 0) return false;
        }

        return true;
    }

    public static List<int> GetPrimesUpTo(int max)
    {
        List<int> primes = new List<int>();

        if (max < 2) return primes;

        // Sieve of Eratosthenes
        bool[] isComposite = new bool[max + 1];

        for (int i = 2; i <= max; i++)
        {
            if (!isComposite[i])
            {
                primes.Add(i);

                // Mark multiples of i as composite
                // Start from i * i because smaller multiples would have been marked by smaller primes
                if (i <= Math.Sqrt(max))
                {
                    for (int j = i * i; j <= max; j += i)
                    {
                        isComposite[j] = true;
                    }
                }
            }
        }

        return primes;
    }
}