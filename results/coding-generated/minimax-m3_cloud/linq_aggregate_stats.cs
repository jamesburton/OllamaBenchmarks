public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(System.Collections.Generic.IEnumerable<double> values)
    {
        if (!System.Linq.Enumerable.Any(values))
        {
            throw new System.ArgumentException("The input sequence is empty.");
        }

        double mean = System.Linq.Enumerable.Average(values);
        double min = System.Linq.Enumerable.Min(values);
        double max = System.Linq.Enumerable.Max(values);

        var sorted = System.Linq.Enumerable.ToList(System.Linq.Enumerable.OrderBy(values, x => x));
        int count = sorted.Count;
        double median;
        if (count % 2 == 1)
        {
            median = sorted[count / 2];
        }
        else
        {
            median = (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        }

        return (mean, median, min, max);
    }
}