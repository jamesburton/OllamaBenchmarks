public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        var list = values.ToList();

        if (list.Count == 0)
        {
            throw new ArgumentException("Sequence is empty.");
        }

        double mean = System.Linq.Enumerable.Average(list);
        double min = System.Linq.Enumerable.Min(list);
        double max = System.Linq.Enumerable.Max(list);

        list.Sort();

        double median;
        int count = list.Count;
        int mid = count / 2;

        if (count % 2 == 0)
        {
            median = (list[mid - 1] + list[mid]) / 2.0;
        }
        else
        {
            median = list[mid];
        }

        return (mean, median, min, max);
    }
}