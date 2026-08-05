public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (!values.Any()) throw new ArgumentException("The input sequence cannot be empty.");

        var mean = System.Linq.Enumerable.Average(values);
        var min = System.Linq.Enumerable.Min(values);
        var max = System.Linq.Enumerable.Max(values);
        var sorted = values.OrderBy(x => x).ToList();
        var count = sorted.Count;
        double median;
        if (count % 2 == 1)
            median = sorted[count / 2];
        else
            median = (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;

        return (mean, median, min, max);
    }
}