public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(System.Collections.Generic.IEnumerable<double> values)
    {
        if (values == null)
        {
            throw new System.ArgumentNullException(nameof(values));
        }

        var list = System.Linq.Enumerable.ToList(values);

        if (list.Count == 0)
        {
            throw new System.ArgumentException("The input sequence cannot be empty.");
        }

        double mean = System.Linq.Enumerable.Average(list);
        double min = System.Linq.Enumerable.Min(list);
        double max = System.Linq.Enumerable.Max(list);

        var sorted = System.Linq.Enumerable.OrderBy(list, x => x).ToList();
        int count = sorted.Count;
        double median;

        if (count % 2 == 0)
        {
            median = (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        }
        else
        {
            median = sorted[count / 2];
        }

        return (mean, median, min, max);
    }
}