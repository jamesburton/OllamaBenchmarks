public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (!System.Linq.Enumerable.Any(values))
            throw new ArgumentException("Input sequence cannot be empty.");

        double mean = System.Linq.Enumerable.Average(values);
        double min = System.Linq.Enumerable.Min(values);
        double max = System.Linq.Enumerable.Max(values);

        var sorted = System.Linq.Enumerable.OrderBy(values, v => v).ToArray();
        int n = sorted.Length;
        double median;
        if (n % 2 == 1)
            median = sorted[n / 2];
        else
            median = (sorted[n / 2 - 1] + sorted[n / 2]) / 2.0;

        return (mean, median, min, max);
    }
}