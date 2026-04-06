public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(System.Collections.Generic.IEnumerable<double> values)
    {
        if (!System.Linq.Enumerable.Any(values))
            throw new System.ArgumentException("The input sequence is empty.");

        var count = System.Linq.Enumerable.Count(values);
        var mean = System.Linq.Enumerable.Average(values);
        var min = System.Linq.Enumerable.Min(values);
        var max = System.Linq.Enumerable.Max(values);

        var sorted = System.Linq.Enumerable.OrderBy(values, x => x).ToArray();

        double median;
        if (count % 2 == 1)
        {
            median = sorted[count / 2];
        }
        else
        {
            median = (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        }

        return (Mean: mean, Median: median, Min: min, Max: max);
    }
}