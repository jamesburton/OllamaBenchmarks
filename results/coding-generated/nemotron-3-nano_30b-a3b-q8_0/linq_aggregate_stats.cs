public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (!System.Linq.Enumerable.Any(values))
            throw new ArgumentException("empty");

        double mean = System.Linq.Enumerable.Average(values);
        double min = System.Linq.Enumerable.Min(values);
        double max = System.Linq.Enumerable.Max(values);

        var sorted = System.Linq.Enumerable.OrderBy(values, (double x) => x);
        int count = System.Linq.Enumerable.Count(sorted);
        var array = System.Linq.Enumerable.ToArray(sorted);
        double median;
        if (count % 2 == 1)
        {
            median = array[count / 2];
        }
        else
        {
            median = (array[count / 2 - 1] + array[count / 2]) / 2.0;
        }

        return (mean, median, min, max);
    }
}