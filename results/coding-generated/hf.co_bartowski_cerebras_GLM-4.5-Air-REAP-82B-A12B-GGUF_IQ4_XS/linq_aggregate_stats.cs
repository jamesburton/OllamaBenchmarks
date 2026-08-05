public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (!values.Any())
            throw new ArgumentException("Input sequence must not be empty.");

        double mean = Enumerable.Average(values);
        double min = Enumerable.Min(values);
        double max = Enumerable.Max(values);

        var sorted = values.OrderBy(x => x).ToArray();
        int n = sorted.Length;
        double median = (n % 2 == 1) 
            ? sorted[n / 2] 
            : (sorted[n / 2 - 1] + sorted[n / 2]) / 2.0;

        return (mean, median, min, max);
    }
}