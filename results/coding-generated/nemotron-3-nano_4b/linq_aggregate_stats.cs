public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (!values.Any())
            throw new ArgumentException("The input sequence is empty.");

        double mean = values.Average();
        double min = values.Min();
        double max = values.Max();

        var sorted = values.OrderBy(x => x).Select(x => x).ToArray<double>();
        double median = sorted.Length % 2 == 1
            ? sorted[sorted.Length / 2]
            : (sorted[sorted.Length / 2 - 1] + sorted[sorted.Length / 2]) / 2;

        return (mean, median, min, max);
    }
}