public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(System.Collections.Generic.IEnumerable<double> values)
    {
        var list = System.Linq.Enumerable.ToList(values);
        if (list.Count == 0)
            throw new System.ArgumentException("The input sequence is empty.");

        double mean = System.Linq.Enumerable.Average(list);
        double min = System.Linq.Enumerable.Min(list);
        double max = System.Linq.Enumerable.Max(list);

        list.Sort();
        int n = list.Count;
        double median;
        if (n % 2 == 1)
        {
            median = list[n / 2];
        }
        else
        {
            median = (list[n / 2 - 1] + list[n / 2]) / 2.0;
        }

        return (Mean: mean, Median: median, Min: min, Max: max);
    }
}