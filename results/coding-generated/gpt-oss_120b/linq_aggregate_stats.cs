public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(System.Collections.Generic.IEnumerable<double> values)
    {
        if (values == null)
            throw new System.ArgumentNullException(nameof(values));

        var array = System.Linq.Enumerable.ToArray(values);
        if (array.Length == 0)
            throw new System.ArgumentException("Sequence contains no elements.");

        double mean = System.Linq.Enumerable.Average(array);
        double min = System.Linq.Enumerable.Min(array);
        double max = System.Linq.Enumerable.Max(array);

        var sorted = (double[])array.Clone();
        System.Array.Sort(sorted);
        double median;
        int n = sorted.Length;
        if (n % 2 == 1)
        {
            median = sorted[n / 2];
        }
        else
        {
            median = (sorted[n / 2 - 1] + sorted[n / 2]) / 2.0;
        }

        return (Mean: mean, Median: median, Min: min, Max: max);
    }
}