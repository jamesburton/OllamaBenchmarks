public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (!values.Any())
            throw new ArgumentException("The input sequence is empty.");

        var sorted = values.ToList().Sort();

        double mean = values.Average();
        double min = values.Min();
        double max = values.Max();

        double median;
        if (sorted.Count % 2 == 1)
        {
            median = sorted[sorted.Count / 2];
        }
        else
        {
            median = (sorted[sorted.Count / 2 - 1] + sorted[sorted.Count / 2]) / 2.0;
        }

        return new (double Mean, double Median, double Min, double Max)
        {
            Mean = mean,
            Median = median,
            Min = min,
            Max = max
        };
}