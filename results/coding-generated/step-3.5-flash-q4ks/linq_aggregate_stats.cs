public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (!System.Linq.Enumerable.Any(values))
        {
            throw new ArgumentException("Input sequence cannot be empty.");
        }

        double mean = System.Linq.Enumerable.Average(values);
        double min = System.Linq.Enumerable.Min(values);
        double max = System.Linq.Enumerable.Max(values);

        double[] array = System.Linq.Enumerable.ToArray(values);
        System.Array.Sort(array);

        int n = array.Length;
        double median;
        if (n % 2 == 1)
        {
            median = array[n / 2];
        }
        else
        {
            median = (array[n / 2 - 1] + array[n / 2]) / 2.0;
        }

        return (mean, median, min, max);
    }
}